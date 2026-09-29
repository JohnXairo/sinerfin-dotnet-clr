using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Owin;
using Sinerfin.Config;
using Sinerfin.Models;
using Sinerfin.Repositories;

namespace Sinerfin.Infrastructure
{
    /// <summary>
    /// OWIN middleware that handles all HTML routes (login, dashboard, movimiento, consulta, usuarios).
    /// Completely replaces MVC 5 + Razor + System.Web.SessionState.
    /// </summary>
    public class AppRouter : OwinMiddleware
    {
        private readonly DbConnectionFactory _factory;
        private readonly MovimientoRepository _repo;

        public AppRouter(OwinMiddleware next, DbConnectionFactory factory, MovimientoRepository repo)
            : base(next)
        {
            _factory = factory;
            _repo    = repo;
        }

        public override async Task Invoke(IOwinContext ctx)
        {
            var req  = ctx.Request;
            var resp = ctx.Response;
            var path = req.Path.Value != null ? req.Path.Value.ToLowerInvariant() : "/";
            if (path == "") path = "/";

            // ── static: pass through to next middleware (Web API) ─────
            if (path.StartsWith("/cliente") || path.StartsWith("/api"))
            {
                await Next.Invoke(ctx);
                return;
            }

            string user, dbType;
            bool hasSession = SessionHelper.TryGetSession(req, out user, out dbType);

            resp.ContentType = "text/html; charset=utf-8";

            // ── GET /login ────────────────────────────────────────────
            if ((path == "/" || path == "/login") && req.Method == "GET")
            {
                if (hasSession) { Redirect(resp, "/dashboard"); return; }
                var error = req.Query["error"];
                await WriteHtml(resp, HtmlTemplates.Login(error));
                return;
            }

            // ── POST /login ───────────────────────────────────────────
            if (path == "/login" && req.Method == "POST")
            {
                var form = await ReadFormAsync(req);
                var db       = form["db"]       ?? "";
                var username = form["username"] ?? "";
                var password = form["password"] ?? "";

                Models.DbProvider provider;
                if (!Enum.TryParse(db, out provider))
                { Redirect(resp, "/login?error=1"); return; }

                try
                {
                    var tabla = DbUtil.T(provider, "usuarios");
                    using (var conn = _factory.GetConnection(provider))
                    {
                        var row = conn.QueryFirstOrDefault(
                            "SELECT usuario, password_hash, nombre FROM " + tabla + " WHERE usuario = @u",
                            new { u = username });

                        if (row != null)
                        {
                            string hash   = row.password_hash ?? "";
                            string nombre = row.nombre        ?? "";
                            if (PasswordUtil.Verify(password, hash))
                            {
                                SessionHelper.SetSession(resp, nombre, db);
                                Redirect(resp, "/dashboard");
                                return;
                            }
                        }
                    }
                    Redirect(resp, "/login?error=1");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.TraceError("Login error ({0}): {1}", db, ex.Message);
                    Redirect(resp, "/login?error=2");
                }
                return;
            }

            // ── GET /logout ───────────────────────────────────────────
            if (path == "/logout")
            {
                SessionHelper.ClearSession(resp);
                Redirect(resp, "/login");
                return;
            }

            // ── all routes below require session ──────────────────────
            if (!hasSession)
            {
                Redirect(resp, "/login");
                return;
            }

            // ── GET /dashboard ────────────────────────────────────────
            if (path == "/dashboard" && req.Method == "GET")
            {
                await WriteHtml(resp, HtmlTemplates.Dashboard(user, dbType));
                return;
            }

            // ── GET /movimiento ───────────────────────────────────────
            if (path == "/movimiento" && req.Method == "GET")
            {
                await WriteHtml(resp, HtmlTemplates.Movimiento(null, user, dbType));
                return;
            }

            // ── POST /movimiento ──────────────────────────────────────
            if (path == "/movimiento" && req.Method == "POST")
            {
                Models.DbProvider p;
                if (!Enum.TryParse(dbType, out p)) { Redirect(resp, "/login"); return; }

                var form   = await ReadFormAsync(req);
                var cedula = form["cedula"] ?? "";
                var nombre = form["nombre"] ?? "";
                var tipo   = form["tipo"]   ?? "DEPOSITO";
                var valorS = form["valor"]  ?? "";

                decimal monto;
                if (!decimal.TryParse(valorS,
                        System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture, out monto) || monto <= 0)
                {
                    await WriteHtml(resp, HtmlTemplates.Movimiento("El valor debe ser mayor a 0", user, dbType));
                    return;
                }
                try
                {
                    var numeroCuenta = _repo.ObtenerOCrearCuenta(p, cedula, nombre);
                    _repo.Guardar(p, new Movimiento
                    {
                        Cedula = cedula, Nombre = nombre,
                        NumeroCuenta = numeroCuenta, TipoMovimiento = tipo, Valor = monto
                    });
                    Redirect(resp, "/consulta?cedula=" + Uri.EscapeDataString(cedula));
                }
                catch (Exception ex)
                {
                    await WriteHtml(resp, HtmlTemplates.Movimiento(ex.Message, user, dbType));
                }
                return;
            }

            // ── GET /consulta ─────────────────────────────────────────
            if (path == "/consulta" && req.Method == "GET")
            {
                Models.DbProvider p;
                if (!Enum.TryParse(dbType, out p)) { Redirect(resp, "/login"); return; }

                var cedula = req.Query["cedula"];
                IEnumerable<Movimiento> lista       = null;
                string saldo = null, numeroCuenta = null, nombreCliente = null;

                if (!string.IsNullOrWhiteSpace(cedula))
                {
                    cedula = cedula.Trim();
                    lista  = _repo.BuscarPorCedula(p, cedula);
                    var cuenta = _repo.ObtenerCuentaPorCedula(p, cedula);
                    if (cuenta != null)
                    {
                        saldo         = cuenta.Saldo.ToString("F2");
                        numeroCuenta  = cuenta.NumeroCuenta;
                        nombreCliente = cuenta.Nombre;
                    }
                }
                await WriteHtml(resp,
                    HtmlTemplates.Consulta(user, dbType, cedula, lista, saldo, numeroCuenta, nombreCliente));
                return;
            }

            // ── GET /usuarios ─────────────────────────────────────────
            if (path == "/usuarios" && req.Method == "GET")
            {
                Models.DbProvider p;
                if (!Enum.TryParse(dbType, out p)) { Redirect(resp, "/login"); return; }

                IEnumerable<dynamic> lista = null;
                string errorMsg = null;
                try
                {
                    using (var conn = _factory.GetConnection(p))
                        lista = conn.Query(
                            "SELECT id, usuario, nombre, creado_en FROM " +
                            DbUtil.T(p, "usuarios") + " ORDER BY id DESC");
                }
                catch (Exception ex) { errorMsg = "Error al listar usuarios: " + ex.Message; }

                await WriteHtml(resp,
                    HtmlTemplates.Usuarios(user, dbType, lista, req.Query["ok"], errorMsg ?? req.Query["error"]));
                return;
            }

            // ── POST /usuarios ────────────────────────────────────────
            if (path == "/usuarios" && req.Method == "POST")
            {
                Models.DbProvider p;
                if (!Enum.TryParse(dbType, out p)) { Redirect(resp, "/login"); return; }

                var form   = await ReadFormAsync(req);
                var accion = form["accion"]   ?? "";
                var id     = form["id"]        ?? "";
                var usu    = form["usuario"]   ?? "";
                var nom    = form["nombre"]    ?? "";
                var pwd    = form["password"]  ?? "";
                var conf   = form["confirm"]   ?? "";

                if (accion == "eliminar")
                {
                    try
                    {
                        using (var conn = _factory.GetConnection(p))
                            conn.Execute("DELETE FROM " + DbUtil.T(p, "usuarios") + " WHERE id = @id",
                                new { id = int.Parse(id) });
                        Redirect(resp, "/usuarios?ok=eliminado");
                    }
                    catch (Exception ex)
                        { Redirect(resp, "/usuarios?error=" + Uri.EscapeDataString(ex.Message)); }
                    return;
                }

                if (string.IsNullOrWhiteSpace(usu) || string.IsNullOrWhiteSpace(nom) || string.IsNullOrWhiteSpace(pwd))
                { Redirect(resp, "/usuarios?error=Todos+los+campos+son+requeridos"); return; }
                if (pwd != conf)
                { Redirect(resp, "/usuarios?error=Las+contrasenas+no+coinciden"); return; }
                if (pwd.Length < 8)
                { Redirect(resp, "/usuarios?error=La+contrasena+debe+tener+al+menos+8+caracteres"); return; }

                try
                {
                    using (var conn = _factory.GetConnection(p))
                        conn.Execute(
                            "INSERT INTO " + DbUtil.T(p, "usuarios") +
                            " (usuario, password_hash, nombre) VALUES (@u, @h, @n)",
                            new { u = usu.Trim(), h = PasswordUtil.Hash(pwd), n = nom.Trim() });
                    Redirect(resp, "/usuarios?ok=creado");
                }
                catch (Exception ex)
                {
                    var msg = ex.Message.IndexOf("unique", StringComparison.OrdinalIgnoreCase) >= 0
                        ? "El usuario ya existe" : ex.Message;
                    Redirect(resp, "/usuarios?error=" + Uri.EscapeDataString(msg));
                }
                return;
            }

            // ── 404 ───────────────────────────────────────────────────
            resp.StatusCode = 404;
            await WriteHtml(resp, HtmlTemplates.Layout("404",
                "<div style='padding:60px;text-align:center;color:#64748b;'><h2>404 - No encontrado</h2></div>"));
        }

