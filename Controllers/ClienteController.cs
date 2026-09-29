using System;
using System.Net.Http;
using System.Web.Http;
using Sinerfin.Config;
using Sinerfin.Models;
using Sinerfin.Repositories;

namespace Sinerfin.Controllers
{
    /// <summary>
    /// GET /cliente?cedula=xxx  — JSON endpoint for transaction form autocomplete.
    /// dbType is passed as query param because there is no HttpContext.Current in OWIN self-host.
    /// </summary>
    public class ClienteController : ApiController
    {
        private readonly DbConnectionFactory _factory;
        private readonly MovimientoRepository _repo;

        public ClienteController(DbConnectionFactory factory, MovimientoRepository repo)
        {
            _factory = factory;
            _repo    = repo;
        }

        [HttpGet]
        public HttpResponseMessage Get([FromUri] string cedula, [FromUri] string db)
        {
            if (string.IsNullOrWhiteSpace(cedula) || string.IsNullOrWhiteSpace(db))
                return Request.CreateResponse(System.Net.HttpStatusCode.BadRequest,
                    new { error = "cedula y db requeridos" });

            DbProvider p;
            if (!Enum.TryParse(db, out p))
                return Request.CreateResponse(System.Net.HttpStatusCode.BadRequest,
                    new { error = "db invalido" });

            try
            {
                var cuenta = _repo.ObtenerCuentaPorCedula(p, cedula.Trim());
                if (cuenta != null)
                    return Request.CreateResponse(System.Net.HttpStatusCode.OK, new
                    {
                        encontrado   = true,
                        nombre       = cuenta.Nombre,
                        numeroCuenta = cuenta.NumeroCuenta,
                        saldo        = cuenta.Saldo.ToString("F2")
                    });

                return Request.CreateResponse(System.Net.HttpStatusCode.OK,
                    new { encontrado = false });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(System.Net.HttpStatusCode.InternalServerError,
                    new { error = ex.Message });
            }
        }
    }
}
