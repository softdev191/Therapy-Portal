using System;
using System.Web.Mvc;
using System.Web.Routing;
using www.therapycorner.com.account;
namespace TherapyCorner.Portal
{
    public class CompanyFilter : FilterAttribute, IActionFilter
    {
        public void OnActionExecuted(ActionExecutedContext filterContext)
        {
            //do nothing after the action
            filterContext.Controller.ViewBag.CurUser = UserAuthorization.CurrentUser;
        }

        public void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var token = UserAuthorization.CurrentUser;
            if (token.Companies.Count==0)
            {
                if (filterContext.RequestContext.HttpContext.Request.IsAjaxRequest())
                {
                    var rsp = new www.soundpower.biz.common.ResponseBase(new System.Collections.Generic.List<string>(), true, new System.Collections.Generic.List<string>());
                    rsp.ErrorMessages.Add(ResourceText.SharedPages.CompanyRequired);
                    filterContext.Result = new JsonResult()
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                        Data = rsp
                    };
                }
                else
                {
                    filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new { controller = "profile", action = "index" }));

                }
                filterContext.Result.ExecuteResult(filterContext.Controller.ControllerContext);
            }

        


        }
    }
}