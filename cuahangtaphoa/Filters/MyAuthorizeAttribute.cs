using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace cuahangtaphoa.Filters
{
    public class MyAuthorize : ActionFilterAttribute
    {
        private int[] _roles;

        public MyAuthorize(params int[] roles)
        {
            _roles = roles;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var session = filterContext.HttpContext.Session["MaVaiTro"];

            if (session == null)
            {
                filterContext.Result = new RedirectResult("/Login/Index");
                return;
            }

            int role = (int)session;
            if (!_roles.Contains(role))
            {
                filterContext.Result = new RedirectResult("/Login/Index");
                return;
            }

            base.OnActionExecuting(filterContext);
        }
    }
}