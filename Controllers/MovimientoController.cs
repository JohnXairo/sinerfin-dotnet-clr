using System;
using System.Web.Mvc;
using Sinerfin.Models;
using Sinerfin.Repositories;

namespace Sinerfin.Controllers
{
    public class MovimientoController : Controller
    {
        private readonly MovimientoRepository _repo;

        public MovimientoController(MovimientoRepository repo)
        {
            _repo = repo;
        }

        [HttpPost]
        public ActionResult Procesar(string cedula, string nombre, string tipo, string valor)
        {
            var dbType = Session["dbType"] as string;
            if (dbType == null) return Redirect("/login");

            DbProvider p;
            if (!Enum.TryParse(dbType, out p)) return Redirect("/login");

            decimal monto;
            if (!decimal.TryParse(valor,
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out monto) || monto <= 0)
            {
                return View("~/Views/Movimiento/Index.cshtml", model: "El valor debe ser mayor a 0");
            }

            try
            {
                var numeroCuenta = _repo.ObtenerOCrearCuenta(p, cedula, nombre);
                var m = new Movimiento
                {
                    Cedula         = cedula,
                    Nombre         = nombre,
                    NumeroCuenta   = numeroCuenta,
                    TipoMovimiento = tipo,
                    Valor          = monto
                };
                _repo.Guardar(p, m);
                return Redirect("/consulta?cedula=" + cedula);
            }
            catch (Exception ex)
            {
                return View("~/Views/Movimiento/Index.cshtml", model: ex.Message);
            }
        }
    }
}
