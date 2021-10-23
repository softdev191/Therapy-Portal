using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using www.soundpower.biz.common;
using www.therapycorner.com.company;
using www.therapycorner.com.company.MessageContracts;

namespace TherapyCorner.Portal.Controllers
{
    [RequireHttps]
    [RequireUser]
    [CompanyFilter]
    [RequireWorkerFilter]
    public class GuardianController : Controller
    {
        [HttpGet]
        public ActionResult Update(int? id)
        {
            if (!id.HasValue)
            {

                SoundPower.Web.Notifications.AddErrorNotification(ResourceText.SharedPages.CouldNotProcessRelogin);
                return RedirectToAction("index", "client");

            }

            Company.ClientServiceClient svcClient = null;
            GuardianInfoResponse rspClient = null;
            try
            {
                var token = UserAuthorization.CurrentUser;


                svcClient = new Company.ClientServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                rspClient = svcClient.Guardian(id.Value);
                if (rspClient.Guardian.IsPrimary)
                {
                    var rsp = svcClient.Details(int.Parse(rspClient.Guardian.Client.UniqueId));
                    ViewBag.CanDelete = (DateTime.Now.Subtract(rsp.Client.DoB.Value).TotalDays / 365 > 16);
                }
                else
                {
                    ViewBag.CanDelete = true;
                }

            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.Guardian.Update", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {
                if (svcClient != null) svcClient.Dispose();

            }

            SoundPower.Web.Notifications.AddResponseNotifications(rspClient);
            if (rspClient.IsFailure || rspClient.Guardian == null) return RedirectToAction("index");
            return View(rspClient.Guardian);
        }


        [HttpGet]
        public ActionResult Create(int? id)
        {
            if (!id.HasValue)
            {

                SoundPower.Web.Notifications.AddErrorNotification(ResourceText.SharedPages.CouldNotProcessRelogin);
                return RedirectToAction("index", "client");

            }

            Company.ClientServiceClient svcClient = null;
            ClientInfoResponse rspClient = null;
            try
            {
                var token = UserAuthorization.CurrentUser;


                svcClient = new Company.ClientServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                rspClient = svcClient.Details(id.Value);


            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.Guardian.Create", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {
                if (svcClient != null) svcClient.Dispose();

            }

            SoundPower.Web.Notifications.AddResponseNotifications(rspClient);
            if (rspClient.IsFailure || rspClient.Client == null) return RedirectToAction("index");
            return View(new GuardianInfo()
            {
                Client= rspClient.Client.ToPerson(),
                GuardianId=-1,
                Version="NEW",
                Address= new www.therapycorner.com.account.AddressInfo()
                
            });
        }

        [HttpPost]
        [AntiForgeryHandleError]
        public ActionResult Delete(int id, int client)
        {
   
                Company.ClientServiceClient svcClient = null;
                ResponseBase rsp = null;

                try
                {
                    var token = UserAuthorization.CurrentUser;


                    svcClient = new Company.ClientServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                    rsp = svcClient.RemoveGuardian(id);

                }
                catch (Exception ex)
                {
                    var bex = new SoundPower.ErrorTracking.BaseException("Save Processing Failure", "TherapyCorner.Portal.Controllers.Guardian.Delete", ex);
                    SoundPower.Web.Utilities.ReportError(bex);
                    throw bex;
                }
                finally
                {
                    if (svcClient != null) svcClient.Dispose();

                }

                SoundPower.Web.Notifications.AddResponseNotifications(rsp);

                if (rsp.IsFailure)
                {
                return RedirectToAction("update", new { id = id });
                }
                else
                {
                    SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.ClientPages.GuardianRemoved);
                    return RedirectToAction("details", "client", new { id = client });
                }
            
          
        }

        [HttpGet]
        public ActionResult Delete()
        {
            SoundPower.Web.Notifications.AddErrorNotification(ResourceText.SharedPages.CouldNotProcessRelogin);
            return RedirectToAction("index", "client");
        }

        [HttpPost]
        [AntiForgeryHandleError]
        public ActionResult Update(GuardianInfo model, bool canDelete)
        {
            if (ModelState.IsValid)
            {
                Company.ClientServiceClient svcClient = null;
                ResponseBase rsp = null;

                try
                {
                    var token = UserAuthorization.CurrentUser;


                    svcClient = new Company.ClientServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                    rsp = svcClient.UpdateGuardian(model);

                }
                catch (Exception ex)
                {
                    var bex = new SoundPower.ErrorTracking.BaseException("Save Processing Failure", "TherapyCorner.Portal.Controllers.Guardian.Update", ex);
                    SoundPower.Web.Utilities.ReportError(bex);
                    throw bex;
                }
                finally
                {
                    if (svcClient != null) svcClient.Dispose();

                }

                SoundPower.Web.Notifications.AddResponseNotifications(rsp);

                if (rsp.IsFailure)
                {
                    SiteUtilities.ApplyFieldIssues(rsp, ModelState);
                }
                else
                {
                    SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.ClientPages.GuardianUpdated);
                    return RedirectToAction("details", "client", new { id = model.Client.UniqueId });
                }
            }
            ViewBag.CanDelete = canDelete;
            return View(model);
        }

        [HttpPost]
        [AntiForgeryHandleError]
        public ActionResult Create(GuardianInfo model)
        {
            if (ModelState.IsValid)
            {
                Company.ClientServiceClient svcClient = null;
                ResponseBase rsp = null;

                try
                {
                    var token = UserAuthorization.CurrentUser;


                    svcClient = new Company.ClientServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                    rsp = svcClient.CreateGuardian(model);

                }
                catch (Exception ex)
                {
                    var bex = new SoundPower.ErrorTracking.BaseException("Save Processing Failure", "TherapyCorner.Portal.Controllers.Guardian.Create", ex);
                    SoundPower.Web.Utilities.ReportError(bex);
                    throw bex;
                }
                finally
                {
                    if (svcClient != null) svcClient.Dispose();

                }

                SoundPower.Web.Notifications.AddResponseNotifications(rsp);

                if (rsp.IsFailure)
                {
                    SiteUtilities.ApplyFieldIssues(rsp, ModelState);
                }
                else
                {
                    SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.ClientPages.GuardianCreated);
                    return RedirectToAction("details","client", new { id = model.Client.UniqueId });
                }
            }
            return View(model);
        }
    }
}