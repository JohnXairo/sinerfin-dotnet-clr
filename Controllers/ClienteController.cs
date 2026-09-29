using System;
using System.Net.Http;
using System.Web.Http;
using Sinerfin.Models;
using Sinerfin.Repositories;

namespace Sinerfin.Controllers
{
    /// <summary>
    /// GET /cliente?cedula=xxx
    /// Endpoint JSON para autocompletar en el formulario de transaccion.
    /// Usa Web API 2 (ApiController) para retornar JSON directo.
    /// </summary>
    public class ClienteController : ApiController
    {
        private readonly MovimientoRepository _repo;

        public ClienteController(MovimientoRepository repo)
        {
            _repo = repo;
        }

        [HttpGet]
        public HttpResponseMessage Get([FromUri] string cedula)
        {
            var dbType = System.Web.HttpContext.Current?.Session?["dbType"] as string;
            if (dbType == null)
                return Request.CreateResponse(System.Net.HttpStatusCode.Unauthorized,
                    new { error = "sin sesion" });

            if (string.IsNullOrWhiteSpace(cedula))
                return Request.CreateResponse(System.Net.HttpStatusCode.BadRequest,
                    new { error = "cedula requerida" });

            DbProvider p;
            if (!Enum.TryParse(dbType, out p))
                return Request.CreateResponse(System.Net.HttpStatusCode.Unauthorized,
                    new { error = "sin sesion" });

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
