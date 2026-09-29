using System;
using System.Web.Mvc;
using Dapper;
using Sinerfin.Config;
using Sinerfin.Models;

namespace Sinerfin.Controllers
{
    public class LoginController : Controller
    {
        private readonly DbConnectionFactory _factory;

        public LoginController(DbConnectionFactory factory)
        {
            _factory = factory;
        }

        [HttpGet]
        public ActionResult Index()
        {
            if (Session["user"] != null)
                return Redirect("/dashboard");
            return View(model: Request.QueryString["error"]);
        }

        [HttpPost]
        public ActionResult DoLogin(string db, string username, string password)
        {
            DbProvider provider;
            if (!Enum.TryParse(db, out provider))
            {
                System.Diagnostics.Trace.TraceWarning("Tipo BD invalido: {0}", db);
                return Redirect("/login?error=1");
            }

            try
            {
                Session["dbType"] = db;
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
                            Session["user"] = nombre;
                            return Redirect("/dashboard");
                        }
                    }
                }
                Session.Remove("dbType");
                return Redirect("/login?error=1");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Error BD login ({0}): {1}", db, ex.Message);
                Session.Remove("dbType");
                return Redirect("/login?error=2");
            }
        }

        [HttpGet]
        public ActionResult Logout()
        {
            Session.Clear();
            return Redirect("/login");
        }
    }
}