        // ── helpers ───────────────────────────────────────────────────
        private static void Redirect(IOwinResponse resp, string url)
        {
            resp.StatusCode = 302;
            resp.Headers["Location"] = url;
        }

        private static Task WriteHtml(IOwinResponse resp, string html)
        {
            resp.StatusCode = 200;
            var bytes = Encoding.UTF8.GetBytes(html);
            resp.ContentLength = (long)bytes.Length;   // explicit cast int -> long
            return resp.Body.WriteAsync(bytes, 0, bytes.Length);
        }

        private static async Task<NameValueCollection> ReadFormAsync(IOwinRequest req)
        {
            var form = new NameValueCollection();
            using (var sr = new StreamReader(req.Body, Encoding.UTF8, false, 4096, leaveOpen: true))
            {
                var body = await sr.ReadToEndAsync();
                if (string.IsNullOrEmpty(body)) return form;
                foreach (var pair in body.Split('&'))
                {
                    var idx = pair.IndexOf('=');
                    if (idx < 0) { form[Uri.UnescapeDataString(pair)] = ""; continue; }
                    var k = Uri.UnescapeDataString(pair.Substring(0, idx).Replace("+", " "));
                    var v = Uri.UnescapeDataString(pair.Substring(idx + 1).Replace("+", " "));
                    form[k] = v;
                }
            }
            return form;
        }
    }
}
