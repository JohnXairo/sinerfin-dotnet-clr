using System;
using System.Web.Http;
using System.Web.Mvc;
using Microsoft.Owin;
using Microsoft.Owin.Cors;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Owin;
using Sinerfin.Config;
using Sinerfin.Repositories;

[assembly: OwinStartup(typeof(Sinerfin.OwinStartup))]

namespace Sinerfin
{
    public class OwinStartup
    {
        public void Configuration(IAppBuilder app)
        {
            // ── Sesiones (equivalente a AddSession en .NET Core) ───────
            // En .NET FW las sesiones son nativas via System.Web.SessionState
            // Se configuran en Web.config de Views (sessionState)

            // ── CORS ──────────────────────────────────────────────────
            app.UseCors(CorsOptions.AllowAll);

            // ── Instanciar dependencias singleton ─────────────────────
            var settings = DbSettings.FromAppConfig();
            var factory  = new DbConnectionFactory(settings);
            var repo     = new MovimientoRepository(factory);

            // Registrar en el DependencyResolver de MVC
            DependencyResolver.SetResolver(
                new Infrastructure.SimpleMvcDependencyResolver(factory, repo));

            // ── MVC 5 ─────────────────────────────────────────────────
            AreaRegistration.RegisterAllAreas();
            MvcConfig.Register(System.Web.Routing.RouteTable.Routes);

            // ── Web API 2 (solo para /cliente JSON endpoint) ──────────
            var apiConfig = new HttpConfiguration();
            var json = apiConfig.Formatters.JsonFormatter;
            json.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            json.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
            apiConfig.Formatters.Remove(apiConfig.Formatters.XmlFormatter);
            apiConfig.MapHttpAttributeRoutes();
            apiConfig.Routes.MapHttpRoute(
                "ClienteApi", "cliente",
                new { controller = "Cliente" });

            // Resolver para Web API (ClienteController necesita MovimientoRepository)
            apiConfig.DependencyResolver =
                new Infrastructure.SimpleApiDependencyResolver(factory, repo);

            app.UseWebApi(apiConfig);
        }
    }
}
