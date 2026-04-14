using cuahangtaphoa.Filters;
using System.Web.Mvc;

namespace cuahangtaphoa.Controllers
{
    [CustomAuthorize(Roles = "1")]
    public class VaiTroController : TrangChuController
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}