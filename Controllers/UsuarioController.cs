using System;
using System.Web.Mvc;
using Dapper;
using Sinerfin.Config;
using Sinerfin.Models;

namespace Sinerfin.Controllers
{
    public class UsuarioController : Controller
    {
        private readonly DbConnectionFactory _factory;

        public UsuarioController(DbConnectionFactory factory)
        {
            _factory = factory;
        }

        [HttpGet]
        public ActionResult Index()
        {
            if (Session["user"] == null) return Redirect("/login");

            DbProvider p;
            if (!Enum.TryParse(Session["dbType"] as string, out p)) return Redirect("/login");

            try
            {
                using (var conn = _factory.GetConnection(p))
                {
                    ViewBag.Lista = conn.Query(
                        "SELECT id, usuario, nombre, creado_en FROM " +
                        DbUtil.T(p, "usuarios") + " ORDER BY id DESC");
                }
            }
            catch (Exception ex) { ViewBag.Error = "Error al listar usuarios: " + ex.Message; }

            ViewBag.Ok    = Request.QueryString["ok"];
            ViewBag.Error = ViewBag.Error ?? Request.QueryString["error"];
            return View();
        }

        [HttpPost]
        public ActionResult Gestionar(
            string accion, string id,
            string usuario, string nombre,
            string password, string confirm)
        {
            if (Session["user"] == null) return Redirect("/login");

            DbProvider p;
            if (!Enum.TryParse(Session["dbType"] as string, out p)) return Redirect("/login");

            if (accion == "eliminar")
            {
                try
                {
                    using (var conn = _factory.GetConnection(p))
                        conn.Execute(
                            "DELETE FROM " + DbUtil.T(p, "usuarios") + " WHERE id = @id",
                            new { id = int.Parse(id) });
                    return Redirect("/usuarios?ok=eliminado");
                }
                catch (Exception ex)
                { return Redirect("/usuarios?error=" + Uri.EscapeDataString(ex.Message)); }
            }

            if (string.IsNullOrWhiteSpace(usuario) ||
                string.IsNullOrWhiteSpace(nombre)  ||
                string.IsNullOrWhiteSpace(password))
                return Redirect("/usuarios?error=Todos+los+campos+son+requeridos");

            if (password != confirm)
                return Redirect("/usuarios?error=Las+contrasenas+no+coinciden");

            if (password.Length < 8)
                return Redirect("/usuarios?error=La+contrasena+debe+tener+al+menos+8+caracteres");

            try
            {
                using (var conn = _factory.GetConnection(p))
                    conn.Execute(
                        "INSERT INTO " + DbUtil.T(p, "usuarios") +
                        " (usuario, password_hash, nombre) VALUES (@u, @h, @n)",
                        new { u = usuario.Trim(), h = PasswordUtil.Hash(password), n = nombre.Trim() });
                return Redirect("/usuarios?ok=creado");
            }
            catch (Exception ex)
            {
                var msg = ex.Message.IndexOf("unique", StringComparison.OrdinalIgnoreCase) >= 0
                    ? "El usuario ya existe" : ex.Message;
                return Redirect("/usuarios?error=" + Uri.EscapeDataString(msg));
            }
        }
    }
}
