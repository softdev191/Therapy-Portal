using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using www.therapycorner.com.company.MessageContracts;
using www.therapycorner.com.company;
using www.soundpower.biz.common;

namespace TherapyCorner.Portal.Controllers
{
    [RequireHttps]
    [RequireUser]
    public class PayRateController : Controller
    {
        // GET: PayRate
        [RequireAdmin]
        [CompanyFilter]
        public ActionResult Index()
        {
            PayRateListResponse  rsp = null;
            Company.PayRateServiceClient svcComp = null;
            try
            {
                var token = UserAuthorization.CurrentUser;

                svcComp = new Company.PayRateServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                rsp = svcComp.List(false);
                SoundPower.Web.Notifications.AddResponseNotifications(rsp);
            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.PayRate.Index", ex);
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
            rsp.Rates.Sort((a, b) => a.Name.CompareTo(b.Name));
            return View(rsp.Rates);
        }

        [RequireAdmin]
        public ActionResult Remove(int? rateId)
        {
            if (!rateId.HasValue)
            {

                SoundPower.Web.Notifications.AddErrorNotification(ResourceText.SharedPages.CouldNotProcessRelogin);
                return RedirectToAction("index");

            }
            ResponseBase rsp = null;
            Company.PayRateServiceClient svcComp = null;
            try
            {
                var token = UserAuthorization.CurrentUser;

                svcComp = new Company.PayRateServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                rsp = svcComp.Remove(rateId.Value);
                SoundPower.Web.Notifications.AddResponseNotifications(rsp);
            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.PayRate.Remove", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {
                if (svcComp != null) svcComp.Dispose();
            }


            if (!rsp.IsFailure)
            {
                SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.PayRatePages.RemoveSuccess);

            }
            return RedirectToAction("index");
        }

        [RequireAdmin]
        [HttpGet]
        public ActionResult Create()
        {
            return View(new CreatePayRateRequest() { Disciplines=new GenericEntityList(), StartDate=DateTime.Today, UnitRate=0   });
        }

        [RequireAdmin]
        [HttpPost]
        [AntiForgeryHandleError]
        public ActionResult Create(CreatePayRateRequest  model, int[] disciplineId)
        {
            model.Disciplines = SiteUtilities.ArrayToGenericEntityList(disciplineId, "Discipline");

            if (ModelState.IsValid)
            {
                ResponseBase rsp = null;
                Company.PayRateServiceClient svcComp = null;
                try
                {
                    var token = UserAuthorization.CurrentUser;

                    svcComp = new Company.PayRateServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                    rsp = svcComp.Create(model);
                    SoundPower.Web.Notifications.AddResponseNotifications(rsp);
                }
                catch (Exception ex)
                {
                    var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.PayRate.Create", ex);
                    SoundPower.Web.Utilities.ReportError(bex);
                    throw bex;
                }
                finally
                {
                    if (svcComp != null) svcComp.Dispose();
                }


                if (!rsp.IsFailure)
                {
                    SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.PayRatePages.CreateSuccess);
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
        public ActionResult Update(PayRate model, int[] disciplineId)
        {
            model.Disciplines = SiteUtilities.ArrayToGenericEntityList(disciplineId, "Discipline");

            if (ModelState.IsValid)
            {
                ResponseBase rsp = null;
                Company.PayRateServiceClient svcComp = null;
                try
                {
                    var token = UserAuthorization.CurrentUser;

                    svcComp = new Company.PayRateServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                    rsp = svcComp.Update(model);
                    SoundPower.Web.Notifications.AddResponseNotifications(rsp);
                }
                catch (Exception ex)
                {
                    var bex = new SoundPower.ErrorTracking.BaseException("Save Processing Failure", "TherapyCorner.Portal.Controllers.PayRate.Update", ex);
                    SoundPower.Web.Utilities.ReportError(bex);
                    throw bex;
                }
                finally
                {
                    if (svcComp != null) svcComp.Dispose();
                }


                if (!rsp.IsFailure)
                {
                    SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.PayRatePages.UpdateSuccess);
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
            PayRateResponse rsp = null;
            Company.PayRateServiceClient svcComp = null;
            try
            {
                var token = UserAuthorization.CurrentUser;

                svcComp = new Company.PayRateServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                rsp = svcComp.Details(id.Value );
                SoundPower.Web.Notifications.AddResponseNotifications(rsp);
            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.PayRate.Update", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {
                if (svcComp != null) svcComp.Dispose();
            }


            if (rsp.IsFailure || rsp.Rate == null)
            {
                return RedirectToAction("index");

            }
            return View(rsp.Rate);
        }

        [RequireAdmin]
        [HttpPost]
        [AntiForgeryHandleError]
        public ActionResult Activate(int id)
        {
            if (ModelState.IsValid)
            {
                ResponseBase rsp = null;
                Company.PayRateServiceClient svcComp = null;
                try
                {
                    var token = UserAuthorization.CurrentUser;

                    svcComp = new Company.PayRateServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                    var rspOriginal = svcComp.Details(id);
                    if(rspOriginal.IsFailure)
                    {
                        rsp = rspOriginal;
                    }
                    else
                    {
                        rspOriginal.Rate.IsActive = true;
                        rsp = svcComp.Update(rspOriginal.Rate);
                        SoundPower.Web.Notifications.AddResponseNotifications(rsp);
                    }
 
                }
                catch (Exception ex)
                {
                    var bex = new SoundPower.ErrorTracking.BaseException("Save Processing Failure", "TherapyCorner.Portal.Controllers.PayRate.Activate", ex);
                    SoundPower.Web.Utilities.ReportError(bex);
                    throw bex;
                }
                finally
                {
                    if (svcComp != null) svcComp.Dispose();
                }


                if (!rsp.IsFailure)
                {
                    SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.PayRatePages.ActivateSuccess);
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
                Company.PayRateServiceClient svcComp = null;
                try
                {
                    var token = UserAuthorization.CurrentUser;

                    svcComp = new Company.PayRateServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                    var rspOriginal = svcComp.Details(id);
                    if (rspOriginal.IsFailure)
                    {
                        rsp = rspOriginal;
                    }
                    else
                    {
                        rspOriginal.Rate.IsActive = false;
                        rsp = svcComp.Update(rspOriginal.Rate);
                        SoundPower.Web.Notifications.AddResponseNotifications(rsp);
                    }

                }
                catch (Exception ex)
                {
                    var bex = new SoundPower.ErrorTracking.BaseException("Save Processing Failure", "TherapyCorner.Portal.Controllers.PayRate.DeActivate", ex);
                    SoundPower.Web.Utilities.ReportError(bex);
                    throw bex;
                }
                finally
                {
                    if (svcComp != null) svcComp.Dispose();
                }


                if (!rsp.IsFailure)
                {
                    SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.PayRatePages.DeactivateSuccess);
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