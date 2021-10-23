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
    public class NotesController : Controller
    {
        [HttpPost]
        public JsonResult PendingCount()
        {
            Company.SessionNoteServiceClient svcClient = null;
            var rsp = new ObjectIdResponseBase(); ;
            JsonResult result = new JsonResult();

            try
            {
                var token = UserAuthorization.CurrentUser;


                svcClient = new Company.SessionNoteServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                var rspSrch = svcClient.Pending();
                rsp.ErrorMessages = rspSrch.ErrorMessages;
                rsp.WarningMessages = rspSrch.WarningMessages;
                rsp.IsFailure = rspSrch.IsFailure;
                rsp.ObjectId = rsp.IsFailure ? "-" : rspSrch.Notes.Count.ToString();
            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Pending Notes Counter Processing Failure", "TherapyCorner.Portal.Controllers.NotesController.PendingCount", ex);
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
            Company.SessionNoteServiceClient svcClient = null;
            var rsp = new ObjectIdResponseBase(); ;
            JsonResult result = new JsonResult();

            try
            {
                var token = UserAuthorization.CurrentUser;


                svcClient = new Company.SessionNoteServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                var rspSrch = svcClient.PendingReview();
                rsp.ErrorMessages = rspSrch.ErrorMessages;
                rsp.WarningMessages = rspSrch.WarningMessages;
                rsp.IsFailure = rspSrch.IsFailure;
                rsp.ObjectId = rsp.IsFailure ? "-" : rspSrch.Notes.Count.ToString();
            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Pending Note Reviews Counter Processing Failure", "TherapyCorner.Portal.Controllers.NotesController.ReviewCount", ex);
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

        // GET: Notes

        [OutputCache(NoStore = true, Duration = 0)]
        public ActionResult Pending()
        {
    
            Company.SessionNoteServiceClient svcNotes = null;
            SessionNoteListResponse rsp = null;
            try
            {
                var token = UserAuthorization.CurrentUser;

                    svcNotes = new Company.SessionNoteServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                    rsp = svcNotes.Pending();

            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Controllers.Notes.Pending", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {
          
                if (svcNotes != null) svcNotes.Dispose();

            }
            SoundPower.Web.Notifications.AddResponseNotifications(rsp);
            if (rsp.IsFailure || rsp.Notes == null) return RedirectToAction("index", "home");
            return View(rsp.Notes);
        }

        [RequireSupervisorFilter]
        public ActionResult PendingReview()
        {

            Company.SessionNoteServiceClient svcNotes = null;
            SessionNoteListResponse rsp = null;
            try
            {
                var token = UserAuthorization.CurrentUser;

                svcNotes = new Company.SessionNoteServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                rsp = svcNotes.PendingReview();

            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Controllers.Notes.PendingReview", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {

                if (svcNotes != null) svcNotes.Dispose();

            }
            SoundPower.Web.Notifications.AddResponseNotifications(rsp);
            if (rsp.IsFailure || rsp.Notes == null) return RedirectToAction("index", "home");
            return View(rsp.Notes);
        }

        [OutputCache(NoStore = true, Duration = 0)]
        public ActionResult Details(long id, string mode, string returnField)
        {
            Company.SessionNoteServiceClient svcNotes = null;
            SessionNoteResponse rsp = null;
            var context = new Dictionary<string, string>();
            context.Add("id", id.ToString());
            try
            {
                var token = UserAuthorization.CurrentUser;

                svcNotes = new Company.SessionNoteServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                if (mode == "a")
                {
                    rsp = svcNotes.Appointment(id);
                }
                else
                {
                    rsp = svcNotes.Details(id);
                }
                if (!rsp.IsFailure && rsp.Note!=null)
                {
                    SetNoteViewBag(rsp.Note, token);
                }

            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Controllers.Notes.Details", ex,context);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {

                if (svcNotes != null) svcNotes.Dispose();

            }
            ViewBag.ReturnField = returnField;

            SoundPower.Web.Notifications.AddResponseNotifications(rsp);
            if (rsp.IsFailure || rsp.Note == null) return RedirectToAction("index", "home");
            return View(rsp.Note);
        }

       [HttpGet]
        public ActionResult CreateGoal(long? id)
        {
            if (!id.HasValue)
            {

                SoundPower.Web.Notifications.AddErrorNotification(ResourceText.SharedPages.CouldNotProcessRelogin);
                return RedirectToAction("pending");

            }

            Company.SessionNoteServiceClient svcNotes = null;
            SessionNoteResponse rsp = null;
            var context = new Dictionary<string, string>();
            context.Add("id", id.ToString());
            try
            {
                var token = UserAuthorization.CurrentUser;

                svcNotes = new Company.SessionNoteServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
             
                    rsp = svcNotes.Details(id.Value);
                
               if (!rsp.IsFailure && rsp.Note!=null)
                {
                    if (!SetClientGoalAreas(int.Parse(rsp.Note.Client.UniqueId),int.Parse(rsp.Note.Service.AlternateId), "")) return RedirectToAction("details");
                }

            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Load Processing Failure", "TherapyCorner.Controllers.Notes.CreateGoal", ex, context);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {

                if (svcNotes != null) svcNotes.Dispose();

            }
            SoundPower.Web.Notifications.AddResponseNotifications(rsp);
            if (rsp.IsFailure || rsp.Note == null) return RedirectToAction("details", new { id = id });

            ViewBag.ApptId = rsp.Note.Appointment.MeetingId;
            ViewBag.NotesId = rsp.Note.NoteId;
            return View(
                new GoalInfo() { Units = "attempts", Success = 3, Attempts = 5 , Area=new GenericEntity("?","GoalArea",null)}
                );
        }

        [HttpPost]
        [AntiForgeryHandleError]
        public ActionResult CreateGoal(GoalInfo request, int clientId, string noteId, long appointmentId, int serviceId)
        {
            ViewBag.ApptId = appointmentId;
            ViewBag.NotesId = noteId;
            if (ModelState.IsValid)
            {
                Company.SessionNoteServiceClient svcStaff = null;
                ResponseBase rspStaff = null;
                try
                {
                    var token = UserAuthorization.CurrentUser;


                    svcStaff = new Company.SessionNoteServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                    rspStaff = svcStaff.CreateGoal(request,long.Parse( noteId));

                }
                catch (Exception ex)
                {
                    var bex = new SoundPower.ErrorTracking.BaseException("Save Processing Failure", "TherapyCorner.Portal.Controllers.Notes.CreateGoal", ex);
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

                    SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.NotePages.GoalCreated);
                    return RedirectToAction("details", new { id = noteId });
                }
                else
                {
                    SiteUtilities.ApplyFieldIssues(rspStaff, ModelState);
                }
            }
            try
            {
                if (!SetClientGoalAreas(clientId, serviceId, request.Area.UniqueId)) return RedirectToAction("details", new { id = noteId });


            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Validation Return Processing Failure", "TherapyCorner.Portal.Controllers.Notes.CreateGoal", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }

            return View(request);
        }

        private bool SetClientGoalAreas(int id,int serviceId, string area = "")
        {
            Company.ClientServiceClient svcClient = null;
            Company.DisciplineServiceClient svcDisc = null;
            Company.ServiceInfoServiceClient svcService = null;
            ClientInfoResponse rspClient = null;
            GenericEntityList areas = new GenericEntityList();
            try
            {
                var token = UserAuthorization.CurrentUser;


                svcClient = new Company.ClientServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                rspClient = svcClient.Details(id);
                svcService = new Company.ServiceInfoServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                svcDisc = new Company.DisciplineServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                var service = svcService.Details(serviceId);

                if (rspClient.Client != null)
                {
        
                        var discipline = svcDisc.Details(int.Parse(service.Service.Discipline.UniqueId));
                        areas.AddRange(discipline.Discipline.GoalAreas);
                    

                }
            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.Notes.SetClientGoalAreas", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {
                if (svcClient != null) svcClient.Dispose();

            }

            SoundPower.Web.Notifications.AddResponseNotifications(rspClient);
            if (rspClient.IsFailure || rspClient.Client == null) return false;
            ViewBag.ServiceId = serviceId;
            ViewBag.ClientId = rspClient.Client.ClientId;
            ViewBag.ClientName = rspClient.Client.ToPerson().LastFirstMI;
            areas.Sort((a, b) => a.Name.CompareTo(b.Name));
            ViewBag.Areas = (from a in areas
                             orderby a.Name
                             select new SelectListItem() { Text = a.Name, Value = a.UniqueId, Selected = a.UniqueId == area }).ToList();
            return true;
        }
        [OutputCache(NoStore = true, Duration = 0)]
        public ActionResult PDF(long id)
        {
            Company.SessionNoteServiceClient svcNotes = null;
            SessionNoteResponse rsp = null;
            var context = new Dictionary<string, string>();
            context.Add("id", id.ToString());
            Company.Exports.SessionNotesExport exporter = null;
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

                svcNotes = new Company.SessionNoteServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                
                    rsp = svcNotes.Details(id);

                if (!rsp.IsFailure && rsp.Note != null)
                {
                    svcService = new Company.ServiceInfoServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                    var rspSvc = svcService.Details(int.Parse(rsp.Note.Service.AlternateId));
                    ViewBag.IsEval = rspSvc.Service.IsEval;
                    svcClient = new Company.ClientServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                    var rspClient = svcClient.Details(int.Parse(rsp.Note.Client.UniqueId));
                    if (rspClient.Client.Diagnosis != null && rspClient.Client.Diagnosis.Count > 0)
                    {
                        var diags = StaticData.Diagnosis;
                        foreach (var d in rspClient.Client.Diagnosis)
                        {
                            d.Name = diags.Find(def => def.UniqueId == d.UniqueId)?.Name;
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
                    svcCompany = new Account.CompanyServiceClient(SiteUtilities.AccountService, token, SiteUtilities.CurrentCulture);
                    var rspCompany = svcCompany.Details(token.CurrentCompany);

                    exporter = new Company.Exports.SessionNotesExport(rsp.Note, rspSvc.Service.IsEval, rspClient.Client, govtFields,rspCompany.Company);
                    renderedBytes = Company.Exports.ExportUtilities.ToPDF(exporter.GenerateReport(), out mimeType, out extension);

                }

            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Report Processing Failure", "TherapyCorner.Controllers.Notes.PDF", ex, context);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {
                if (svcGovt != null) svcGovt.Dispose();
                if (svcClient != null) svcClient.Dispose();
                if (svcService != null) svcService.Dispose();
                if (svcNotes != null) svcNotes.Dispose();
                if (svcCompany != null) svcCompany.Dispose();

            }
            if (rsp.IsFailure || rsp.Note == null)
            {
                SoundPower.Web.Notifications.AddResponseNotifications(rsp);
                return RedirectToAction("index", "home");

            }


            Response.AddHeader("Content-Disposition",
           "attachment; filename=SessionNotes." + extension);

            //Step 7 : Return file content result
            return new FileContentResult(renderedBytes, mimeType);
        }

        [HttpGet]
        public ActionResult Save()
        {
            SoundPower.Web.Notifications.AddErrorNotification(ResourceText.SharedPages.CouldNotProcessRelogin);
            return RedirectToAction("pending");
        }

        [HttpGet]
        public ActionResult SaveTime()
        {
            SoundPower.Web.Notifications.AddErrorNotification(ResourceText.SharedPages.CouldNotProcessRelogin);
            return RedirectToAction("pending");
        }

        [HttpPost]
        [AntiForgeryHandleError]
        public ActionResult Save(SessionNote notes, string[] selectedArea, DateTime startTimeText, DateTime endTimeText,string returnField)
        {
            var token = UserAuthorization.CurrentUser;
            if (selectedArea != null && selectedArea.Length > 0)
            {
                notes.Areas = new GenericEntityList();
                notes.Areas.AddRange(from a in selectedArea
                                     select new GenericEntity(a, "GoalArea", null));
            }
            notes.StartTime = new TimeSpan(startTimeText.Hour, startTimeText.Minute, 0);
            notes.EndTime = new TimeSpan(endTimeText.Hour, endTimeText.Minute, 0);
            if (ModelState.IsValid)
            {
               
                Company.SessionNoteServiceClient svcNotes = null;
                ResponseBase rsp = null;
                var context = new Dictionary<string, string>();
                context.Add("notes", Utilities.SerializeDataContractToXML(notes));
                try
                {

                    svcNotes = new Company.SessionNoteServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                    rsp = svcNotes.Save(notes);

                }
                catch (Exception ex)
                {
                    var bex = new SoundPower.ErrorTracking.BaseException("Save Processing Failure", "TherapyCorner.Controllers.Notes.Save", ex, context);
                    SoundPower.Web.Utilities.ReportError(bex);
                    throw bex;
                }
                finally
                {

                    if (svcNotes != null) svcNotes.Dispose();

                }
                SoundPower.Web.Notifications.AddResponseNotifications(rsp);
                if (!rsp.IsFailure)
                {
                    SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.NotePages.SaveSuccess);
                    return RedirectToAction("details", new { id = notes.Appointment.MeetingId, mode = "a", returnField= returnField  });
                }
            }

            try
            {
                SetNoteViewBag(notes, token);
            }
            catch(Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Validation Return Processing Failure", "TherapyCorner.Controllers.Notes.Save", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            ViewBag.ReturnField = returnField;
            return View("details",notes);
        }

        [HttpPost]
        [AntiForgeryHandleError]
        public ActionResult SaveTime(SessionNote notes, DateTime startTimeText, DateTime endTimeText)
        {
            var token = UserAuthorization.CurrentUser;
            
            notes.StartTime = new TimeSpan(startTimeText.Hour, startTimeText.Minute, 0);
            notes.EndTime = new TimeSpan(endTimeText.Hour, endTimeText.Minute, 0);
            if (ModelState.IsValid)
            {

                Company.SessionNoteServiceClient svcNotes = null;
                ResponseBase rsp = null;
                var context = new Dictionary<string, string>();
                context.Add("notes", Utilities.SerializeDataContractToXML(notes));
                try
                {

                    svcNotes = new Company.SessionNoteServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                    rsp = svcNotes.ChangeTime(notes);

                }
                catch (Exception ex)
                {
                    var bex = new SoundPower.ErrorTracking.BaseException("Save Processing Failure", "TherapyCorner.Controllers.Notes.SaveTime", ex, context);
                    SoundPower.Web.Utilities.ReportError(bex);
                    throw bex;
                }
                finally
                {

                    if (svcNotes != null) svcNotes.Dispose();

                }
                SoundPower.Web.Notifications.AddResponseNotifications(rsp);
                if (!rsp.IsFailure)
                {
                    SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.NotePages.SaveSuccess);
                    return RedirectToAction("details", new { id = notes.NoteId });
                }
            }

            try
            {
                SetNoteViewBag(notes, token);
            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Validation Return Processing Failure", "TherapyCorner.Controllers.Notes.SaveTime", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
           
            return View("details", notes);
        }

        [HttpPost]
        [AntiForgeryHandleError]
        public ActionResult Attach(HttpPostedFileBase file,long? id)
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
            if (file==null || file.ContentLength<=0)
            {
                SoundPower.Web.Notifications.AddErrorNotification(ResourceText.NotePages.FileMissing);
                fileGood = false;

            }
            if (ModelState.IsValid && fileGood )
            {


                Company.SessionNoteServiceClient svc = null;
                ResponseBase rsp = null;
                AttachmentInfoRequest req = new AttachmentInfoRequest() {  ObjectId=id.Value.ToString(), Attachement=new AttachmentInfo() };
               

                    req.FileData = new byte[file.InputStream.Length];
                    file.InputStream.Read(req.FileData, 0, req.FileData.Length);
                    req.Attachement.Type = file.ContentType;
                    var parts = file.FileName.Split('.');
                    req.Attachement.Extension = parts[parts.Length - 1];
                var parts2 = parts[parts.Length - 2].Split('\\');
                req.Attachement.Name = parts2[parts2.Length-1];
                req.Attachement.Id = "NEW";
               
            
            try
                {
                    var token = UserAuthorization.CurrentUser;
                    svc = new Company.SessionNoteServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                    rsp = svc.AddAttachment(req);

                }
                catch (Exception ex)
                {
                    var bex = new SoundPower.ErrorTracking.BaseException("Save Failure", "TherapyCorner.Controllers.Notes.Attach", ex);
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

                    SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.NotePages.FileAttached);
                }
                

            }

            return RedirectToAction("details", new { id = id});

        }


        public ActionResult RemAttach(string fileId, long id)
        {
          


                Company.SessionNoteServiceClient svc = null;
                ResponseBase rsp = null;
                


                try
                {
                    var token = UserAuthorization.CurrentUser;
                    svc = new Company.SessionNoteServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                    rsp = svc.RemoveAttachment(id,fileId);

                }
                catch (Exception ex)
                {
                    var bex = new SoundPower.ErrorTracking.BaseException("Save Failure", "TherapyCorner.Controllers.Notes.RemAttach", ex);
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

                    SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.NotePages.FileRemoved);
                }


            

            return RedirectToAction("details", new { id = id });

        }

        public ActionResult RemGoal(int goalId, long id)
        {



            Company.SessionNoteServiceClient svc = null;
            ResponseBase rsp = null;



            try
            {
                var token = UserAuthorization.CurrentUser;
                svc = new Company.SessionNoteServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                rsp = svc.RemoveGoal(id, goalId);

            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Save Failure", "TherapyCorner.Controllers.Notes.RemGoal", ex);
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

                SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.NotePages.GoalRemoved);
            }




            return RedirectToAction("details", new { id = id });

        }

        [HttpGet]
        public ActionResult Attachment(long id, string f)
        {



            Company.SessionNoteServiceClient svc = null;
            www.therapycorner.com.account.MessageContracts.FileResponse rsp = null;

            try
            {
                var token = UserAuthorization.CurrentUser;
                svc = new Company.SessionNoteServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                rsp = svc.Attachment(id,f);

            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Controllers.Notes.Attachment", ex);
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

                return new FileContentResult(rsp.FileData, rsp.FileType );
            }
            return RedirectToAction("details", new { id = id });


        }

        [HttpPost]
        [AntiForgeryHandleError]
        public ActionResult Submit(SessionNote notes, string[] selectedArea, DateTime startTimeText, DateTime endTimeText)
        {
            var token = UserAuthorization.CurrentUser;
            if (selectedArea != null && selectedArea.Length > 0)
            {
                notes.Areas = new GenericEntityList();
                notes.Areas.AddRange(from a in selectedArea
                                     select new GenericEntity(a, "GoalArea", null));
            }
            notes.StartTime = new TimeSpan(startTimeText.Hour, startTimeText.Minute, 0);
            notes.EndTime = new TimeSpan(endTimeText.Hour, endTimeText.Minute, 0);
            if (ModelState.IsValid)
            {

                Company.SessionNoteServiceClient svcNotes = null;
                ResponseBase rsp = null;
                var context = new Dictionary<string, string>();
                context.Add("notes", Utilities.SerializeDataContractToXML(notes));
                try
                {

                    svcNotes = new Company.SessionNoteServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                    rsp = svcNotes.Submit(notes);

                }
                catch (Exception ex)
                {
                    var bex = new SoundPower.ErrorTracking.BaseException("Save Processing Failure", "TherapyCorner.Controllers.Notes.Submit", ex, context);
                    SoundPower.Web.Utilities.ReportError(bex);
                    throw bex;
                }
                finally
                {

                    if (svcNotes != null) svcNotes.Dispose();

                }
                SoundPower.Web.Notifications.AddResponseNotifications(rsp);
                if (!rsp.IsFailure)
                {
                    SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.NotePages.SaveSuccess);
                    return RedirectToAction("pending");
                }
            }

            try
            {
                SetNoteViewBag(notes, token);
            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Validation Return Processing Failure", "TherapyCorner.Controllers.Notes.Submit", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }

            return View("details", notes);
        }

        [HttpGet]
        public ActionResult Submit()
        {
            SoundPower.Web.Notifications.AddErrorNotification(ResourceText.SharedPages.CouldNotProcessRelogin);
            return RedirectToAction("pending");
        }

        [HttpPost]
        [AntiForgeryHandleError]
        public ActionResult Reject(SessionNote notes)
        {
            var token = UserAuthorization.CurrentUser;
            if (ModelState.IsValid)
            {

                Company.SessionNoteServiceClient svcNotes = null;
                ResponseBase rsp = null;
                var context = new Dictionary<string, string>();
                context.Add("notes", Utilities.SerializeDataContractToXML(notes));
                try
                {

                    svcNotes = new Company.SessionNoteServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                    rsp = svcNotes.Reject(notes);

                }
                catch (Exception ex)
                {
                    var bex = new SoundPower.ErrorTracking.BaseException("Save Processing Failure", "TherapyCorner.Controllers.Notes.Reject", ex, context);
                    SoundPower.Web.Utilities.ReportError(bex);
                    throw bex;
                }
                finally
                {

                    if (svcNotes != null) svcNotes.Dispose();

                }
                SoundPower.Web.Notifications.AddResponseNotifications(rsp);
                if (!rsp.IsFailure)
                {
                    SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.NotePages.RejectSuccess);
                    return RedirectToAction("pendingreview");
                }
            }

            try
            {
                SetNoteViewBag(notes, token);
            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Validation Return Processing Failure", "TherapyCorner.Controllers.Notes.Reject", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }

            return View("details", notes);
        }

        [HttpGet]
        public ActionResult Reject()
        {
            SoundPower.Web.Notifications.AddErrorNotification(ResourceText.SharedPages.CouldNotProcessRelogin);
            return RedirectToAction("pendingreview");
        }

        [HttpPost]
        [AntiForgeryHandleError]
        public ActionResult Share(long? id, string[] destination)
        {
            if (!id.HasValue)
            {

                SoundPower.Web.Notifications.AddErrorNotification(ResourceText.SharedPages.CouldNotProcessRelogin);
                return RedirectToAction("index","client");

            }

            var token = UserAuthorization.CurrentUser;
            if (destination==null || destination.Length==0)
            {
                SoundPower.Web.Notifications.AddErrorNotification(ResourceText.NotePages.NoDestination);
            }
            else
            {

                Company.SessionNoteServiceClient svcNotes = null;
                ResponseBase rsp = null;
                var context = new Dictionary<string, string>();
                context.Add("notes",id.ToString());
                try
                {

                    svcNotes = new Company.SessionNoteServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                    rsp = svcNotes.Share(id.Value,string.Join(",",destination));

                }
                catch (Exception ex)
                {
                    var bex = new SoundPower.ErrorTracking.BaseException("Save Processing Failure", "TherapyCorner.Controllers.Notes.Share", ex, context);
                    SoundPower.Web.Utilities.ReportError(bex);
                    throw bex;
                }
                finally
                {

                    if (svcNotes != null) svcNotes.Dispose();

                }
                SoundPower.Web.Notifications.AddResponseNotifications(rsp);
                if (!rsp.IsFailure)
                {
                    SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.NotePages.ShareSuccess);
                }
            }

          

            return RedirectToAction("details",new { id = id });
        }

        [HttpPost]
        [AntiForgeryHandleError]
        public ActionResult Approve(SessionNote notes)
        {
            var token = UserAuthorization.CurrentUser;
            if (ModelState.IsValid)
            {

                Company.SessionNoteServiceClient svcNotes = null;
                ResponseBase rsp = null;
                var context = new Dictionary<string, string>();
                context.Add("notes", Utilities.SerializeDataContractToXML(notes));
                try
                {

                    svcNotes = new Company.SessionNoteServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                    rsp = svcNotes.Approve(long.Parse(notes.NoteId), notes.ActionType.GetValueOrDefault(ServiceActionEnum.Continue) );

                }
                catch (Exception ex)
                {
                    var bex = new SoundPower.ErrorTracking.BaseException("Save Processing Failure", "TherapyCorner.Controllers.Notes.Approve", ex, context);
                    SoundPower.Web.Utilities.ReportError(bex);
                    throw bex;
                }
                finally
                {

                    if (svcNotes != null) svcNotes.Dispose();

                }
                SoundPower.Web.Notifications.AddResponseNotifications(rsp);
                if (!rsp.IsFailure)
                {
                    SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.NotePages.ApproveSuccess);
                    return RedirectToAction("pendingreview");
                }
            }

            try
            {
                SetNoteViewBag(notes, token);
            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Validation Return Processing Failure", "TherapyCorner.Controllers.Notes.Approve", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }

            return View("details", notes);
        }

        [HttpGet]
        public ActionResult Approve()
        {
            SoundPower.Web.Notifications.AddErrorNotification(ResourceText.SharedPages.CouldNotProcessRelogin);
            return RedirectToAction("pendingreview");
        }

        private void SetNoteViewBag(SessionNote note, www.therapycorner.com.account.Session token)
        {
            Company.ServiceInfoServiceClient svcService = null;
            Company.ClientServiceClient svcClient = null;
            Company.GovernmentProgramServiceClient svcGovt = null;
            Company.DisciplineServiceClient svcDiscipline = null;
            Company.AppointmentServiceClient svcAppt = null;
            bool canOverride = false;
            try
            { 
             svcService = new Company.ServiceInfoServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
            var rspSvc = svcService.Details(int.Parse(note.Service.AlternateId));
            ViewBag.IsEval = rspSvc.Service.IsEval;

                if (!rspSvc.Service.IsEval || note.ApprovedAt.HasValue)
                {
                     svcClient = new Company.ClientServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                    var rspClient = svcClient.Details(int.Parse(note.Client.UniqueId));

                    if (!rspSvc.Service.IsEval)
                    {
                        www.soundpower.biz.common.GenericEntityList areas = new GenericEntityList();
                        GoalInfoList goals = new GoalInfoList();
                        svcDiscipline = new Company.DisciplineServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                        var discipline = svcDiscipline.Details(int.Parse(rspSvc.Service.Discipline.UniqueId));
                        if (note.Areas != null) areas.AddRange(note.Areas);
                        if (rspClient.Client.Goals != null)
                        {
                            foreach (var g in rspClient.Client.Goals)
                            {
                                if (g.Status != GoalStatusEnum.Active || !discipline.Discipline.GoalAreas.Exists(a=>a.UniqueId==g.Area.UniqueId)) continue;
                                goals.Add(g);
                                var ga = areas.Find(a => a.UniqueId == g.Area.UniqueId);
                                if (ga == null)
                                {
                                    areas.Add(g.Area);
                                }
                                else
                                {
                                    ga.Name = g.Area.Name;
                                }
                            }
                        }
                        areas.Sort((a, b) => a.Name.CompareTo(b.Name));
                        ViewBag.Areas = areas;
                        ViewBag.Goals = goals;
                    }

                    if (note.ApprovedAt.HasValue)
                    {
                        if (token.IsAdmin)
                        {
                            svcAppt = new Company.AppointmentServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                            var appt = svcAppt.Details(note.Appointment.MeetingId);
                            canOverride = appt.Appointment.ClaimStatus.GetValueOrDefault(ClaimStatusEnum.Closed) != ClaimStatusEnum.Closed && appt.Appointment.ClaimStatus.GetValueOrDefault(ClaimStatusEnum.Closed) != ClaimStatusEnum.Paid;

                        }
                        www.soundpower.biz.common.GenericEntityList options = new GenericEntityList();
                        if (!string.IsNullOrWhiteSpace(rspClient.Client.DrEmail))
                        {
                            options.Add(new GenericEntity(rspClient.Client.DrEmail, "ShareOption", ResourceText.Dictionary.PHCP));
                        }
                        if (rspClient.Client.GovtProgram != null && rspClient.Client.GovtValues != null)
                        {
                            var fields = StaticData.GovernmentFields(rspClient.Client.GovtProgram.UniqueId);
                            var ft = fields.Find(f => f.Type == ValueTypeEnum.ReferenceList && f.ReferenceList == "CaseWorker");
                            if (ft!=null && rspClient.Client.GovtValues.Exists(i=>i.FieldId==ft.FieldId))
                            { 
                            var cw = rspClient.Client.GovtValues.Find(v => v.FieldId==ft.FieldId);
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
                throw new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Controllers.Notes.SetNoteViewBag", ex);
             
            }
            finally
            {

                if (svcAppt != null) svcAppt.Dispose();
                if (svcService != null) svcService.Dispose();
                if (svcClient != null) svcClient.Dispose();
                if (svcGovt != null) svcGovt.Dispose();
                if (svcDiscipline != null) svcDiscipline.Dispose();

            }
            ViewBag.CanOverride = canOverride;
        }
    }
}