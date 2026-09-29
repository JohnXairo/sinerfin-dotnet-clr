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
            // ── CORS ──────────────────────────────────────────────────
            app.UseCors(CorsOptions.AllowAll);

            // ── Dependencias singleton ────────────────────────────────
            var settings = DbSettings.FromAppConfig();
            var factory  = new DbConnectionFactory(settings);
            var repo     = new MovimientoRepository(factory);

            // ── MVC 5 DI ──────────────────────────────────────────────
            DependencyResolver.SetResolver(
                new Infrastructure.SimpleMvcDependencyResolver(factory, repo));

            // ── Rutas MVC 5 ───────────────────────────────────────────
            // NOTA: AreaRegistration.RegisterAllAreas() NO se puede llamar
            // en OWIN self-host — lanza InvalidOperationException.
            // No usamos Areas, se omite sin problema.
            MvcConfig.Register(System.Web.Routing.RouteTable.Routes);

            // ── Web API 2 (/cliente JSON endpoint) ────────────────────
            var apiConfig = new HttpConfiguration();
            var json = apiConfig.Formatters.JsonFormatter;
            json.SerializerSettings.ContractResolver  = new CamelCasePropertyNamesContractResolver();
            json.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
            apiConfig.Formatters.Remove(apiConfig.Formatters.XmlFormatter);
            apiConfig.MapHttpAttributeRoutes();
            apiConfig.Routes.MapHttpRoute(
                "ClienteApi", "cliente",
                new { controller = "Cliente" });

            apiConfig.DependencyResolver =
                new Infrastructure.SimpleApiDependencyResolver(factory, repo);

            app.UseWebApi(apiConfig);
        }
    }
}
