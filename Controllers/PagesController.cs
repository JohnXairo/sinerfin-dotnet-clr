using System;
using System.Web.Mvc;
using Sinerfin.Models;
using Sinerfin.Repositories;

namespace Sinerfin.Controllers
{
    public class PagesController : Controller
    {
        private bool HasSession() => Session["user"] != null;

        [HttpGet]
        public ActionResult Dashboard()
        {
            if (!HasSession()) return Redirect("/login");
            return View("~/Views/Dashboard/Index.cshtml");
        }

        [HttpGet]
        public ActionResult Movimiento()
        {
            if (!HasSession()) return Redirect("/login");
            return View("~/Views/Movimiento/Index.cshtml", model: (string)null);
        }
    }
}
