using cuahangtaphoa.Filters;
using System.Web.Mvc;

namespace cuahangtaphoa.Controllers
{
    [CustomAuthorize(Roles = "1")]
    public class AdminController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}