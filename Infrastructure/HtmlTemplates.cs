using System;
using System.Collections.Generic;
using System.Text;
using Sinerfin.Models;

namespace Sinerfin.Infrastructure
{
    public static class HtmlTemplates
    {
        private const string CSS = @"
*{box-sizing:border-box;margin:0;padding:0}
body{background:#0b1120;color:#e2e8f0;font-family:-apple-system,Segoe UI,sans-serif;font-size:15px;line-height:1.5}
input,select,button{display:block;width:100%;padding:10px 14px;margin-bottom:14px;
  background:#0f172a;border:1px solid #1e293b;border-radius:8px;color:#e2e8f0;font-size:14px}
button{background:#3b82f6;border:none;color:#fff;cursor:pointer;font-weight:600}
button:hover{background:#2563eb}
a{color:#3b82f6;text-decoration:none}
a:hover{text-decoration:underline}
.container{min-height:100vh;display:flex;justify-content:center;align-items:center}
.card{background:#111827;border:1px solid #1e293b;border-radius:14px;padding:36px;width:100%;max-width:480px}
.logo{font-size:28px;font-weight:700;color:#a3e635;margin-bottom:28px;text-align:center}
.alert-error{background:#3b0f14;color:#ef4444;border:1px solid #ef4444;border-radius:8px;
  padding:10px 14px;margin-bottom:16px;font-size:13px}
.alert-ok{background:#052e16;color:#22c55e;border:1px solid #22c55e;border-radius:8px;
  padding:10px 14px;margin-bottom:16px;font-size:13px}
.navbar{background:#111827;border-bottom:1px solid #1e293b;padding:10px 32px;
  display:flex;justify-content:space-between;align-items:center}
.navbar a{color:#94a3b8;margin-left:18px;font-size:13px}
.navbar a:hover{color:#e2e8f0}
.dashboard{padding:40px;text-align:center}
.dashboard h1{font-size:26px;font-weight:700;margin-bottom:12px}
.db-grid{display:flex;flex-wrap:wrap;gap:10px;justify-content:center;margin:16px 0}
.db-card{display:flex;flex-direction:column;align-items:center;background:#0f172a;
  border:2px solid #1e293b;border-radius:10px;padding:12px 18px;cursor:pointer;
  font-size:12px;transition:border-color .15s}
.db-card:hover,.db-card.selected{border-color:#3b82f6;color:#a3e635}
table{width:100%;border-collapse:collapse}
th{background:#1e293b;padding:10px 14px;text-align:left;font-size:13px}
td{padding:10px 14px;border-bottom:1px solid #1e293b;font-size:13px}
.saldo-card{background:#111827;border:1px solid #1e293b}
.table-container{overflow-x:auto}
";

        public static string Layout(string title, string body, bool navbar = false,
            string navUser = null, string navDb = null)
        {
            var sb = new StringBuilder();
            sb.Append("<!DOCTYPE html><html lang=\"es\"><head><meta charset=\"UTF-8\">");
            sb.AppendFormat("<title>{0} - Sinerfin</title>", HE(title));
            sb.AppendFormat("<style>{0}</style></head><body>", CSS);
            if (navbar)
            {
                sb.Append("<div class=\"navbar\"><div><b style=\"color:#a3e635;\">Sinerfin</b></div><div>");
                if (navDb != null)
                    sb.AppendFormat(
                        "<span style=\"font-size:12px;color:#64748b;\">Motor: <b style=\"color:#a3e635;\">{0}</b></span>&nbsp;&nbsp;",
                        HE(navDb));
                sb.Append("<a href=\"/dashboard\">Dashboard</a>");
                sb.Append("<a href=\"/movimiento\">Transacciones</a>");
                sb.Append("<a href=\"/consulta\">Consultas</a>");
                sb.Append("<a href=\"/usuarios\">Usuarios</a>");
                sb.Append("<a href=\"/logout\">Salir</a>");
                sb.Append("</div></div>");
            }
            sb.Append(body);
            sb.Append("</body></html>");
            return sb.ToString();
        }

        public static string Login(string error = null)
        {
            var alert = "";
            if (error == "1") alert = "<div class=\"alert-error\">Usuario o contrase&ntilde;a incorrectos.</div>";
            else if (error == "2") alert = "<div class=\"alert-error\">No se pudo conectar a la base de datos.</div>";

            var body = string.Format(@"
<div class='container'><div class='card'>
<div class='logo'>Sinerfin</div>
{0}
<form action='/login' method='post'>
  Usuario:<input name='username' autocomplete='username' required>
  Contrase&ntilde;a:<input type='password' name='password' autocomplete='current-password' required>
  <h4 style='margin-bottom:12px;'>Seleccionar Base de Datos</h4>
  <div class='db-grid'>
    <label class='db-card selected' onclick='sel(this)'><input type='radio' name='db' value='POSTGRES' checked hidden>PostgreSQL</label>
    <label class='db-card' onclick='sel(this)'><input type='radio' name='db' value='SQLSERVER' hidden>SQL Server</label>
    <label class='db-card' onclick='sel(this)'><input type='radio' name='db' value='SQLSERVER_DEV' hidden>SQL Server<br><span style='font-size:10px;color:#a3e635;'>Dev</span></label>
    <label class='db-card' onclick='sel(this)'><input type='radio' name='db' value='MYSQL' hidden>MySQL</label>
    <label class='db-card' onclick='sel(this)'><input type='radio' name='db' value='ORACLE' hidden>Oracle</label>
    <label class='db-card' onclick='sel(this)'><input type='radio' name='db' value='DB2' hidden>DB2</label>
  </div><br>
  <button>Ingresar</button>
</form>
</div></div>
<script>function sel(el){{document.querySelectorAll('.db-card').forEach(function(e){{e.classList.remove('selected');}});el.classList.add('selected')}}</script>
", alert);
            return Layout("Login", body);
        }

        public static string Dashboard(string user, string dbType)
        {
            var body = string.Format(@"
<div class='dashboard'>
  <h1>Core Bancario Demo</h1>
  <p style='color:#64748b;margin-bottom:8px;'>Bienvenido, <b style='color:white;'>{0}</b></p>
  <p style='margin-bottom:28px;'>Motor activo: <b style='color:#a3e635;'>{1}</b></p>
  <a href='/movimiento'><button style='width:250px;'>Realizar Transacci&oacute;n</button></a>
  <br><br>
  <a href='/consulta'><button style='width:250px;'>Consultar Movimientos</button></a>
</div>", HE(user), HE(dbType));
            return Layout("Dashboard", body, navbar: true, navUser: user, navDb: dbType);
        }

        public static string Movimiento(string errorMsg, string user, string dbType)
        {
            var alert = errorMsg != null
                ? "<div class='alert-error'>" + HE(errorMsg) + "</div>"
                : "";
            // Pass dbType in fetch URL so ClienteController can read it (no HttpContext.Current in self-host)
            var body = string.Format(@"
<div class='container'><div class='card'>
  <h2 style='margin-bottom:20px;'>Transacci&oacute;n</h2>
  {0}
  <div id='cliente-card' style='display:none;background:#0f172a;border:1px solid #1e293b;border-radius:10px;padding:14px 18px;margin-bottom:18px;'>
    <div style='font-size:11px;color:#64748b;margin-bottom:6px;'>Cliente encontrado</div>
    <div style='display:flex;gap:24px;align-items:center;flex-wrap:wrap;'>
      <div><div style='font-size:11px;color:#94a3b8;'>Titular</div><div id='cli-nombre' style='font-weight:bold;'></div></div>
      <div><div style='font-size:11px;color:#94a3b8;'>Cuenta</div><div id='cli-cuenta' style='font-family:monospace;'></div></div>
      <div><div style='font-size:11px;color:#94a3b8;'>Saldo</div><div id='cli-saldo' style='color:#a3e635;font-weight:bold;'></div></div>
    </div>
  </div>
  <form action='/movimiento' method='post'>
    <div style='position:relative;'>
      <input id='cedula' name='cedula' placeholder='C&eacute;dula' required autocomplete='off' style='padding-right:40px;'>
      <span id='cedula-status' style='position:absolute;right:14px;top:50%;transform:translateY(-50%);font-size:18px;display:none;'></span>
    </div>
    <input id='nombre' name='nombre' placeholder='Nombre' required>
    <select name='tipo'>
      <option value='DEPOSITO'>Dep&oacute;sito</option>
      <option value='RETIRO'>Retiro</option>
    </select>
    <input name='valor' placeholder='Valor' type='number' step='0.01' min='0.01' required>
    <button>Procesar</button>
  </form>
</div></div>
<script>
(function(){{var DB='{1}',ci=document.getElementById('cedula'),ni=document.getElementById('nombre'),
    st=document.getElementById('cedula-status'),cc=document.getElementById('cliente-card'),t=null;
  function reset(){{cc.style.display='none';ni.readOnly=false;ni.value='';ni.style.color='';st.style.display='none';}}
  function show(d){{document.getElementById('cli-nombre').textContent=d.nombre;
    document.getElementById('cli-cuenta').textContent=d.numeroCuenta;
    document.getElementById('cli-saldo').textContent='$'+d.saldo;
    cc.style.display='block';ni.value=d.nombre;ni.readOnly=true;ni.style.color='#64748b';
    st.textContent='\u2713';st.style.color='#22c55e';st.style.display='inline';}}
  ci.addEventListener('input',function(){{reset();clearTimeout(t);
    if(ci.value.trim().length>=6){{st.textContent='\u27f3';st.style.color='#94a3b8';st.style.display='inline';
      t=setTimeout(function(){{fetch('/cliente?cedula='+encodeURIComponent(ci.value.trim())+'&db='+DB)
        .then(function(r){{return r.json();}}).then(function(d){{d.encontrado?show(d):reset();}}).catch(reset);}},600);}}}});
}})();
</script>", alert, HE(dbType));
            return Layout("Transacci\u00f3n", body, navbar: true, navUser: user, navDb: dbType);
        }

        public static string Consulta(string user, string dbType, string cedula,
            IEnumerable<Movimiento> lista, string saldo, string numeroCuenta, string nombreCliente)
        {
            var sb = new StringBuilder();
            sb.Append("<div style='padding:32px 40px;'>");
            sb.Append("<h2 style='margin-bottom:24px;'>Consulta de Movimientos</h2>");
            sb.AppendFormat(
                "<form method='get' action='/consulta' style='display:flex;gap:12px;align-items:center;margin-bottom:28px;'>" +
                "<input name='cedula' placeholder='Ingresa la c&eacute;dula del cliente' value='{0}' style='width:320px;'>" +
                "<button style='width:140px;'>Buscar</button>",
                HE(cedula ?? ""));
            if (cedula != null) sb.Append("<a href='/consulta' style='color:#64748b;font-size:13px;'>Limpiar</a>");
            sb.Append("</form>");

            if (lista == null)
            {
                sb.Append("<div style='margin-top:80px;text-align:center;'>" +
                    "<div style='font-size:18px;font-weight:bold;color:#64748b;'>Ingresa una c&eacute;dula para consultar</div></div>");
            }
            else
            {
                if (saldo != null)
                {
                    sb.Append("<div style='display:flex;gap:16px;margin-bottom:28px;flex-wrap:wrap;'>");
                    sb.AppendFormat(
                        "<div class='saldo-card' style='padding:20px 28px;border-radius:12px;min-width:200px;'>" +
                        "<div style='font-size:12px;color:#94a3b8;'>Saldo actual</div>" +
                        "<div style='color:#a3e635;font-size:28px;font-weight:bold;'>${0}</div></div>", HE(saldo));
                    if (numeroCuenta != null)
                        sb.AppendFormat(
                            "<div class='saldo-card' style='padding:20px 28px;border-radius:12px;min-width:200px;'>" +
                            "<div style='font-size:12px;color:#94a3b8;'>N&uacute;mero de cuenta</div>" +
                            "<div style='font-size:20px;font-weight:bold;letter-spacing:2px;'>{0}</div></div>", HE(numeroCuenta));
                    if (nombreCliente != null)
                        sb.AppendFormat(
                            "<div class='saldo-card' style='padding:20px 28px;border-radius:12px;min-width:200px;'>" +
                            "<div style='font-size:12px;color:#94a3b8;'>Titular</div>" +
                            "<div style='font-size:18px;font-weight:bold;'>{0}</div></div>", HE(nombreCliente));
                    sb.Append("</div>");
                }

                var movs = new List<Movimiento>(lista);
                if (movs.Count > 0)
                {
                    sb.Append("<div class='table-container'><table>" +
                        "<tr><th>C&eacute;dula</th><th>Nombre</th><th>Cuenta</th><th>Tipo</th><th>Valor</th><th>Fecha</th></tr>");
                    foreach (var m in movs)
                    {
                        var bg    = m.TipoMovimiento == "DEPOSITO" ? "#052e16" : "#3b0f14";
                        var color = m.TipoMovimiento == "DEPOSITO" ? "#22c55e" : "#ef4444";
                        sb.AppendFormat(
                            "<tr><td>{0}</td><td>{1}</td><td style='font-family:monospace;letter-spacing:1px;'>{2}</td>" +
                            "<td><span style='padding:3px 10px;border-radius:20px;font-size:12px;font-weight:bold;background:{3};color:{4};'>{5}</span></td>" +
                            "<td style='font-weight:bold;'>${6}</td><td style='color:#94a3b8;font-size:13px;'>{7}</td></tr>",
                            HE(m.Cedula), HE(m.Nombre), HE(m.NumeroCuenta),
                            bg, color, HE(m.TipoMovimiento),
                            m.Valor.ToString("F2"),
                            m.Fecha.HasValue ? HE(m.Fecha.Value.ToString("g")) : "");
                    }
                    sb.Append("</table></div>");
                }
                else
                {
                    sb.AppendFormat(
                        "<div style='padding:40px;text-align:center;color:#64748b;'>" +
                        "No se encontraron movimientos para la c&eacute;dula <b>{0}</b>.</div>", HE(cedula));
                }
            }
            sb.Append("</div>");
            return Layout("Consultas", sb.ToString(), navbar: true, navUser: user, navDb: dbType);
        }

        public static string Usuarios(string user, string dbType,
            IEnumerable<dynamic> lista, string ok, string error)
        {
            var sb = new StringBuilder();
            sb.Append("<div style='padding:32px 40px;max-width:1100px;'>");
            sb.Append("<h2 style='margin-bottom:24px;'>Gesti&oacute;n de Usuarios</h2>");
            if (ok != null)
                sb.AppendFormat("<div class='alert-ok'>{0}</div>",
                    ok == "creado" ? "Usuario creado exitosamente." : "Usuario eliminado.");
            if (error != null)
                sb.AppendFormat("<div class='alert-error'>{0}</div>", HE(error));

            sb.Append("<div style='display:flex;gap:32px;flex-wrap:wrap;align-items:flex-start;'>");
            sb.Append(
                "<div class='saldo-card' style='padding:24px;border-radius:12px;min-width:320px;flex:1;'>" +
                "<h3 style='margin-bottom:20px;font-size:16px;'>Nuevo Usuario</h3>" +
                "<form action='/usuarios' method='post'>" +
                "<label style='font-size:12px;color:#94a3b8;'>Usuario (login)</label>" +
                "<input name='usuario' placeholder='ej: jperez' required style='margin-bottom:12px;'>" +
                "<label style='font-size:12px;color:#94a3b8;'>Nombre completo</label>" +
                "<input name='nombre' placeholder='ej: Juan P&eacute;rez' required style='margin-bottom:12px;'>" +
                "<label style='font-size:12px;color:#94a3b8;'>Contrase&ntilde;a</label>" +
                "<input type='password' name='password' placeholder='M&iacute;nimo 8 caracteres' required style='margin-bottom:12px;'>" +
                "<label style='font-size:12px;color:#94a3b8;'>Confirmar contrase&ntilde;a</label>" +
                "<input type='password' name='confirm' placeholder='Repetir contrase&ntilde;a' required style='margin-bottom:16px;'>" +
                "<button style='width:100%;'>Crear Usuario</button></form></div>");

            sb.Append("<div style='flex:2;min-width:400px;'><h3 style='margin-bottom:16px;font-size:16px;'>Usuarios Registrados</h3>");
            var rows = new List<dynamic>(lista ?? new List<dynamic>());
            if (rows.Count > 0)
            {
                sb.Append("<table><tr><th>#</th><th>Usuario</th><th>Nombre</th><th>Creado</th><th>Acci&oacute;n</th></tr>");
                foreach (var u in rows)
                {
                    // u.id may be long (Dapper), convert to string explicitly
                    string uid      = Convert.ToString(u.id);
                    string uusuario = HE(Convert.ToString(u.usuario));
                    string unombre  = HE(Convert.ToString(u.nombre));
                    string ucreado  = Convert.ToString(u.creado_en);
                    sb.AppendFormat(
                        "<tr><td style='color:#64748b;'>{0}</td><td style='font-weight:bold;'>{1}</td>" +
                        "<td>{2}</td><td style='color:#64748b;font-size:12px;'>{3}</td>" +
                        "<td style='text-align:center;'>" +
                        "<form action='/usuarios' method='post' style='margin:0;' onsubmit=\"return confirm('Eliminar usuario {1}?')\">" +
                        "<input type='hidden' name='accion' value='eliminar'>" +
                        "<input type='hidden' name='id' value='{0}'>" +
                        "<button style='background:#3b0f14;color:#ef4444;border:1px solid #ef4444;" +
                        "padding:4px 12px;border-radius:6px;font-size:12px;cursor:pointer;width:auto;'>Eliminar</button>" +
                        "</form></td></tr>",
                        uid, uusuario, unombre, ucreado);
                }
                sb.Append("</table>");
            }
            else
                sb.Append("<div style='padding:30px;text-align:center;color:#64748b;'>No hay usuarios registrados a&uacute;n.</div>");

            sb.Append("</div></div></div>");
            return Layout("Usuarios", sb.ToString(), navbar: true, navUser: user, navDb: dbType);
        }

        public static string HE(string s)
        {
            if (s == null) return "";
            return s.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;");
        }
    }
}
