using System.Web.Http;
using Microsoft.Owin;
using Microsoft.Owin.Cors;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Owin;
using Sinerfin.Config;
using Sinerfin.Infrastructure;
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

            // ── HTML router middleware (reemplaza MVC 5 + Razor) ──────
            app.Use<AppRouter>(factory, repo);

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
                new SimpleApiDependencyResolver(factory, repo);

            app.UseWebApi(apiConfig);
        }
    }
}
