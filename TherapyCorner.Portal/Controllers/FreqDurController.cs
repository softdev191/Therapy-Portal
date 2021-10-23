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
    public class FreqDurController : Controller
    {
        [RequireAdmin]
        [CompanyFilter]
        public ActionResult Index()
        {
            FreqDurInfoListResponse rsp = null;
            Company.FreqDurServiceClient svcComp = null;
            try
            {
                var token = UserAuthorization.CurrentUser;

                svcComp = new Company.FreqDurServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                rsp = svcComp.List(false);
                SoundPower.Web.Notifications.AddResponseNotifications(rsp);
            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.FreqDur.Index", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {
                if (svcComp != null) svcComp.Dispose();
            }


            if (rsp.IsFailure)
            {
                return RedirectToAction("index", "home");

            }
            rsp.Durations.Sort((a, b) => a.Name.CompareTo(b.Name));
            return View(rsp.Durations);
        }

        [RequireAdmin]
        public ActionResult Remove(int rateId)
        {
            ResponseBase rsp = null;
            Company.FreqDurServiceClient svcComp = null;
            try
            {
                var token = UserAuthorization.CurrentUser;

                svcComp = new Company.FreqDurServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                rsp = svcComp.Remove(rateId);
                SoundPower.Web.Notifications.AddResponseNotifications(rsp);
            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.FreqDur.Remove", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {
                if (svcComp != null) svcComp.Dispose();
            }


            if (!rsp.IsFailure)
            {
                SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.FreqDurPages.RemoveSuccess);

            }
            return RedirectToAction("index");
        }

        [RequireAdmin]
        [HttpGet]
        public ActionResult Create()
        {
        

                return View(new FreqDurInfo() { FreqDurId = -1, IsActive = true , Duration=60, PerWeek=1, Weeks=52, Disciplines=new GenericEntityList()});
        }

   
        [RequireAdmin]
        [HttpPost]
        [AntiForgeryHandleError]
        public ActionResult Create(FreqDurInfo model,int[] disciplineId)
        {
            model.Disciplines = SiteUtilities.ArrayToGenericEntityList(disciplineId, "Discipline");
       if (ModelState.IsValid)
            {
                ResponseBase rsp = null;
                Company.FreqDurServiceClient svcComp = null;
                try
                {
                    var token = UserAuthorization.CurrentUser;

                    svcComp = new Company.FreqDurServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                    rsp = svcComp.Create(model);
                    SoundPower.Web.Notifications.AddResponseNotifications(rsp);
                }
                catch (Exception ex)
                {
                    var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.FreqDur.Create", ex);
                    SoundPower.Web.Utilities.ReportError(bex);
                    throw bex;
                }
                finally
                {
                    if (svcComp != null) svcComp.Dispose();
                }


                if (!rsp.IsFailure)
                {
                    SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.FreqDurPages.CreateSuccess);
                    return RedirectToAction("index");
                }
                else
                {
                    SiteUtilities.ApplyFieldIssues(rsp, ModelState);
                }
            }


            return View(model);
        }

        [RequireAdmin]
        [HttpPost]
        [AntiForgeryHandleError]
        public ActionResult Update(FreqDurInfo model, int[] disciplineId)
        {
            model.Disciplines = SiteUtilities.ArrayToGenericEntityList(disciplineId, "Discipline");

            if (ModelState.IsValid)
            {
                ResponseBase rsp = null;
                Company.FreqDurServiceClient svcComp = null;
                try
                {
                    var token = UserAuthorization.CurrentUser;

                    svcComp = new Company.FreqDurServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                    rsp = svcComp.Update(model);
                    SoundPower.Web.Notifications.AddResponseNotifications(rsp);
                }
                catch (Exception ex)
                {
                    var bex = new SoundPower.ErrorTracking.BaseException("Save Processing Failure", "TherapyCorner.Portal.Controllers.FreqDur.Update", ex);
                    SoundPower.Web.Utilities.ReportError(bex);
                    throw bex;
                }
                finally
                {
                    if (svcComp != null) svcComp.Dispose();
                }


                if (!rsp.IsFailure)
                {
                    SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.FreqDurPages.UpdateSuccess);
                    return RedirectToAction("index");

                }
                else
                {
                    SiteUtilities.ApplyFieldIssues(rsp, ModelState);
                }
            }


            return View(model);
        }

        [RequireAdmin]
        [HttpGet]
        public ActionResult Update(int? id)
        {
            if (!id.HasValue)
            {

                SoundPower.Web.Notifications.AddErrorNotification(ResourceText.SharedPages.CouldNotProcessRelogin);
                return RedirectToAction("index");

            }
            FreqDurInfoResponse rsp = null;
            Company.FreqDurServiceClient svcComp = null;
            try
            {
                var token = UserAuthorization.CurrentUser;

                svcComp = new Company.FreqDurServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                rsp = svcComp.Details(id.Value);
                SoundPower.Web.Notifications.AddResponseNotifications(rsp);
            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.FreqDur.Update", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {
                if (svcComp != null) svcComp.Dispose();
            }


            if (rsp.IsFailure || rsp.Duration == null)
            {
                return RedirectToAction("index");

            }
            return View(rsp.Duration);
        }

        [RequireAdmin]
        [HttpPost]
        [AntiForgeryHandleError]
        public ActionResult Activate(int id)
        {
            if (ModelState.IsValid)
            {
                ResponseBase rsp = null;
                Company.FreqDurServiceClient svcComp = null;
                try
                {
                    var token = UserAuthorization.CurrentUser;

                    svcComp = new Company.FreqDurServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                    var rspOriginal = svcComp.Details(id);
                    if (rspOriginal.IsFailure)
                    {
                        rsp = rspOriginal;
                    }
                    else
                    {
                        rspOriginal.Duration.IsActive = true;
                        rsp = svcComp.Update(rspOriginal.Duration);
                        SoundPower.Web.Notifications.AddResponseNotifications(rsp);
                    }

                }
                catch (Exception ex)
                {
                    var bex = new SoundPower.ErrorTracking.BaseException("Save Processing Failure", "TherapyCorner.Portal.Controllers.FreqDur.Activate", ex);
                    SoundPower.Web.Utilities.ReportError(bex);
                    throw bex;
                }
                finally
                {
                    if (svcComp != null) svcComp.Dispose();
                }


                if (!rsp.IsFailure)
                {
                    SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.FreqDurPages.ActivateSuccess);
                    return RedirectToAction("index");

                }
                else
                {
                    SiteUtilities.ApplyFieldIssues(rsp, ModelState);
                }
            }


            return RedirectToAction("index");
        }

        [HttpGet]
        public ActionResult Activate()
        {
            SoundPower.Web.Notifications.AddErrorNotification(ResourceText.SharedPages.CouldNotProcessRelogin);
            return RedirectToAction("index");
        }

        [HttpGet]
        public ActionResult DeActivate()
        {
            SoundPower.Web.Notifications.AddErrorNotification(ResourceText.SharedPages.CouldNotProcessRelogin);
            return RedirectToAction("index");
        }
        [RequireAdmin]
        [HttpPost]
        [AntiForgeryHandleError]
        public ActionResult DeActivate(int id)
        {
            if (ModelState.IsValid)
            {
                ResponseBase rsp = null;
                Company.FreqDurServiceClient svcComp = null;
                try
                {
                    var token = UserAuthorization.CurrentUser;

                    svcComp = new Company.FreqDurServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                    var rspOriginal = svcComp.Details(id);
                    if (rspOriginal.IsFailure)
                    {
                        rsp = rspOriginal;
                    }
                    else
                    {
                        rspOriginal.Duration.IsActive = false;
                        rsp = svcComp.Update(rspOriginal.Duration);
                        SoundPower.Web.Notifications.AddResponseNotifications(rsp);
                    }

                }
                catch (Exception ex)
                {
                    var bex = new SoundPower.ErrorTracking.BaseException("Save Processing Failure", "TherapyCorner.Portal.Controllers.FreqDur.DeActivate", ex);
                    SoundPower.Web.Utilities.ReportError(bex);
                    throw bex;
                }
                finally
                {
                    if (svcComp != null) svcComp.Dispose();
                }


                if (!rsp.IsFailure)
                {
                    SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.FreqDurPages.DeactivateSuccess);
                    return RedirectToAction("index");

                }
                else
                {
                    SiteUtilities.ApplyFieldIssues(rsp, ModelState);
                }
            }


            return RedirectToAction("index");
        }
    }
}