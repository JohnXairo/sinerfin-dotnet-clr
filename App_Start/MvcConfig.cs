using System;
using System.Web.Mvc;
using System.Web.Routing;

namespace Sinerfin
{
    public static class MvcConfig
    {
        public static void Register(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            // Rutas explicitas (equivalente a los [HttpGet("/ruta")] de .NET Core)
            routes.MapRoute("Login",      "login",      new { controller = "Login",      action = "Index" });
            routes.MapRoute("DoLogin",    "login",      new { controller = "Login",      action = "DoLogin" },   new { httpMethod = new HttpMethodConstraint("POST") });
            routes.MapRoute("Logout",     "logout",     new { controller = "Login",      action = "Logout" });
            routes.MapRoute("Dashboard",  "dashboard",  new { controller = "Pages",      action = "Dashboard" });
            routes.MapRoute("Movimiento", "movimiento", new { controller = "Pages",      action = "Movimiento" });
            routes.MapRoute("DoMovimiento","movimiento",new { controller = "Movimiento", action = "Procesar" },  new { httpMethod = new HttpMethodConstraint("POST") });
            routes.MapRoute("Consulta",   "consulta",   new { controller = "Consulta",   action = "Index" });
            routes.MapRoute("Usuarios",   "usuarios",   new { controller = "Usuario",    action = "Index" });
            routes.MapRoute("DoUsuarios", "usuarios",   new { controller = "Usuario",    action = "Gestionar" }, new { httpMethod = new HttpMethodConstraint("POST") });

            // Ruta raiz -> login
            routes.MapRoute("Root", "", new { controller = "Login", action = "Index" });
        }
    }
}
