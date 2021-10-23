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
    [CompanyFilter]
    [OutputCache(NoStore = true, Duration = 0)]
    public class ReportController : Controller
    {

        [HttpPost]
        public JsonResult PendingCount()
        {
            Company.PeriodicReportServiceClient svcClient = null;
            var rsp = new ObjectIdResponseBase(); ;
            JsonResult result = new JsonResult();

            try
            {
                var token = UserAuthorization.CurrentUser;


                svcClient = new Company.PeriodicReportServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                var rspSrch = svcClient.Pending();
                rsp.ErrorMessages = rspSrch.ErrorMessages;
                rsp.WarningMessages = rspSrch.WarningMessages;
                rsp.IsFailure = rspSrch.IsFailure;
                rsp.ObjectId = rsp.IsFailure ? "-" : rspSrch.Reports.Count.ToString();
            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Pending Report Counter Processing Failure", "TherapyCorner.Portal.Controllers.ReportController.PendingCount", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                rsp.IsFailure = true;
                rsp.ErrorMessages.Add(bex.Message);
            }
            finally
            {
                if (svcClient != null) svcClient.Dispose();

            }

            SoundPower.Web.Notifications.AddResponseNotifications(rsp);


            result.Data = rsp;
            return result;
        }

        [HttpPost]
        public JsonResult ReviewCount()
        {
            Company.PeriodicReportServiceClient svcClient = null;
            var rsp = new ObjectIdResponseBase(); ;
            JsonResult result = new JsonResult();

            try
            {
                var token = UserAuthorization.CurrentUser;


                svcClient = new Company.PeriodicReportServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                var rspSrch = svcClient.PendingReview();
                rsp.ErrorMessages = rspSrch.ErrorMessages;
                rsp.WarningMessages = rspSrch.WarningMessages;
                rsp.IsFailure = rspSrch.IsFailure;
                rsp.ObjectId = rsp.IsFailure ? "-" : rspSrch.Reports.Count.ToString();
            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Pending Report Reviews Counter Processing Failure", "TherapyCorner.Portal.Controllers.ReportController.ReviewCount", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                rsp.IsFailure = true;
                rsp.ErrorMessages.Add(bex.Message);
            }
            finally
            {
                if (svcClient != null) svcClient.Dispose();

            }

            SoundPower.Web.Notifications.AddResponseNotifications(rsp);


            result.Data = rsp;
            return result;
        }
        [OutputCache(NoStore = true, Duration = 0)]
        public ActionResult Pending()
        {

            Company.PeriodicReportServiceClient svcReport = null;
            PeriodicReportListResponse rsp = null;
            try
            {
                var token = UserAuthorization.CurrentUser;

                svcReport = new Company.PeriodicReportServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                rsp = svcReport.Pending();

            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Controllers.Report.Pending", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {

                if (svcReport != null) svcReport.Dispose();

            }
            SoundPower.Web.Notifications.AddResponseNotifications(rsp);
            if (rsp.IsFailure || rsp.Reports == null) return RedirectToAction("index", "home");
            return View(rsp.Reports);
        }

        [RequireSupervisorFilter]
        public ActionResult PendingReview()
        {

            Company.PeriodicReportServiceClient svcReport = null;
            PeriodicReportListResponse rsp = null;
            try
            {
                var token = UserAuthorization.CurrentUser;

                svcReport = new Company.PeriodicReportServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                rsp = svcReport.PendingReview();

            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Controllers.Report.PendingReview", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {

                if (svcReport != null) svcReport.Dispose();

            }
            SoundPower.Web.Notifications.AddResponseNotifications(rsp);
            if (rsp.IsFailure || rsp.Reports == null) return RedirectToAction("index", "home");
            return View(rsp.Reports);
        }

        [OutputCache(NoStore = true, Duration = 0)]
        public ActionResult Details(int id, string returnField)
        {
            ViewBag.ReturnField = returnField;
            Company.PeriodicReportServiceClient svcReport = null;
            PeriodicReportResponse rsp = null;
            var context = new Dictionary<string, string>();
            context.Add("id", id.ToString());
            try
            {
                var token = UserAuthorization.CurrentUser;
                
                svcReport = new Company.PeriodicReportServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                
                    rsp = svcReport.Details(id);
                
                if (!rsp.IsFailure && rsp.Report != null)
                {
                    SetReportViewBag(rsp.Report, token);
                }

            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Controllers.Report.Details", ex, context);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {

                if (svcReport != null) svcReport.Dispose();

            }
            SoundPower.Web.Notifications.AddResponseNotifications(rsp);
            if (rsp.IsFailure || rsp.Report == null) return RedirectToAction("index", "home");
            return View(rsp.Report);
        }

        [HttpGet]
        [RequireSupervisorFilter]
        public ActionResult CreateGoal(int? id)
        {
            if (!id.HasValue)
            {

                SoundPower.Web.Notifications.AddErrorNotification(ResourceText.SharedPages.CouldNotProcessRelogin);
                return RedirectToAction("pending");

            }

            Company.PeriodicReportServiceClient svcReport = null;
            PeriodicReportResponse rsp = null;
            var context = new Dictionary<string, string>();
            context.Add("id", id.ToString());
            try
            {
                var token = UserAuthorization.CurrentUser;

                svcReport = new Company.PeriodicReportServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);

                rsp = svcReport.Details(id.Value);

                if (!rsp.IsFailure && rsp.Report != null)
                {
                    if (!SetClientGoalAreas(int.Parse(rsp.Report.Client.UniqueId),int.Parse(rsp.Report.Discipline.UniqueId), "")) return RedirectToAction("details");
                  
                }

            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Load Processing Failure", "TherapyCorner.Controllers.Report.CreateGoal", ex, context);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {

                if (svcReport != null) svcReport.Dispose();

            }
            SoundPower.Web.Notifications.AddResponseNotifications(rsp);
            if (rsp.IsFailure || rsp.Report == null) return RedirectToAction("details", new { id = id });
            ViewBag.DisciplineId = int.Parse( rsp.Report.Discipline.UniqueId);
            ViewBag.ReportId = rsp.Report.ReportId;
            return View(
                new GoalInfo() { Units = "attempts", Success = 3, Attempts = 5, Area = new GenericEntity("?", "GoalArea", null) }
                );
        }

        [HttpPost]
        [AntiForgeryHandleError]
        [RequireAdminOrSupervisorFilter]
        public ActionResult Cancel(int id)
        {
            Company.PeriodicReportServiceClient svcReport = null;
            ResponseBase rsp = null;
            var context = new Dictionary<string, string>();
            context.Add("id", id.ToString());
            try
            {
                var token = UserAuthorization.CurrentUser;

                svcReport = new Company.PeriodicReportServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);

                rsp = svcReport.Cancel(id);

              

            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Controllers.Report.Cancel", ex, context);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {

                if (svcReport != null) svcReport.Dispose();

            }
            SoundPower.Web.Notifications.AddResponseNotifications(rsp);
            if (!rsp.IsFailure)
            {
                SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.ReportPages.ReportCancelled);
            }
            return RedirectToAction("details",new { id = id });
        }

        private bool SetClientGoalAreas(int id, int disciplineId, string area = "")
        {
            Company.ClientServiceClient svcClient = null;
            Company.DisciplineServiceClient svcDisc = null;
            ClientInfoResponse rspClient = null;
            GenericEntityList areas = new GenericEntityList();
            try
            {
                var token = UserAuthorization.CurrentUser;
                svcClient = new Company.ClientServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                rspClient = svcClient.Details(id);


                svcDisc = new Company.DisciplineServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                        var discipline = svcDisc.Details(disciplineId);
                        areas.AddRange(discipline.Discipline.GoalAreas);

               
            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.Report.SetClientGoalAreas", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {
                if (svcClient != null) svcClient.Dispose();

            }



            ViewBag.ClientId = rspClient.Client.ClientId;
            ViewBag.ClientName = rspClient.Client.ToPerson().LastFirstMI;
            areas.Sort((a, b) => a.Name.CompareTo(b.Name));
            ViewBag.Areas = (from a in areas
                             orderby a.Name
                             select new SelectListItem() { Text = a.Name, Value = a.UniqueId, Selected = a.UniqueId == area }).ToList();
            return true;
        }

        [HttpPost]
        [AntiForgeryHandleError]
        [RequireSupervisorFilter]

        public ActionResult CreateGoal(GoalInfo request, int clientId, string ReportId, int disciplineId)
        {
            ViewBag.ReportId = ReportId;
            ViewBag.DisciplineId = disciplineId;
            
            if (ModelState.IsValid)
            {
                Company.PeriodicReportServiceClient svcStaff = null;
                ResponseBase rspStaff = null;
                try
                {
                    var token = UserAuthorization.CurrentUser;


                    svcStaff = new Company.PeriodicReportServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                    rspStaff = svcStaff.CreateGoal(request, int.Parse(ReportId));

                }
                catch (Exception ex)
                {
                    var bex = new SoundPower.ErrorTracking.BaseException("Save Processing Failure", "TherapyCorner.Portal.Controllers.Report.CreateGoal", ex);
                    SoundPower.Web.Utilities.ReportError(bex);
                    throw bex;
                }
                finally
                {
                    if (svcStaff != null) svcStaff.Dispose();

                }
                SoundPower.Web.Notifications.AddResponseNotifications(rspStaff);
                if (!rspStaff.IsFailure)
                {

                    SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.ReportPages.GoalCreated);
                    return RedirectToAction("details", new { id = ReportId });
                }
                else
                {
                    SiteUtilities.ApplyFieldIssues(rspStaff, ModelState);
                }
            }
            try
            {
                if (!SetClientGoalAreas(clientId, disciplineId, request.Area.UniqueId)) return RedirectToAction("details", new { id = ReportId });


            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Validation Return Processing Failure", "TherapyCorner.Portal.Controllers.Report.CreateGoal", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }

            return View(request);
        }


        [OutputCache(NoStore = true, Duration = 0)]
        public ActionResult PDF(int id)
        {
            Company.PeriodicReportServiceClient svcReport = null;
            PeriodicReportResponse rsp = null;
            var context = new Dictionary<string, string>();
            context.Add("id", id.ToString());
            Company.Exports.PeriodicReportExport exporter = null;
            Company.ServiceInfoServiceClient svcService = null;
            Company.ClientServiceClient svcClient = null;
            Company.GovernmentProgramServiceClient svcGovt = null;
            www.therapycorner.com.account.FieldTypeList govtFields = null;
            byte[] renderedBytes = null;
            string mimeType = null;
            string extension = null;
            Account.CompanyServiceClient svcCompany = null;

            try
            {
                var token = UserAuthorization.CurrentUser;

                svcReport = new Company.PeriodicReportServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);

                rsp = svcReport.Details(id);

                if (!rsp.IsFailure && rsp.Report != null)
                {
                    svcCompany = new Account.CompanyServiceClient(SiteUtilities.AccountService, token, SiteUtilities.CurrentCulture);
                    var rspCompany = svcCompany.Details(token.CurrentCompany);
                    svcClient = new Company.ClientServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                    var rspClient = svcClient.Details(int.Parse(rsp.Report.Client.UniqueId));
                    if (rspClient.Client.Diagnosis != null && rspClient.Client.Diagnosis.Count > 0)
                    {
                        var diags = StaticData.Diagnosis;
                        foreach (var d in rspClient.Client.Diagnosis)
                        {
                            d.Name = diags.Find(def => def.UniqueId == d.UniqueId).Name;
                        }
                    }
                    if (rspClient.Client.GovtProgram != null)
                    {
                        govtFields = StaticData.GovernmentFields(rspClient.Client.GovtProgram.UniqueId);
                        if (rspClient.Client.GovtValues != null)
                        {
                            var ft = govtFields.Find(f => f.Type == ValueTypeEnum.ReferenceList && f.ReferenceList == "CaseWorker");
                            if (ft != null && rspClient.Client.GovtValues.Exists(i => i.FieldId == ft.FieldId))
                            {
                                var cw = rspClient.Client.GovtValues.Find(v => v.FieldId == ft.FieldId);
                                if (!string.IsNullOrWhiteSpace(cw.Value))
                                {
                                    svcGovt = new Company.GovernmentProgramServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                                    var rspGovt = svcGovt.CaseWorkers();
                                    var worker = rspGovt.EntityList.Find(w => w.UniqueId == cw.Value);
                                    cw.Value = worker.Name;
                                }
                            }

                        }
                    }

                    exporter = new Company.Exports.PeriodicReportExport(rsp.Report,rspClient.Client, govtFields,rspCompany.Company);
                    renderedBytes = Company.Exports.ExportUtilities.ToPDF(exporter.GenerateReport(), out mimeType, out extension);

                }

            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Report Processing Failure", "TherapyCorner.Controllers.Report.PDF", ex, context);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {
                if (svcGovt != null) svcGovt.Dispose();
                if (svcClient != null) svcClient.Dispose();
                if (svcService != null) svcService.Dispose();
                if (svcReport != null) svcReport.Dispose();
                if (svcCompany != null) svcCompany.Dispose();

            }
            if (rsp.IsFailure || rsp.Report == null)
            {
                SoundPower.Web.Notifications.AddResponseNotifications(rsp);
                return RedirectToAction("index", "home");

            }


            Response.AddHeader("Content-Disposition",
           "attachment; filename=Report." + extension);

            //Step 7 : Return file content result
            return new FileContentResult(renderedBytes, mimeType);
        }


        [HttpPost]
        [AntiForgeryHandleError]
        public ActionResult Save(PeriodicReport Report,string returnField)
        {
            var token = UserAuthorization.CurrentUser;

            if (ModelState.IsValid)
            {

                Company.PeriodicReportServiceClient svcReport = null;
                ResponseBase rsp = null;
                var context = new Dictionary<string, string>();
                context.Add("Report", Utilities.SerializeDataContractToXML(Report));
                try
                {

                    svcReport = new Company.PeriodicReportServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                    rsp = svcReport.Save(Report);

                }
                catch (Exception ex)
                {
                    var bex = new SoundPower.ErrorTracking.BaseException("Save Processing Failure", "TherapyCorner.Controllers.Report.Save", ex, context);
                    SoundPower.Web.Utilities.ReportError(bex);
                    throw bex;
                }
                finally
                {

                    if (svcReport != null) svcReport.Dispose();

                }
                SoundPower.Web.Notifications.AddResponseNotifications(rsp);
                if (!rsp.IsFailure)
                {
                    SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.ReportPages.SaveSuccess);
                    return RedirectToAction("details", new { id = Report.ReportId, returnField= returnField  });
                }
            }

            try
            {
                SetReportViewBag(Report, token);
            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Validation Return Processing Failure", "TherapyCorner.Controllers.Report.Save", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            ViewBag.ReturnField = returnField;
            return View("details", Report);
        }

        [HttpGet]
        public ActionResult DeActivate()
        {
            SoundPower.Web.Notifications.AddErrorNotification(ResourceText.SharedPages.CouldNotProcessRelogin);
            return RedirectToAction("pending");
        }

        [HttpGet]
        public ActionResult Cancel()
        {
            SoundPower.Web.Notifications.AddErrorNotification(ResourceText.SharedPages.CouldNotProcessRelogin);
            return RedirectToAction("index","home");
        }

        [HttpPost]
        [AntiForgeryHandleError]
        public ActionResult Attach(HttpPostedFileBase file, int? id)
        {
            if (!id.HasValue)
            {

                SoundPower.Web.Notifications.AddErrorNotification(ResourceText.SharedPages.CouldNotProcessRelogin);
                return RedirectToAction("pending");

            }
            bool fileGood = true;
            if (file != null && file.ContentLength > 3000000)
            {
                SoundPower.Web.Notifications.AddErrorNotification(ResourceText.CredentialPages.FileTooLarge);
                fileGood = false;
            }
            if (file == null || file.ContentLength <= 0)
            {
                SoundPower.Web.Notifications.AddErrorNotification(ResourceText.ReportPages.FileMissing);
                fileGood = false;

            }
            if (ModelState.IsValid && fileGood)
            {


                Company.PeriodicReportServiceClient svc = null;
                ResponseBase rsp = null;
                AttachmentInfoRequest req = new AttachmentInfoRequest() { ObjectId = id.ToString(), Attachement = new AttachmentInfo() };


                req.FileData = new byte[file.InputStream.Length];
                file.InputStream.Read(req.FileData, 0, req.FileData.Length);
                req.Attachement.Type = file.ContentType;
                var parts = file.FileName.Split('.');
                req.Attachement.Extension = parts[parts.Length - 1];
                var parts2 = parts[parts.Length - 2].Split('\\');
                req.Attachement.Name = parts2[parts2.Length - 1];
                req.Attachement.Id = "NEW";


                try
                {
                    var token = UserAuthorization.CurrentUser;
                    svc = new Company.PeriodicReportServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                    rsp = svc.AddAttachment(req);

                }
                catch (Exception ex)
                {
                    var bex = new SoundPower.ErrorTracking.BaseException("Save Failure", "TherapyCorner.Controllers.Report.Attach", ex);
                    SoundPower.Web.Utilities.ReportError(bex);
                    throw bex;
                }
                finally
                {
                    if (svc != null) svc.Dispose();
                }


                SoundPower.Web.Notifications.AddResponseNotifications(rsp);
                if (!rsp.IsFailure)
                {

                    SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.ReportPages.FileAttached);
                }


            }

            return RedirectToAction("details", new { id = id });

        }


        public ActionResult RemAttach(string fileId, int? id)
        {
            if (!id.HasValue)
            {

                SoundPower.Web.Notifications.AddErrorNotification(ResourceText.SharedPages.CouldNotProcessRelogin);
                return RedirectToAction("pending");

            }


            Company.PeriodicReportServiceClient svc = null;
            ResponseBase rsp = null;



            try
            {
                var token = UserAuthorization.CurrentUser;
                svc = new Company.PeriodicReportServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                rsp = svc.RemoveAttachment(id.Value, fileId);

            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Save Failure", "TherapyCorner.Controllers.Report.RemAttach", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {
                if (svc != null) svc.Dispose();
            }


            SoundPower.Web.Notifications.AddResponseNotifications(rsp);
            if (!rsp.IsFailure)
            {

                SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.ReportPages.FileRemoved);
            }




            return RedirectToAction("details", new { id = id });

        }

        public ActionResult RemGoal(int goalId, int? id)
        {
            if (!id.HasValue)
            {

                SoundPower.Web.Notifications.AddErrorNotification(ResourceText.SharedPages.CouldNotProcessRelogin);
                return RedirectToAction("pending");

            }


            Company.PeriodicReportServiceClient svc = null;
            ResponseBase rsp = null;



            try
            {
                var token = UserAuthorization.CurrentUser;
                svc = new Company.PeriodicReportServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                rsp = svc.RemoveGoal(id.Value, goalId);

            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Save Failure", "TherapyCorner.Controllers.Report.RemGoal", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {
                if (svc != null) svc.Dispose();
            }


            SoundPower.Web.Notifications.AddResponseNotifications(rsp);
            if (!rsp.IsFailure)
            {

                SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.ReportPages.GoalRemoved);
            }




            return RedirectToAction("details", new { id = id });

        }

        [HttpGet]
        public ActionResult Attachment(int id, string f)
        {



            Company.PeriodicReportServiceClient svc = null;
            www.therapycorner.com.account.MessageContracts.FileResponse rsp = null;

            try
            {
                var token = UserAuthorization.CurrentUser;
                svc = new Company.PeriodicReportServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                rsp = svc.Attachment(id, f);

            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Controllers.Report.Attachment", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
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
            return RedirectToAction("details", new { id = id });


        }

        [HttpPost]
        [AntiForgeryHandleError]
        public ActionResult Submit(PeriodicReport Report)
        {
            var token = UserAuthorization.CurrentUser;

            if (ModelState.IsValid)
            {

                Company.PeriodicReportServiceClient svcReport = null;
                ResponseBase rsp = null;
                var context = new Dictionary<string, string>();
                context.Add("Report", Utilities.SerializeDataContractToXML(Report));
                try
                {

                    svcReport = new Company.PeriodicReportServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                    rsp = svcReport.Submit(Report);

                }
                catch (Exception ex)
                {
                    var bex = new SoundPower.ErrorTracking.BaseException("Save Processing Failure", "TherapyCorner.Controllers.Report.Submit", ex, context);
                    SoundPower.Web.Utilities.ReportError(bex);
                    throw bex;
                }
                finally
                {

                    if (svcReport != null) svcReport.Dispose();

                }
                SoundPower.Web.Notifications.AddResponseNotifications(rsp);
                if (!rsp.IsFailure)
                {
                    SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.ReportPages.SaveSuccess);
                    return RedirectToAction("pending");
                }
            }

            try
            {
                SetReportViewBag(Report, token);
            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Validation Return Processing Failure", "TherapyCorner.Controllers.Report.Submit", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }

            return View("details", Report);
        }
        [HttpGet]
        public ActionResult Submit()
        {
            SoundPower.Web.Notifications.AddErrorNotification(ResourceText.SharedPages.CouldNotProcessRelogin);
            return RedirectToAction("pending");
        }

        [HttpPost]
        [AntiForgeryHandleError]
        public ActionResult Reject(PeriodicReport Report, string RejectReason)
        {
            var token = UserAuthorization.CurrentUser;
            if (ModelState.IsValid)
            {

                Company.PeriodicReportServiceClient svcReport = null;
                ResponseBase rsp = null;
                var context = new Dictionary<string, string>();
                context.Add("reason", RejectReason);
                context.Add("Report", Utilities.SerializeDataContractToXML(Report));
                try
                {

                    svcReport = new Company.PeriodicReportServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                    rsp = svcReport.Reject(new ReportRejectionRequest() { Report = Report, Reason = RejectReason, ObjectId = Report.ReportId });

                }
                catch (Exception ex)
                {
                    var bex = new SoundPower.ErrorTracking.BaseException("Save Processing Failure", "TherapyCorner.Controllers.Report.Reject", ex, context);
                    SoundPower.Web.Utilities.ReportError(bex);
                    throw bex;
                }
                finally
                {

                    if (svcReport != null) svcReport.Dispose();

                }
                SoundPower.Web.Notifications.AddResponseNotifications(rsp);
                if (!rsp.IsFailure)
                {
                    SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.ReportPages.RejectSuccess);
                    return RedirectToAction("pendingreview");
                }
            }

           return RedirectToAction("details", new { id = Report.ReportId });
        }
        [HttpGet]
        public ActionResult Reject()
        {
            SoundPower.Web.Notifications.AddErrorNotification(ResourceText.SharedPages.CouldNotProcessRelogin);
            return RedirectToAction("pendingreview");
        }

        [HttpPost]
        [AntiForgeryHandleError]
        public ActionResult Share(int? id, string[] destination)
        {
            if (!id.HasValue)
            {

                SoundPower.Web.Notifications.AddErrorNotification(ResourceText.SharedPages.CouldNotProcessRelogin);
                return RedirectToAction("index","client");

            }
            var token = UserAuthorization.CurrentUser;
            if (destination == null || destination.Length == 0)
            {
                SoundPower.Web.Notifications.AddErrorNotification(ResourceText.ReportPages.NoDestination);
            }
            else
            {

                Company.PeriodicReportServiceClient svcReport = null;
                ResponseBase rsp = null;
                var context = new Dictionary<string, string>();
                context.Add("Report", id.ToString());
                try
                {

                    svcReport = new Company.PeriodicReportServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                    rsp = svcReport.Share(id.Value, string.Join(",", destination));

                }
                catch (Exception ex)
                {
                    var bex = new SoundPower.ErrorTracking.BaseException("Save Processing Failure", "TherapyCorner.Controllers.Report.Share", ex, context);
                    SoundPower.Web.Utilities.ReportError(bex);
                    throw bex;
                }
                finally
                {

                    if (svcReport != null) svcReport.Dispose();

                }
                SoundPower.Web.Notifications.AddResponseNotifications(rsp);
                if (!rsp.IsFailure)
                {
                    SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.ReportPages.ShareSuccess);
                }
            }



            return RedirectToAction("details", new { id = id });
        }

        [HttpPost]
        [AntiForgeryHandleError]
        public ActionResult Approve(PeriodicReport Report)
        {
            var token = UserAuthorization.CurrentUser;
            if (ModelState.IsValid)
            {

                Company.PeriodicReportServiceClient svcReport = null;
                ResponseBase rsp = null;
                var context = new Dictionary<string, string>();
                context.Add("Report", Utilities.SerializeDataContractToXML(Report));
                try
                {

                    svcReport = new Company.PeriodicReportServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                    rsp = svcReport.Approve(int.Parse(Report.ReportId));

                }
                catch (Exception ex)
                {
                    var bex = new SoundPower.ErrorTracking.BaseException("Save Processing Failure", "TherapyCorner.Controllers.Report.Approve", ex, context);
                    SoundPower.Web.Utilities.ReportError(bex);
                    throw bex;
                }
                finally
                {

                    if (svcReport != null) svcReport.Dispose();

                }
                SoundPower.Web.Notifications.AddResponseNotifications(rsp);
                if (!rsp.IsFailure)
                {
                    SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.ReportPages.ApproveSuccess);
                    return RedirectToAction("pendingreview");
                }
            }

            try
            {
                SetReportViewBag(Report, token);
            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Validation Return Processing Failure", "TherapyCorner.Controllers.Report.Approve", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }

            return View("details", Report);
        }

        [HttpGet]
        public ActionResult Approve()
        {
            SoundPower.Web.Notifications.AddErrorNotification(ResourceText.SharedPages.CouldNotProcessRelogin);
            return RedirectToAction("pendingreview");
        }
        private void SetReportViewBag(PeriodicReport Report, www.therapycorner.com.account.Session token)
        {
            Company.ClientServiceClient svcClient = null;
            Company.GovernmentProgramServiceClient svcGovt = null;

            try
            {

                if ( Report.ApprovedAt.HasValue)
                {
                    svcClient = new Company.ClientServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                    var rspClient = svcClient.Details(int.Parse(Report.Client.UniqueId));

               

                    if (Report.ApprovedAt.HasValue)
                    {
                        www.soundpower.biz.common.GenericEntityList options = new GenericEntityList();
                        if (!string.IsNullOrWhiteSpace(rspClient.Client.DrEmail))
                        {
                            options.Add(new GenericEntity(rspClient.Client.DrEmail, "ShareOption", ResourceText.Dictionary.PHCP));
                        }
                        if (rspClient.Client.GovtProgram != null && rspClient.Client.GovtValues != null)
                        {
                            var fields = StaticData.GovernmentFields(rspClient.Client.GovtProgram.UniqueId);
                            var ft = fields.Find(f => f.Type == ValueTypeEnum.ReferenceList && f.ReferenceList == "CaseWorker");
                            if (ft != null && rspClient.Client.GovtValues.Exists(i => i.FieldId == ft.FieldId))
                            {
                                var cw = rspClient.Client.GovtValues.Find(v => v.FieldId == ft.FieldId);
                                if (!string.IsNullOrWhiteSpace(cw.Value))
                                {
                                    svcGovt = new Company.GovernmentProgramServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                                    var rspGovt = svcGovt.CaseWorkers();
                                    var worker = rspGovt.EntityList.Find(w => w.UniqueId == cw.Value);
                                    options.Add(new GenericEntity(worker.AlternateId, "ShareOption", ResourceText.Dictionary.CaseWorker));
                                }
                            }

                        }
                        if (rspClient.Client.Guardians != null && rspClient.Client.Guardians.Count > 0)
                        {
                            foreach (var g in rspClient.Client.Guardians)
                            {
                                if (!string.IsNullOrWhiteSpace(g.Email)) options.Add(new GenericEntity("G" + g.GuardianId.ToString(), "ShareOption", string.Format("{0} {1}", g.FirstName, g.LastName)));
                            }
                        }
                        ViewBag.ShareOptions = options;

                    }
                }
            }
            catch (Exception ex)
            {
                throw new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Controllers.Report.SetReportViewBag", ex);

            }
            finally
            {

                if (svcClient != null) svcClient.Dispose();
                if (svcGovt != null) svcGovt.Dispose();

            }
        }
    }
}