using SoundPower.ErrorTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace TherapyCorner.Portal
{
    public class AntiForgeryHandleErrorAttribute : FilterAttribute, IAuthorizationFilter

    {
        public void OnAuthorization(AuthorizationContext filterContext)
        {
            bool shouldValidate = !filterContext
                .ActionDescriptor
                .GetCustomAttributes(typeof(ExcludeFromAntiForgeryValidationAttribute), true)
                .Any();
            if (shouldValidate)
            {
                try
                {
                    System.Web.Helpers.AntiForgery.Validate();
                }
                catch (HttpAntiForgeryException afe)
                {
                    var token = UserAuthorization.CurrentUser;
                    if (token == null)
                    {
                        filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new { controller = "profile", action = "Login" }));

                    }
                    else
                    {
                        Dictionary<string, string> context = new Dictionary<string, string>();
                        context.Add("User", token.UserId.ToString());
                        SoundPower.Web.Utilities.ReportError(new BaseException("Antiforgery Error", filterContext.HttpContext.Request.Url.ToString(), context));
                        filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new { controller = "error", action = "ForgeryError"}));

                    }
                }
                catch (Exception ex)
                {
                    throw new BaseException("AntiForgery Check Failure", filterContext.HttpContext.Request.Url.ToString(), ex);
                }
            }
        }


    }
    [AttributeUsage(AttributeTargets.Method)]
    public class ExcludeFromAntiForgeryValidationAttribute : Attribute
    {
    }
}