using SoundPower.ErrorTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using www.soundpower.biz.common;
using www.therapycorner.com.account.MessageContracts;

namespace TherapyCorner.Portal.Controllers
{
    [RequireHttps]
    public class FileShareController : Controller
    {
        [HttpGet]
        public ActionResult Index(string id)
        {
            Account.FileShareServiceClient svc = null;
            FileShareResponse rsp = null;
            try
            {
                svc = new Account.FileShareServiceClient(SiteUtilities.AccountService, UserAuthorization.CurrentUser, SiteUtilities.CurrentCulture);
                rsp = svc.Details(id);


            }

            catch (Exception ex)
            {

                BaseException iex = new BaseException("Processing Failure", "TherapyCorner.Controllers.FileShare.Index", ex);
                SoundPower.Web.Utilities.ReportError(iex);
                throw iex;
            }
            finally
            {
                if (svc != null) svc.Dispose();
            }

            SoundPower.Web.Notifications.AddResponseNotifications(rsp);
            if (!rsp.IsFailure)
            {
                ViewBag.Email = rsp.Share.Email;
                ViewBag.Mobile = string.IsNullOrWhiteSpace(rsp.Share.Mobile) ? null : rsp.Share.Mobile.Substring(rsp.Share.Mobile.Length-4) ;
            }
        

            return View(new CreateVerificationRequest { Id = id });
        }

        [HttpPost]
        public ActionResult Index(CreateVerificationRequest model,string email, string mobile)
        {
            if (ModelState.IsValid)
            {

                Account.VerificationClientService svc = null;
                ObjectIdResponseBase rsp = null;
                try
                {
                    svc = new Account.VerificationClientService(SiteUtilities.AccountService, UserAuthorization.CurrentUser, SiteUtilities.CurrentCulture);

                    rsp = svc.CreateShare(model);


                }

                catch (Exception ex)
                {

                    BaseException iex = new BaseException("Start Processing Failure", "TherapyCorner.Controllers.FileShare.Index", ex);
                    SoundPower.Web.Utilities.ReportError(iex);
                    throw iex;
                }
                finally
                {
                    if (svc != null) svc.Dispose();
                }

                SoundPower.Web.Notifications.AddResponseNotifications(rsp);
                if (!rsp.IsFailure)
                {

                    return View("validate", new Models.ValidationCode() { ReturnURL=model.Id, ByEmail = model.ByEmail,  ValidationId = int.Parse(rsp.ObjectId) });
                }
                else
                {
                    SiteUtilities.ApplyFieldIssues(rsp, ModelState);

                }

            }
            ViewBag.Email = email;
            ViewBag.Mobile = mobile;

            return View(model);
        }

        [HttpPost]
        public ActionResult Validate(Models.ValidationCode model)
        {
            if (ModelState.IsValid)
            {

                Account.FileShareServiceClient svc = null;
                FileResponse rsp = null;
                try
                {
                    svc = new Account.FileShareServiceClient(SiteUtilities.AccountService, UserAuthorization.CurrentUser, SiteUtilities.CurrentCulture);

                    rsp = svc.File(new VerificationRequest() { Code = model.Code, Id = model.ValidationId, UserId = 0, DeviceCode = model.ReturnURL });

                 
                }

                catch (Exception ex)
                {

                    BaseException iex = new BaseException("Processing Failure", "TherapyCorner.Controllers.FileShare.Validate", ex);
                    SoundPower.Web.Utilities.ReportError(iex);
                    throw iex;
                }
                finally
                {
                    if (svc != null) svc.Dispose();
                }



                SoundPower.Web.Notifications.AddResponseNotifications(rsp);
                if (!rsp.IsFailure)
                {
                    return new FileContentResult(rsp.FileData, rsp.FileType);
                }
                else
                {
                    SiteUtilities.ApplyFieldIssues(rsp, ModelState);

                }
            }

            return View(model);
        }
    }
}