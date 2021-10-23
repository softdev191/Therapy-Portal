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
    public class GoalAreaController : Controller
    {
        [RequireAdmin]
        [HttpPost]
        [AntiForgeryHandleError]
        public ActionResult Create(string goalName, string disciplineId)
        {

            if (!string.IsNullOrWhiteSpace(goalName))
            {
                ResponseBase rsp = null;
                Company.GoalServiceClient svcComp = null;
                Company.DisciplineServiceClient svcDisc = null;
                GenericEntity  areaInfo = null;
                try
                {
                    var token = UserAuthorization.CurrentUser;
                    var area = new www.therapycorner.com.company.GoalArea() { AreaId = -1, Name = goalName.Trim(), Discipline = new GenericEntity(disciplineId, "Discipline", null), Version = "NEW" };
                    svcComp = new Company.GoalServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                    rsp = svcComp.CreateArea(area);
                    SoundPower.Web.Notifications.AddResponseNotifications(rsp);
                    if (!rsp.IsFailure )
                    {
                        svcDisc = new Company.DisciplineServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                        var rspDisc = svcDisc.Details(int.Parse(disciplineId));
                        areaInfo = rspDisc.Discipline.GoalAreas.Find(a => a.Name.ToUpper() == area.Name.ToUpper());
                    }
                }
                catch (Exception ex)
                {
                    var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.GoalArea.Create", ex);
                    SoundPower.Web.Utilities.ReportError(bex);
                    throw bex;
                }
                finally
                {
                    if (svcComp != null) svcComp.Dispose();
                    if (svcDisc != null) svcDisc.Dispose();
                }


                if (!rsp.IsFailure)
                {
                    SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.GoalAreaPages.CreateSuccess);
                    return RedirectToAction("edit", new { id = areaInfo.UniqueId });

                }
                else if (rsp.FieldIssues != null && rsp.FieldIssues.Count > 0)
                {
                    rsp.FieldIssues.ForEach(i => SoundPower.Web.Notifications.AddErrorNotification(i.IssueMessage));
                }
            }
            else
            {
                SoundPower.Web.Notifications.AddErrorNotification(ResourceText.GoalAreaPages.NameRequired);
            }

            return RedirectToAction("edit","discipline",new { id = disciplineId });
        }

        [HttpGet]
        public ActionResult Create()
        {
            SoundPower.Web.Notifications.AddErrorNotification(ResourceText.SharedPages.CouldNotProcessRelogin);
            return RedirectToAction("index", "discipline");
        }



        [RequireAdmin]
        [HttpGet]
        // GET: GoalArea
        public ActionResult Edit(int? id)
        {
            if (!id.HasValue)
            {

                SoundPower.Web.Notifications.AddErrorNotification(ResourceText.SharedPages.CouldNotProcessRelogin);
                return RedirectToAction("index","discipline");

            }

            GoalAreaResponse rsp = null;
            Company.GoalServiceClient svcComp = null;
            try
            {
                var token = UserAuthorization.CurrentUser;

                svcComp = new Company.GoalServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                rsp = svcComp.GoalArea(id.Value);
                SoundPower.Web.Notifications.AddResponseNotifications(rsp);
            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.GoalArea.Edit", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {
                if (svcComp != null) svcComp.Dispose();
            }


            if (rsp.IsFailure || rsp.Area == null)
            {
                return RedirectToAction("index","discipline");

            }
            return View(rsp.Area);
        }

        [RequireAdmin]
        [AntiForgeryHandleError]
        [HttpPost]
        // GET: GoalArea
        public ActionResult Edit(GoalArea model)
        {
            if (ModelState.IsValid)
            {
                ResponseBase rsp = null;
                Company.GoalServiceClient svcComp = null;
                try
                {
                    var token = UserAuthorization.CurrentUser;

                    svcComp = new Company.GoalServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                    rsp = svcComp.UpdateArea(model);
                    SoundPower.Web.Notifications.AddResponseNotifications(rsp);
                }
                catch (Exception ex)
                {
                    var bex = new SoundPower.ErrorTracking.BaseException("Save Processing Failure", "TherapyCorner.Portal.Controllers.GoalArea.Edit", ex);
                    SoundPower.Web.Utilities.ReportError(bex);
                    throw bex;
                }
                finally
                {
                    if (svcComp != null) svcComp.Dispose();
                }


                if (!rsp.IsFailure)
                {
                    SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.GoalAreaPages.UpdateSuccess);
                    return RedirectToAction("edit",new { id = model.AreaId });

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
        // GET: GoalArea
        public ActionResult CreateTemplate(int? id)
        {
            if (!id.HasValue)
            {

                SoundPower.Web.Notifications.AddErrorNotification(ResourceText.SharedPages.CouldNotProcessRelogin);
                return RedirectToAction("index", "discipline");

            }
            GoalAreaResponse rsp = null;
            Company.GoalServiceClient svcComp = null;
            try
            {
                var token = UserAuthorization.CurrentUser;

                svcComp = new Company.GoalServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                rsp = svcComp.GoalArea(id.Value);
                SoundPower.Web.Notifications.AddResponseNotifications(rsp);
            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.GoalArea.CreateTemplate", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {
                if (svcComp != null) svcComp.Dispose();
            }


            if (rsp.IsFailure || rsp.Area == null)
            {
                return RedirectToAction("index", "discipline");

            }
            ViewBag.DisciplineId = rsp.Area.Discipline.UniqueId;
            ViewBag.DisciplineName = rsp.Area.Discipline.Name;
            var model = new GoalInfo() { Area = new GenericEntity(rsp.Area.AreaId.ToString(), "GoalArea", rsp.Area.Name), GoalId = 0, Success = 1, Attempts = 2 };
            return View(model);
        }

        [RequireAdmin]
        [AntiForgeryHandleError]
        [HttpPost]
        // GET: GoalArea
        public ActionResult CreateTemplate(GoalInfo model, int disciplineId, string disciplineName)
        {
            ViewBag.DisciplineId = disciplineId;
            ViewBag.DisciplineName = disciplineName;
            if (ModelState.IsValid)
            {
             
                ResponseBase rsp = null;
                Company.GoalServiceClient svcComp = null;
                try
                {
                    var token = UserAuthorization.CurrentUser;

                    svcComp = new Company.GoalServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                    rsp = svcComp.CreateTemplate(model);
                    SoundPower.Web.Notifications.AddResponseNotifications(rsp);
                }
                catch (Exception ex)
                {
                    var bex = new SoundPower.ErrorTracking.BaseException("Save Processing Failure", "TherapyCorner.Portal.Controllers.GoalArea.CreateTemplate", ex);
                    SoundPower.Web.Utilities.ReportError(bex);
                    throw bex;
                }
                finally
                {
                    if (svcComp != null) svcComp.Dispose();
                }


                if (!rsp.IsFailure)
                {
                    SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.GoalAreaPages.CreateTemplateSuccess);
                    return RedirectToAction("edit",new { id = model.Area.UniqueId });

                }
                else
                {
                    SiteUtilities.ApplyFieldIssues(rsp, ModelState);
                }
            }


            return View(model);
        }
        [RequireAdmin]
        [AntiForgeryHandleError]
        [HttpPost]
        // GET: GoalArea
        public ActionResult RemoveTemplate(int templateId, int areaId)
        {
            ResponseBase rsp = null;
            Company.GoalServiceClient svcComp = null;
            try
            {
                var token = UserAuthorization.CurrentUser;

                svcComp = new Company.GoalServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                rsp = svcComp.RemoveTemplate(templateId);

                SoundPower.Web.Notifications.AddResponseNotifications(rsp);
            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.GoalArea.RemoveTemplate", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {
                if (svcComp != null) svcComp.Dispose();
            }


            if (!rsp.IsFailure )
            {
                SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.GoalAreaPages.RemoveTemplateSuccess);

            }
           
                return RedirectToAction("edit", new { id = areaId });

          
        }

        [RequireAdmin]
        [AntiForgeryHandleError]
        [HttpPost]
        // GET: GoalArea
        public ActionResult Remove(int id, int disciplineId)
        {
            ResponseBase rsp = null;
            Company.GoalServiceClient svcComp = null;
            try
            {
                var token = UserAuthorization.CurrentUser;

                svcComp = new Company.GoalServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                rsp = svcComp.RemoveArea(id);
               

                SoundPower.Web.Notifications.AddResponseNotifications(rsp);
            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.GoalArea.Remove", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {
                if (svcComp != null) svcComp.Dispose();
            }


            if (!rsp.IsFailure)
            {
                SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.GoalAreaPages.RemoveSuccess);

            }

            return RedirectToAction("edit","discipline", new { id = disciplineId  });


        }

        [RequireAdmin]
        [HttpGet]
        // GET: GoalArea
        public ActionResult UpdateTemplate(int? id, int areaId)
        {
            if (!id.HasValue)
            {

                SoundPower.Web.Notifications.AddErrorNotification(ResourceText.SharedPages.CouldNotProcessRelogin);
                return RedirectToAction("index", "discipline");

            }

            GoalAreaResponse rsp = null;
            Company.GoalServiceClient svcComp = null;
            try
            {
                var token = UserAuthorization.CurrentUser;

                svcComp = new Company.GoalServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                rsp = svcComp.GoalArea(areaId);
                
                SoundPower.Web.Notifications.AddResponseNotifications(rsp);
            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.GoalArea.UpdateTemplate", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {
                if (svcComp != null) svcComp.Dispose();
            }


            if (rsp.IsFailure || rsp.Area == null)
            {
                return RedirectToAction("index", "discipline");

            }
            var template = rsp.Area.Templates.Find(t => t.GoalId == id);
            ViewBag.DisciplineId = rsp.Area.Discipline.UniqueId;
            ViewBag.DisciplineName = rsp.Area.Discipline.Name;
            if (template ==null )
            {
                return RedirectToAction("edit", new { id = areaId });

            }
            template.Area = new GenericEntity(rsp.Area.AreaId.ToString(), "GoalArea", rsp.Area.Name);
            return View(template);
        }

        [HttpGet]
        public ActionResult Remove()
        {
            SoundPower.Web.Notifications.AddErrorNotification(ResourceText.SharedPages.CouldNotProcessRelogin);
            return RedirectToAction("index", "discipline");
        }

        [HttpGet]
        public ActionResult UpdateTemplate()
        {
            SoundPower.Web.Notifications.AddErrorNotification(ResourceText.SharedPages.CouldNotProcessRelogin);
            return RedirectToAction("index", "discipline");
        }

        [RequireAdmin]
        [HttpPost]
        // GET: GoalArea
        public ActionResult UpdateTemplate(GoalInfo model, int disciplineId, string disciplineName)
        {
            ViewBag.DisciplineId = disciplineId;
            ViewBag.DisciplineName = disciplineName;
            
            if (ModelState.IsValid)
            {

                ResponseBase rsp = null;
                Company.GoalServiceClient svcComp = null;
                try
                {
                    var token = UserAuthorization.CurrentUser;

                    svcComp = new Company.GoalServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                    rsp = svcComp.UpdateTemplate(model);
                    SoundPower.Web.Notifications.AddResponseNotifications(rsp);
                }
                catch (Exception ex)
                {
                    var bex = new SoundPower.ErrorTracking.BaseException("Save Processing Failure", "TherapyCorner.Portal.Controllers.GoalArea.UpdateTemplate", ex);
                    SoundPower.Web.Utilities.ReportError(bex);
                    throw bex;
                }
                finally
                {
                    if (svcComp != null) svcComp.Dispose();
                }


                if (!rsp.IsFailure)
                {
                    SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.GoalAreaPages.UpdateTemplateSuccess);
                    return RedirectToAction("edit", new { id = model.Area.UniqueId });

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
        public ActionResult Activate(int id)
        {
            if (ModelState.IsValid)
            {
                ResponseBase rsp = null;
                Company.GoalServiceClient svcComp = null;
                try
                {
                    var token = UserAuthorization.CurrentUser;

                    svcComp = new Company.GoalServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                    var rspOriginal = svcComp.GoalArea(id);
                    if (rspOriginal.IsFailure)
                    {
                        rsp = rspOriginal;
                    }
                    else
                    {
                        rspOriginal.Area.IsActive = true;
                        rsp = svcComp.UpdateArea(rspOriginal.Area);
                        SoundPower.Web.Notifications.AddResponseNotifications(rsp);
                    }

                }
                catch (Exception ex)
                {
                    var bex = new SoundPower.ErrorTracking.BaseException("Save Processing Failure", "TherapyCorner.Portal.Controllers.GoalArea.Activate", ex);
                    SoundPower.Web.Utilities.ReportError(bex);
                    throw bex;
                }
                finally
                {
                    if (svcComp != null) svcComp.Dispose();
                }


                if (!rsp.IsFailure)
                {
                    SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.GoalAreaPages.ActivateSuccess);

                }
          
            }


            return RedirectToAction("edit",new { id = id });
        }

        [RequireAdmin]
        [HttpPost]
        [AntiForgeryHandleError]
        public ActionResult DeActivate(int id)
        {
            if (ModelState.IsValid)
            {
                ResponseBase rsp = null;
                Company.GoalServiceClient svcComp = null;
                try
                {
                    var token = UserAuthorization.CurrentUser;

                    svcComp = new Company.GoalServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                    var rspOriginal = svcComp.GoalArea(id);
                    if (rspOriginal.IsFailure)
                    {
                        rsp = rspOriginal;
                    }
                    else
                    {
                        rspOriginal.Area.IsActive = false;
                        rsp = svcComp.UpdateArea(rspOriginal.Area);
                        SoundPower.Web.Notifications.AddResponseNotifications(rsp);
                    }

                }
                catch (Exception ex)
                {
                    var bex = new SoundPower.ErrorTracking.BaseException("Save Processing Failure", "TherapyCorner.Portal.Controllers.GoalArea.DeActivate", ex);
                    SoundPower.Web.Utilities.ReportError(bex);
                    throw bex;
                }
                finally
                {
                    if (svcComp != null) svcComp.Dispose();
                }


                if (!rsp.IsFailure)
                {
                    SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.GoalAreaPages.DeactivateSuccess);

                }
              
            }


            return RedirectToAction("edit", new { id = id });

        }

        [HttpGet]
        public ActionResult Activate()
        {
            SoundPower.Web.Notifications.AddErrorNotification(ResourceText.SharedPages.CouldNotProcessRelogin);
            return RedirectToAction("index","discipline");
        }

        [HttpGet]
        public ActionResult DeActivate()
        {
            SoundPower.Web.Notifications.AddErrorNotification(ResourceText.SharedPages.CouldNotProcessRelogin);
            return RedirectToAction("index", "discipline");
        }

        [CompanyFilter]
        public JsonResult Templates(int id)
        {
            JsonResult result = new JsonResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet };

            GoalAreaResponse rsp = null;
            Company.GoalServiceClient svcComp = null;
            GoalInfoListResponse data = new GoalInfoListResponse();
            try
            {
                var token = UserAuthorization.CurrentUser;

                svcComp = new Company.GoalServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                rsp = svcComp.GoalArea(id);
            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.GoalArea.Templates", ex);
                data.IsFailure = true ;
                data.ErrorMessages.Add(bex.Message);
            }
            finally
            {
                if (svcComp != null) svcComp.Dispose();
            }

if (rsp.IsFailure)
            {
                data.IsFailure = true;
                data.ErrorMessages = rsp.ErrorMessages;
            }
else
            {
                data.Goals = rsp.Area.Templates;
            }
            result.Data = data;
            return result;
        }
    }
}