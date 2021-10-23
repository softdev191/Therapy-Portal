using SoundPower.ErrorTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace TherapyCorner.Portal.Controllers
{
    public class ErrorController : Controller
    {
        // GET: Error
        public ActionResult Http404()
        {
            HttpContext.Response.StatusCode = 404;

            var token = UserAuthorization.CurrentUser;
            if (token != null)
            {
                Dictionary<string, string> context = new Dictionary<string, string>();
                context.Add("User", token.UserId.ToString());
                SoundPower.Web.Utilities.ReportError(new BaseException("404 Error", HttpContext.Request.Url.ToString(), context));
            }
                return View();
        }

        [ErrorHandlingFilter]
        public ActionResult Index()
        {

            var ex= new BaseException("Unhandled Error", Request.Url.ToString());
            SoundPower.Web.Utilities.ReportError(ex);

            return View(ex);
        }
        [ExcludeFromAntiForgeryValidationAttribute]
        public ActionResult ForgeryError()
        {
            return View();
        }
    }
}