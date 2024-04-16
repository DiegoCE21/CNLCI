using CNLCI.Models;
using System;
using System.Linq;
using System.Web.Mvc;



namespace CNLCI.Controllers
{
    [Authorize]
    public class ReportesController : Controller
    {

        public ActionResult PorAlumno() { return View(); }

        public ActionResult PorAlumnoModal() { return View();  }

        [HttpPost]
        public ActionResult TraerDatos(string matricula, DateTime fechaInicial, DateTime fechaFinal)
        {
            using (DB db = new DB())
            {

                var response = db.RAJ(matricula, fechaInicial, fechaFinal).ToList();

                return Json(response, JsonRequestBehavior.AllowGet);
            }


        }
        public ActionResult PorGrupo() { return View(); }
        [HttpPost]
        public ActionResult TraerDatosG(DateTime fechaInicial, DateTime fechaFinal)
        {
            using (DB db = new DB())
            {

                var response = db.RGJ(fechaInicial, fechaFinal).ToList();

                return Json(response, JsonRequestBehavior.AllowGet);
            }


        }


        public ActionResult PorCarrera() { return View(); }

        [HttpPost]
        public ActionResult TraerDatosC(DateTime fechaInicial, DateTime fechaFinal)
        {
            using (DB db = new DB())
            {

                var response = db.RCJ(fechaInicial, fechaFinal).ToList();

                return Json(response, JsonRequestBehavior.AllowGet);
            }


        }


        public ActionResult PorMotivo() { return View(); }
        public ActionResult TraerDatosM(DateTime fechaInicial, DateTime fechaFinal)
        {
            using (DB db = new DB())
            {

                var response = db.RMJ(fechaInicial, fechaFinal).ToList();

                return Json(response, JsonRequestBehavior.AllowGet);
            }


        }




    }
}
