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
    public class DisciplineController : Controller
    {
        // GET: Discipline
        [RequireAdmin]
        [CompanyFilter]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]

        public ActionResult Index()
        {
            DisciplineInfoListResponse rsp = null;
            Company.DisciplineServiceClient svcComp = null;
            try
            {
                var token = UserAuthorization.CurrentUser;

                svcComp = new Company.DisciplineServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                rsp = svcComp.List();
                SoundPower.Web.Notifications.AddResponseNotifications(rsp);
            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.Discipline.Index", ex);
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
            rsp.Disciplines.Sort((a, b) => a.Name.CompareTo(b.Name));
            return View(rsp.Disciplines);
        }

        [ChildActionOnly]
        public PartialViewResult CheckList(GenericEntityList selected)
        {
            DisciplineInfoListResponse rsp = null;
            Company.DisciplineServiceClient svcComp = null;
            try
            {
                var token = UserAuthorization.CurrentUser;

                svcComp = new Company.DisciplineServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                rsp = svcComp.List();
                SoundPower.Web.Notifications.AddResponseNotifications(rsp);
            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.Discipline.CheckList", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {
                if (svcComp != null) svcComp.Dispose();
            }


            if (rsp.IsFailure)
            {
                rsp.Disciplines = new DisciplineInfoList();

            }
            ViewBag.SelectedDisciplines = selected;
            rsp.Disciplines.Sort((a, b) => a.Name.CompareTo(b.Name));
            return PartialView(rsp.Disciplines);
        }
        [RequireAdmin]
        public ActionResult Remove(int id)
        {
            ResponseBase rsp = null;
            Company.DisciplineServiceClient svcComp = null;
            try
            {
                var token = UserAuthorization.CurrentUser;

                svcComp = new Company.DisciplineServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                rsp = svcComp.Remove(id);
                SoundPower.Web.Notifications.AddResponseNotifications(rsp);
            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.Discipline.Remove", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {
                if (svcComp != null) svcComp.Dispose();
            }


            if (!rsp.IsFailure)
            {
                SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.DisciplinePages.RemoveSuccess);

            }
            return RedirectToAction("index");
        }

        [RequireAdmin]
        [HttpPost]
        //[AntiForgeryHandleError]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]

        public ActionResult Create(string name)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                ResponseBase rsp = null;
                Company.DisciplineServiceClient svcComp = null;
                try
                {
                    var token = UserAuthorization.CurrentUser;

                    svcComp = new Company.DisciplineServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                    rsp = svcComp.Create(new www.therapycorner.com.company.DisciplineInfo() { DisciplineId = -1, Name = name, Version = "NEW" });
                    SoundPower.Web.Notifications.AddResponseNotifications(rsp);
                }
                catch (Exception ex)
                {
                    var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.Discipline.Create", ex);
                    SoundPower.Web.Utilities.ReportError(bex);
                    throw bex;
                }
                finally
                {
                    if (svcComp != null) svcComp.Dispose();
                }


                if (!rsp.IsFailure)
                {
                    SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.DisciplinePages.CreateSuccess);

                }
                else if (rsp.FieldIssues!=null && rsp.FieldIssues.Count>0)
                {
                    rsp.FieldIssues.ForEach(i => SoundPower.Web.Notifications.AddErrorNotification(i.IssueMessage));
                }
            }
            else
            {
                SoundPower.Web.Notifications.AddErrorNotification(ResourceText.DisciplinePages.NameRequired);
            }
         
            return RedirectToAction("index");
        }


        [CompanyFilter]
        [RequireAdmin]
        public JsonResult Items(int id)
        {
            JsonResult result = new JsonResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet };

            DisciplineItemsResponse  rsp = null;
            Company.DisciplineServiceClient svcComp = null;
            try
            {
                var token = UserAuthorization.CurrentUser;

                svcComp = new Company.DisciplineServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                rsp = svcComp.Items (id);
            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.Discipline.Items", ex);
                rsp = new DisciplineItemsResponse() { IsFailure = true };
                rsp.ErrorMessages.Add(bex.Message);
            }
            finally
            {
                if (svcComp != null) svcComp.Dispose();
            }

            if (rsp.WarningMessages == null) rsp.WarningMessages = new List<string>();
            if (rsp.PayRates == null || rsp.PayRates.Count == 0) rsp.WarningMessages.Add(ResourceText.DisciplinePages.NoPayRates );
            if (rsp.FreqDurations == null || rsp.FreqDurations.Count == 0) rsp.WarningMessages.Add(ResourceText.DisciplinePages.NoDurations );
            if (rsp.ProvidingStaff == null || rsp.ProvidingStaff.Count == 0) rsp.WarningMessages.Add(ResourceText.DisciplinePages.NoProviders);

            result.Data = rsp;
            return result;
        }

        [RequireAdmin]
        [HttpPost]
        [AntiForgeryHandleError]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public ActionResult Edit(DisciplineInfo model)
        {

            if (ModelState.IsValid)
            {
                ResponseBase rsp = null;
                Company.DisciplineServiceClient svcComp = null;
                try
                {
                    var token = UserAuthorization.CurrentUser;

                    svcComp = new Company.DisciplineServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                    rsp = svcComp.Update(model);
                    SoundPower.Web.Notifications.AddResponseNotifications(rsp);
                }
                catch (Exception ex)
                {
                    var bex = new SoundPower.ErrorTracking.BaseException("Save Processing Failure", "TherapyCorner.Portal.Controllers.Discipline.Edit", ex);
                    SoundPower.Web.Utilities.ReportError(bex);
                    throw bex;
                }
                finally
                {
                    if (svcComp != null) svcComp.Dispose();
                }


                if (!rsp.IsFailure)
                {
                    SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.DisciplinePages.UpdateSuccess);
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
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]

        public ActionResult Edit(int? id)
        {
            if (!id.HasValue)
            {

                SoundPower.Web.Notifications.AddErrorNotification(ResourceText.SharedPages.CouldNotProcessRelogin);
                return RedirectToAction("index");

            }

            DisciplineInfoResponse rsp = null;
            Company.DisciplineServiceClient svcComp = null;
            try
            {
                var token = UserAuthorization.CurrentUser;

                svcComp = new Company.DisciplineServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                rsp = svcComp.Details(id.Value);
                SoundPower.Web.Notifications.AddResponseNotifications(rsp);
            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.Discipline.Edit", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {
                if (svcComp != null) svcComp.Dispose();
            }


            if (rsp.IsFailure || rsp.Discipline == null)
            {
                return RedirectToAction("index");

            }
            return View(rsp.Discipline);
        }

        [HttpGet]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]

        public ActionResult Activate()
        {
            SoundPower.Web.Notifications.AddErrorNotification(ResourceText.SharedPages.CouldNotProcessRelogin);
            return RedirectToAction("index");
        }

        [RequireAdmin]
        [HttpPost]
        [AntiForgeryHandleError]
        public ActionResult Activate(int id)
        {
            if (ModelState.IsValid)
            {
                ResponseBase rsp = null;
                Company.DisciplineServiceClient svcComp = null;
                try
                {
                    var token = UserAuthorization.CurrentUser;

                    svcComp = new Company.DisciplineServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                    rsp = svcComp.SetStatus(id, true);
                    

                }
                catch (Exception ex)
                {
                    var bex = new SoundPower.ErrorTracking.BaseException("Save Processing Failure", "TherapyCorner.Portal.Controllers.Discipline.Activate", ex);
                    SoundPower.Web.Utilities.ReportError(bex);
                    throw bex;
                }
                finally
                {
                    if (svcComp != null) svcComp.Dispose();
                }


                if (!rsp.IsFailure)
                {
                    SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.DisciplinePages.ActivateSuccess);
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
                Company.DisciplineServiceClient svcComp = null;
                try
                {
                    var token = UserAuthorization.CurrentUser;

                    svcComp = new Company.DisciplineServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                    rsp = svcComp.SetStatus(id, false);


                }
                catch (Exception ex)
                {
                    var bex = new SoundPower.ErrorTracking.BaseException("Save Processing Failure", "TherapyCorner.Portal.Controllers.Discipline.DeActivate", ex);
                    SoundPower.Web.Utilities.ReportError(bex);
                    throw bex;
                }
                finally
                {
                    if (svcComp != null) svcComp.Dispose();
                }


                if (!rsp.IsFailure)
                {
                    SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.DisciplinePages.DeactivateSuccess);
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