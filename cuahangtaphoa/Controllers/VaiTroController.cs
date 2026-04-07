using cuahangtaphoa.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace cuahangtaphoa.Controllers
{
    [MyAuthorize(1)]

    public class VaiTroController : Controller
    {
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (Session["User"] == null)
            {
                filterContext.Result = RedirectToAction("Index", "Login");
            }

            base.OnActionExecuting(filterContext);
        }



        public ActionResult Index()
        {
            return View();
        }
    }
}