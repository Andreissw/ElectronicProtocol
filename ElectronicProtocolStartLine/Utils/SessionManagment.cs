using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ElectronicProtocolStartLine.Utils
{  

    public class SessionTimeoutAttribute : ActionFilterAttribute
    {
        //public override void OnActionExecuted(ActionExecutedContext filterContext)
        //{
        //    HttpContext httpContext = HttpContext.Current;
        //    if (HttpContext.Current.Session["userID"] == null)
        //    {
        //         filterContext.Result = new RedirectResult("~/Home/Index");
        //    }
        //    base.OnActionExecuted(filterContext);
        //}

    }
}