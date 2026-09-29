using System;
using System.Web.Mvc;
using Sinerfin.Models;
using Sinerfin.Repositories;

namespace Sinerfin.Controllers
{
    public class ConsultaController : Controller
    {
        private readonly MovimientoRepository _repo;

        public ConsultaController(MovimientoRepository repo)
        {
            _repo = repo;
        }

        [HttpGet]
        public ActionResult Index(string cedula)
        {
            if (Session["user"] == null) return Redirect("/login");

            DbProvider p;
            if (!Enum.TryParse(Session["dbType"] as string, out p)) return Redirect("/login");

            if (!string.IsNullOrWhiteSpace(cedula))
            {
                cedula = cedula.Trim();
                ViewBag.Cedula = cedula;
                ViewBag.Lista  = _repo.BuscarPorCedula(p, cedula);
                var cuenta     = _repo.ObtenerCuentaPorCedula(p, cedula);
                if (cuenta != null)
                {
                    ViewBag.Saldo         = cuenta.Saldo.ToString("F2");
                    ViewBag.NumeroCuenta  = cuenta.NumeroCuenta;
                    ViewBag.NombreCliente = cuenta.Nombre;
                }
            }
            return View();
        }
    }
}
