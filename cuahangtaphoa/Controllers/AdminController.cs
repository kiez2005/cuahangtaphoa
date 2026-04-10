using cuahangtaphoa.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace cuahangtaphoa.Controllers
{
    [MyAuthorize(1)]
    public class AdminController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (Session["User"] == null || Convert.ToInt32(Session["MaVaiTro"]) != 1)
            {
                filterContext.Result = RedirectToAction("Index", "Login");
            }

            base.OnActionExecuting(filterContext);
        }
    }
}