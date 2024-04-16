using System.Web.Mvc;

namespace CNLCI.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult PaseDeSalida()
        {


            return View();
        }

        public ActionResult Contact()
        {


            return View();
        }

    }
}