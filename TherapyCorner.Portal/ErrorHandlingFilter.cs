using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace TherapyCorner.Portal
{
    public class ErrorHandlingFilter : System.Web.Mvc.HandleErrorAttribute
    {

        public override void OnException(System.Web.Mvc.ExceptionContext filterContext)
        {
            if ((filterContext.ExceptionHandled))
            {
                return;
            }

            System.Web.HttpContext context = System.Web.HttpContext.Current;
            SoundPower.ErrorTracking.BaseException ex = default(SoundPower.ErrorTracking.BaseException);
            if ((filterContext.Exception is SoundPower.ErrorTracking.BaseException))
            {
                ex = (SoundPower.ErrorTracking.BaseException)filterContext.Exception;

            }
            else
            {
                ex = new SoundPower.ErrorTracking.BaseException("An unexpected error occurred!", filterContext.Exception);
                if (context != null)
                {
                    ex.Data.Add("URL", context.Request.Url.ToString());
                
                }
            }
            if (context !=null)
            {
                if (context.Request.IsAuthenticated && context.User.Identity != null)
                {
                    var token = UserAuthorization.CurrentUser;
                    if (token !=null)
                    {
                    ex.Data.Add("UserSession", token.SessionId);
                        ex.Data.Add("UserId", token.UserId.ToString());
                        ex.Data.Add("Company", token.CurrentCompany ?? "");

                    }
                }
            }
            SoundPower.Web.Utilities.ReportError(ex);
            if (filterContext.HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                www.soundpower.biz.common.ResponseBase rsp = new www.soundpower.biz.common.ResponseBase(new List<string>(), true, null);
                rsp.ErrorMessages.Add(string.Format("An unexpected error occurred (#{0}).", ex.UniqueId.ToString()));
                filterContext.Result = new JsonResult()
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = rsp
                };
            }

            else
            {
                filterContext.Result = new ViewResult
                {
                    ViewName = filterContext.IsChildAction ? "ChildProcessedError" : "ProcessedError",
                    ViewData = new ViewDataDictionary<SoundPower.ErrorTracking.BaseException>(ex)
                    
                    
                };


            }
            filterContext.ExceptionHandled = true;
        }
    }
}