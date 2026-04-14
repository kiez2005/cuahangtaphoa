using System;
using System.Web.Mvc;
using cuahangtaphoa.Models;

namespace cuahangtaphoa.Filters
{
    public class CustomAuthorizeAttribute : ActionFilterAttribute
    {
        public string Roles { get; set; }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var user = filterContext.HttpContext.Session["user"] as NguoiDung;

            if (user == null)
            {
                filterContext.Result = new RedirectToRouteResult(
                    new System.Web.Routing.RouteValueDictionary(
                        new { controller = "Login", action = "Index" }));
                return;
            }

            if (!string.IsNullOrEmpty(Roles))
            {
                var allowedRoles = Roles.Split(',');
                if (!Array.Exists(allowedRoles, r => r.Trim() == user.MaVaiTro.ToString()))
                {
                    filterContext.Result = new RedirectToRouteResult(
                        new System.Web.Routing.RouteValueDictionary(
                            new { controller = "TrangChu", action = "KhongCoDuocPhep" }));
                    return;
                }
            }

            base.OnActionExecuting(filterContext);
        }
    }
}