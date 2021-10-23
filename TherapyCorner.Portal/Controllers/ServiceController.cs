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
    public class ServiceController : Controller
    {
        [RequireAdmin]
        [CompanyFilter]
        public ActionResult Index()
        {
            ServiceInfoListResponse rsp = null;
            Company.ServiceInfoServiceClient svcComp = null;
            try
            {
                var token = UserAuthorization.CurrentUser;

                svcComp = new Company.ServiceInfoServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                rsp = svcComp.List(false);
                SoundPower.Web.Notifications.AddResponseNotifications(rsp);
            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.Service.Index", ex);
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
            rsp.Services.Sort((a, b) => a.Name.CompareTo(b.Name));
            return View(rsp.Services);
        }

        [RequireAdmin]
        public ActionResult Remove(int rateId)
        {
            ResponseBase rsp = null;
            Company.ServiceInfoServiceClient svcComp = null;
            try
            {
                var token = UserAuthorization.CurrentUser;

                svcComp = new Company.ServiceInfoServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                rsp = svcComp.Remove(rateId);
                SoundPower.Web.Notifications.AddResponseNotifications(rsp);
            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.Service.Remove", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {
                if (svcComp != null) svcComp.Dispose();
            }


            if (!rsp.IsFailure)
            {
                SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.ServicePages.RemoveSuccess);

            }
            return RedirectToAction("index");
        }

        [RequireAdmin]
        [HttpGet]
        public ActionResult Create()
        {
            try
            {
                SetViewBagLists();

            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.Service.Create", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            return View(new ServiceInfo() { ServiceId = -1, IsActive = true, Discipline=new GenericEntity("","Discipline",null) , Minutes=60});
        }

        [RequireAdmin]
        [HttpPost]
        [AntiForgeryHandleError]
        public ActionResult Create(ServiceInfo request,string[] cptIds,string[] rateIds, string[] providerIds,string[] durationIds)
        {
            SetRequestLists(request, cptIds, rateIds, providerIds, durationIds);
            request.Validate();
            if (ModelState.IsValid)
            {
                ResponseBase rsp = null;
                Company.ServiceInfoServiceClient svcComp = null;
                try
                {
                    var token = UserAuthorization.CurrentUser;

                    svcComp = new Company.ServiceInfoServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                    rsp = svcComp.Create(request);
                    SoundPower.Web.Notifications.AddResponseNotifications(rsp);
                }
                catch (Exception ex)
                {
                    var bex = new SoundPower.ErrorTracking.BaseException("Save Processing Failure", "TherapyCorner.Portal.Controllers.Service.Create", ex);
                    SoundPower.Web.Utilities.ReportError(bex);
                    throw bex;
                }
                finally
                {
                    if (svcComp != null) svcComp.Dispose();
                }


                if (!rsp.IsFailure)
                {
                    SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.ServicePages.CreateSuccess);
                    return RedirectToAction("index");
                }
                else
                {
                    SiteUtilities.ApplyFieldIssues(rsp, ModelState);
                }
            }
            try
            {
                SetViewBagLists();

            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Reload Processing Failure", "TherapyCorner.Portal.Controllers.Service.Create", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }

            return View(request);
        }

        private static void SetRequestLists(ServiceInfo request, string[] cptIds, string[] rateIds, string[] providerIds, string[] durationIds)
        {
            if (cptIds!=null && cptIds.Length>0)
            {
                request.CPTs = new GenericEntityList();
                request.CPTs.AddRange(from c in cptIds
                                      select new GenericEntity(c.Trim(), "CPT", null));
            }
            if (rateIds != null && rateIds.Length > 0)
            {
                request.Rates = new GenericEntityList();
                request.Rates.AddRange(from c in rateIds
                                       select new GenericEntity(c.Trim(), "PayRate", null));
            }
            if (providerIds != null && providerIds.Length > 0)
            {
                request.Providers = new GenericEntityList();
                request.Providers.AddRange(from c in providerIds
                                           select new GenericEntity(c.Trim(), "Staff", null));
            }
            if (durationIds != null && durationIds.Length > 0)
            {
                request.Frequencies = new GenericEntityList();
                request.Frequencies.AddRange(from c in durationIds
                                             select new GenericEntity(c.Trim(), "Freq/Dur", null));
            }
        }

        [RequireAdmin]
        [HttpPost]
        [AntiForgeryHandleError]
        public ActionResult Update(ServiceInfo request, string[] cptIds, string[] rateIds, string[] providerIds, string[] durationIds)
        {
            SetRequestLists(request, cptIds, rateIds, providerIds, durationIds);

            if (ModelState.IsValid)
            {
                ResponseBase rsp = null;
                Company.ServiceInfoServiceClient svcComp = null;
                try
                {
                    var token = UserAuthorization.CurrentUser;

                    svcComp = new Company.ServiceInfoServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                    rsp = svcComp.Update(request);
                    SoundPower.Web.Notifications.AddResponseNotifications(rsp);
                }
                catch (Exception ex)
                {
                    var bex = new SoundPower.ErrorTracking.BaseException("Save Processing Failure", "TherapyCorner.Portal.Controllers.Service.Update", ex);
                    SoundPower.Web.Utilities.ReportError(bex);
                    throw bex;
                }
                finally
                {
                    if (svcComp != null) svcComp.Dispose();
                }


                if (!rsp.IsFailure)
                {
                    SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.ServicePages.UpdateSuccess);
                    return RedirectToAction("index");

                }
                else
                {
                    SiteUtilities.ApplyFieldIssues(rsp, ModelState);
                }
            }
            try
            {
                SetViewBagLists(int.Parse(request.Discipline.UniqueId));


            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Reload Processing Failure", "TherapyCorner.Portal.Controllers.Service.Update", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }

            return View(request);
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
            ServiceInfoResponse rsp = null;
            Company.ServiceInfoServiceClient svcComp = null;
            try
            {
                var token = UserAuthorization.CurrentUser;

                svcComp = new Company.ServiceInfoServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                rsp = svcComp.Details(id.Value );
                SoundPower.Web.Notifications.AddResponseNotifications(rsp);
                SetViewBagLists(int.Parse(rsp.Service.Discipline.UniqueId));
            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.Service.Update", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {
                if (svcComp != null) svcComp.Dispose();
            }


            if (rsp.IsFailure || rsp.Service == null)
            {
                return RedirectToAction("index");

            }
            return View(rsp.Service);
        }


        [RequireWorkerFilter]
        public JsonResult Details(int id)
        {
            JsonResult result = new JsonResult();
            result.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            ServiceInfoResponse rsp = null;
            Company.ServiceInfoServiceClient svcComp = null;
            try
            {
                var token = UserAuthorization.CurrentUser;

                svcComp = new Company.ServiceInfoServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                rsp = svcComp.Details(id);
                SoundPower.Web.Notifications.AddResponseNotifications(rsp);
                SetViewBagLists();
            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.Service.Details", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                rsp = new ServiceInfoResponse() { IsFailure = true };
                rsp.ErrorMessages.Add(bex.Message);
            }
            finally
            {
                if (svcComp != null) svcComp.Dispose();
            }

            result.Data = rsp;
            return result;
        }

        [RequireAdmin]
        [HttpPost]
        [AntiForgeryHandleError]
        public ActionResult Activate(int id)
        {
            if (ModelState.IsValid)
            {
                ResponseBase rsp = null;
                Company.ServiceInfoServiceClient svcComp = null;
                try
                {
                    var token = UserAuthorization.CurrentUser;

                    svcComp = new Company.ServiceInfoServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                    var rspOriginal = svcComp.Details(id);
                    if (rspOriginal.IsFailure)
                    {
                        rsp = rspOriginal;
                    }
                    else
                    {
                        rspOriginal.Service.IsActive = true;
                        rsp = svcComp.Update(rspOriginal.Service);
                        SoundPower.Web.Notifications.AddResponseNotifications(rsp);
                    }

                }
                catch (Exception ex)
                {
                    var bex = new SoundPower.ErrorTracking.BaseException("Save Processing Failure", "TherapyCorner.Portal.Controllers.Service.Activate", ex);
                    SoundPower.Web.Utilities.ReportError(bex);
                    throw bex;
                }
                finally
                {
                    if (svcComp != null) svcComp.Dispose();
                }


                if (!rsp.IsFailure)
                {
                    SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.ServicePages.ActivateSuccess);
                    return RedirectToAction("index");

                }
                else
                {
                    SiteUtilities.ApplyFieldIssues(rsp, ModelState);
                }
            }


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
                Company.ServiceInfoServiceClient svcComp = null;
                try
                {
                    var token = UserAuthorization.CurrentUser;

                    svcComp = new Company.ServiceInfoServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                    var rspOriginal = svcComp.Details(id);
                    if (rspOriginal.IsFailure)
                    {
                        rsp = rspOriginal;
                    }
                    else
                    {
                        rspOriginal.Service.IsActive = false;
                        rsp = svcComp.Update(rspOriginal.Service);
                        SoundPower.Web.Notifications.AddResponseNotifications(rsp);
                    }

                }
                catch (Exception ex)
                {
                    var bex = new SoundPower.ErrorTracking.BaseException("Save Processing Failure", "TherapyCorner.Portal.Controllers.Service.DeActivate", ex);
                    SoundPower.Web.Utilities.ReportError(bex);
                    throw bex;
                }
                finally
                {
                    if (svcComp != null) svcComp.Dispose();
                }


                if (!rsp.IsFailure)
                {
                    SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.ServicePages.DeactivateSuccess);
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

        private void SetViewBagLists(int? discipline =null)
        {
            Company.PayRateServiceClient svcRate = null;
            Company.FreqDurServiceClient svcFreq = null;
            Company.StaffServiceClient svcProvider = null;
            Company.DisciplineServiceClient svcDisc = null;
            Company.GovernmentProgramServiceClient svcGovt = null;
            
            try
            {
                var token = UserAuthorization.CurrentUser;
                //Get Disciplines
                svcDisc = new Company.DisciplineServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                var rspDisc = svcDisc.List();
                SoundPower.Web.Notifications.AddResponseNotifications(rspDisc);
                if (rspDisc.IsFailure) throw new Exception("Failure received from discipline list call.");
                if (rspDisc.Disciplines != null && rspDisc.Disciplines.Count > 0) rspDisc.Disciplines.Sort((a, b) => a.Name.CompareTo(b.Name));
                ViewBag.Disciplines = rspDisc.Disciplines;

                //Get Rates
                if (discipline.HasValue)
                {
                    svcRate = new Company.PayRateServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                    var rspInfo = svcDisc.Items(discipline.Value);
                    SoundPower.Web.Notifications.AddResponseNotifications(rspInfo);
                    if (rspInfo.IsFailure) throw new Exception("Failure received from discipline item list call.");
                    if (rspInfo.PayRates != null && rspInfo.PayRates.Count > 0) rspInfo.PayRates.Sort((a, b) => a.Name.CompareTo(b.Name));
                    ViewBag.Rates = rspInfo.PayRates;

                    if (rspInfo.FreqDurations != null && rspInfo.FreqDurations.Count > 0) rspInfo.FreqDurations.Sort((a, b) => a.Name.CompareTo(b.Name));
                    ViewBag.Durations = rspInfo.FreqDurations;

                    if (rspInfo.ProvidingStaff != null && rspInfo.ProvidingStaff.Count > 0) rspInfo.ProvidingStaff.Sort((a, b) => a.Name.CompareTo(b.Name));
                    ViewBag.Providers = rspInfo.ProvidingStaff;
                }
                ViewBag.CPTs = StaticData.CPTCodes;

                //Get Gov't Programs
                svcGovt = new Company.GovernmentProgramServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                var rspGovt = svcGovt.List (true);
                SoundPower.Web.Notifications.AddResponseNotifications(rspGovt );
                if (rspGovt.IsFailure) throw new Exception("Failure received from governmetn program list call.");
                if (rspGovt.Programs != null && rspGovt.Programs.Count > 0)
                {
                    rspGovt.Programs.Sort((a, b) => a.Name.CompareTo(b.Name));
                }
                ViewBag.Programs = rspGovt.Programs;

              
            }
            catch (Exception ex)
            {
                throw new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.Service.SetViewBagLists", ex);
              
            }
            finally
            {
                if (svcRate != null) svcRate.Dispose();
                if (svcFreq != null) svcFreq.Dispose();
                if (svcProvider != null) svcProvider.Dispose();
                if (svcGovt != null) svcGovt.Dispose();
            }
        }
    }
}