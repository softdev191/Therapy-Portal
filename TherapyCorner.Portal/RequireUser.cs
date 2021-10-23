using System.Web.Mvc;
using System.Web.Routing;
using www.therapycorner.com.account;

namespace TherapyCorner.Portal
{
    public class RequireUserAttribute : FilterAttribute, IActionFilter
    {
        public void OnActionExecuted(ActionExecutedContext filterContext)
        {
            //do nothing after the action
            filterContext.Controller.ViewBag.CurUser = UserAuthorization.CurrentUser;
        }

        public void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var token = UserAuthorization.CurrentUser;
            if (token == null)
            {
                if (filterContext.RequestContext.HttpContext.Request.IsAjaxRequest())
                {
                    var rsp = new www.soundpower.biz.common.ResponseBase(new System.Collections.Generic.List<string>(), true, new System.Collections.Generic.List<string>());
                    rsp.ErrorMessages.Add(ResourceText.SharedPages.SessionEnded);
                    filterContext.Result = new JsonResult()
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                        Data = rsp
                    };
                }
                else
                {

                    string initialCompany = "";
                    if (!string.IsNullOrWhiteSpace(filterContext.RequestContext.HttpContext.Request.QueryString["ic"])) initialCompany = filterContext.RequestContext.HttpContext.Request.QueryString["ic"];
                    filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new { controller = "profile", action = "Login", returnUrl = filterContext.HttpContext.Request.Url.ToString(), ic=initialCompany }));
                   
                }
                filterContext.Result.ExecuteResult(filterContext.Controller.ControllerContext);
            }

            else if (!filterContext.RequestContext.HttpContext.Request.IsAjaxRequest())
            {
                if (token.ReqValidation )
                {
                    if (filterContext.ActionDescriptor.ActionName.ToLower() != "validate" && filterContext.ActionDescriptor.ActionName.ToLower() != "validateconfirm")
                    {
                        filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new { controller = "profile", action = "validate", returnUrl = filterContext.HttpContext.Request.Url.ToString() }));
                        filterContext.Result.ExecuteResult(filterContext.Controller.ControllerContext);
                    }
                }
                else if(token.ReqPassword  && filterContext.ActionDescriptor.ActionName.ToLower() != "passwordexpired" )
                {
                    filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new { controller = "profile", action = "passwordExpired", returnUrl = filterContext.HttpContext.Request.Url.ToString() }));
                    filterContext.Result.ExecuteResult(filterContext.Controller.ControllerContext);

                }
            }


        }
    }
}