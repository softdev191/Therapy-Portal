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
    [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]

    public class ClientController : Controller
    {
        [HttpGet]
        public ActionResult GoalDetails(int id,int client)
        {
            var token = UserAuthorization.CurrentUser;

            Company.GoalServiceClient svc = null;

            www.therapycorner.com.company.MessageContracts.GoalInfoResponse rsp = null;
            try
            {

                svc = new Company.GoalServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                rsp = svc.Details(id);
            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.Client.GoalDetails", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {
                if (svc != null) svc.Dispose();


            }
            SoundPower.Web.Notifications.AddResponseNotifications(rsp);

            if (rsp.IsFailure || rsp.Goal==null  )
            {
                return RedirectToAction("details", "client",new { id = client });
            }
            ViewBag.ClientId = client;

            return View(rsp.Goal);
        }

        [HttpGet]
        public ActionResult Filter()
        {
            ClientSearchRequest mdl = new ClientSearchRequest();

            mdl.Status = ClientSearchRequest.StatusEnum.NotInactiveOnly;
           

            return Filter(mdl);
        }

        [HttpGet]
        public ActionResult Attach(int id)
        {
            SoundPower.Web.Notifications.AddErrorNotification(ResourceText.SharedPages.CouldNotProcessRelogin);
            return RedirectToAction("details", new { id = id, activeTab =5 });
        }

        [HttpGet]
        public ActionResult RemAttach(int id)
        {
            SoundPower.Web.Notifications.AddErrorNotification(ResourceText.SharedPages.CouldNotProcessRelogin);
            return RedirectToAction("details", new { id = id, activeTab = 5 });
        }

        [HttpPost]
        [AntiForgeryHandleError]
        public ActionResult Attach(HttpPostedFileBase file, int id)
        {
       
            bool fileGood = true;
            if (file != null && file.ContentLength > 10000000)
            {
                SoundPower.Web.Notifications.AddErrorNotification(ResourceText.ClientPages.FileTooLarge);
                fileGood = false;
            }
            if (file == null || file.ContentLength <= 0)
            {
                SoundPower.Web.Notifications.AddErrorNotification(ResourceText.ReportPages.FileMissing);
                fileGood = false;

            }
            if (ModelState.IsValid && fileGood)
            {


                Company.ClientServiceClient svc = null;
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
                    svc = new Company.ClientServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                    rsp = svc.AddAttachment(req);

                }
                catch (Exception ex)
                {
                    var bex = new SoundPower.ErrorTracking.BaseException("Save Failure", "TherapyCorner.Controllers.Client.Attach", ex);
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

                    SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.ClientPages.DocumentAttached);
                }


            }

            return RedirectToAction("details", new { id = id, activeTab = 5 });

        }

        [AntiForgeryHandleError]
        [HttpPost]
        public ActionResult RemAttach(string fileId, int id)
        {
           


            Company.ClientServiceClient svc = null;
            ResponseBase rsp = null;



            try
            {
                var token = UserAuthorization.CurrentUser;
                svc = new Company.ClientServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                rsp = svc.RemoveAttachment(fileId,id);

            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Save Failure", "TherapyCorner.Controllers.Client.RemAttach", ex);
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

                SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.ClientPages.DocumentRemoved);
            }




            return RedirectToAction("details", new { id = id , activeTab=5});

        }

   
        [HttpGet]
        public ActionResult Attachment(int id, string f)
        {



            Company.ClientServiceClient svc = null;
            www.therapycorner.com.account.MessageContracts.FileResponse rsp = null;

            try
            {
                var token = UserAuthorization.CurrentUser;
                svc = new Company.ClientServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                rsp = svc.Attachment(f, id);

            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Controllers.Cleint.Attachment", ex);
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

        [HttpGet]
        public ActionResult Index(int? service, string insurance, int? govt)
        {
            ClientSearchRequest mdl = new ClientSearchRequest();
           
            mdl.Status = ClientSearchRequest.StatusEnum.NotInactiveOnly;
            if (service.HasValue)
            {
                mdl.Services = new List<int>();
                mdl.Services.Add(service.Value);
            }
            if (govt.HasValue)
            {
                mdl.GovernmentPrograms = new List<int>();
                mdl.GovernmentPrograms.Add(govt.Value);
            }
            if (!string.IsNullOrWhiteSpace(insurance))
            {
                mdl.InsuranceCompanies = new List<string>();
                mdl.InsuranceCompanies.Add(insurance);
            }
            
            return Index(mdl);
        }

        [HttpPost]
        [AntiForgeryHandleError]
        [RequireBillerOrWorker]
        public ActionResult AddProgram(int id, int programId)
        {
            Company.ClientServiceClient svcClient = null;
            ResponseBase rsp = null;

            try
            {
                var token = UserAuthorization.CurrentUser;


                svcClient = new Company.ClientServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                rsp = svcClient.SetProgram(id,programId);

            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Save Processing Failure", "TherapyCorner.Portal.Controllers.Client.AddProgram", ex);
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
                return RedirectToAction("details", new { id = id });

            }
            else
            {
                return RedirectToAction("govtvalues", new { id = id });
            }
        }

        [HttpGet]
        public ActionResult AddProgram()
        {
            SoundPower.Web.Notifications.AddErrorNotification(ResourceText.SharedPages.CouldNotProcessRelogin);
            return RedirectToAction("index");
        }

        [HttpPost]
        [AntiForgeryHandleError]
        [RequireBillerOrWorker]
        public ActionResult RemoveProgram(int id)
        {
            Company.ClientServiceClient svcClient = null;
            ResponseBase rsp = null;

            try
            {
                var token = UserAuthorization.CurrentUser;


                svcClient = new Company.ClientServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                rsp = svcClient.SetProgram(id, null);

            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Save Processing Failure", "TherapyCorner.Portal.Controllers.Client.RemoveProgram", ex);
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
                return RedirectToAction("govtvalues", new { id = id });

            }
            else
            {
                return RedirectToAction("details", new { id = id });

            }
        }

        [HttpGet]
        public ActionResult RemoveProgram()
        {
            SoundPower.Web.Notifications.AddErrorNotification(ResourceText.SharedPages.CouldNotProcessRelogin);
            return RedirectToAction("index");
        }

        [HttpPost]
        [RequireBillerOrWorker]
        public JsonResult AddCaseWorker(string name, string email)
        {
            Company.GovernmentProgramServiceClient svcClient = null;
            var rsp = new ObjectIdResponseBase(); ;
            JsonResult result = new JsonResult();

            try
            {
                var token = UserAuthorization.CurrentUser;


                svcClient = new Company.GovernmentProgramServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                rsp = svcClient.AddCaseWorker(email, name);

            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Save Processing Failure", "TherapyCorner.Portal.Controllers.Client.AddProgram", ex);
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
        public ActionResult Index(ClientSearchRequest model)
        {

            Company.ServiceInfoServiceClient svcSvc = null;
            Company.StaffServiceClient svcStaff = null;
            Company.ClientServiceClient svcClient = null;
            List<string> serviceNames = new List<string>();
            List<string> insurances = new List<string>();
            List<string> govts = new List<string>();
            ClientInfoListResponse rspClients = null;
            string staffName = null;
            try
            {
                var token = UserAuthorization.CurrentUser;
                if (model.Provider.HasValue)
                {
                    svcStaff = new Company.StaffServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                    var rStaff = svcStaff.Details(model.Provider.Value);
                    staffName = rStaff.Staff.DisplayName;
                }
                if (model.Services != null && model.Services.Count > 0)
                {
                    svcSvc = new Company.ServiceInfoServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                    var rsp = svcSvc.List(true);
                    if (!rsp.IsFailure && rsp.Services.Count > 0)
                    {
                        serviceNames = (from s in rsp.Services
                                        where model.Services.Contains(s.ServiceId)
                                        select s.Name).ToList();
                    }
                }
                if (model.InsuranceCompanies!=null && model.InsuranceCompanies.Count>0)
                {
                    var ins = StaticData.AllInsuranceCompanies;
                    insurances.AddRange(from i in ins
                                        where model.InsuranceCompanies.Contains(i.CompanyId.ToString())
                                        select i.Name);
                }
                if (model.GovernmentPrograms != null && model.GovernmentPrograms.Count > 0)
                {
                    var g = StaticData.GovernmentPrograms;
                    govts.AddRange(from i in g
                                        where model.GovernmentPrograms.Contains((int)i.ProgramId)
                                        select i.Name);
                    if (model.GovernmentPrograms.Contains(-1)) govts.Add(ResourceText.ClientPages.PrivatePay);
                }
                svcClient = new Company.ClientServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                rspClients = svcClient.Search(model);

            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.Client.Index", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {
                if (svcSvc != null) svcSvc.Dispose();
                if (svcClient != null) svcClient.Dispose();

            }

            SoundPower.Web.Notifications.AddResponseNotifications(rspClients);
            if (rspClients.IsFailure) return RedirectToAction("index", "home");
            ViewBag.ServiceNames = string.Join(",", serviceNames);
            ViewBag.Insurances = string.Join(",", insurances);
            ViewBag.GovtPrograms = string.Join(",", govts);
            ViewBag.Results = rspClients.Clients;
            ViewBag.Staff = staffName;

            if (Request.HttpMethod == "POST" && rspClients.Clients!=null && rspClients.Clients.Count==1)
            {
                return RedirectToAction("details", new { id = rspClients.Clients[0].ClientId });
            }
            return View(model);
        }

        [HttpPost]
        public ActionResult Filter(ClientSearchRequest model)
        {
            ServiceInfoListResponse rsp = null;
            Company.ServiceInfoServiceClient svcService = null;
            Company.InsuranceCompanyServiceClient svcComp = null;
            Company.GovernmentProgramServiceClient svcGovt = null;
            Company.StaffServiceClient svcStaff = null;

            try
            {
                var token = UserAuthorization.CurrentUser;

                svcService = new Company.ServiceInfoServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                rsp = svcService.List(true);
                if (!rsp.IsFailure && rsp.Services.Count > 0)
                {
                   
                    rsp.Services.Sort((a, b) => a.Name.CompareTo(b.Name));
                    ViewBag.Services = rsp.Services;
                }

                svcComp = new Company.InsuranceCompanyServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                var rspIns = svcComp.List(true);
                if (!rspIns.IsFailure && rspIns.Companies.Count > 0)
                {

                    rspIns.Companies.Sort((a, b) => a.Name.CompareTo(b.Name));
                    ViewBag.Insurance = rspIns.Companies;
                }

                svcGovt = new Company.GovernmentProgramServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                var rspGovt = svcGovt.List(true);
                if (!rspGovt.IsFailure && rspGovt.Programs.Count > 0)
                {

                    rspGovt.Programs.Sort((a, b) => a.Name.CompareTo(b.Name));
                  
                }
                else
                {
                    rspGovt.Programs = new ReferencedGovernmentProgramList();
                }
                rspGovt.Programs.Insert(0, new ReferencedGovernmentProgram() { ProgramId = -1, Name = ResourceText.ClientPages.PrivatePay });
                ViewBag.Govt = rspGovt.Programs;

                List<SelectListItem> staff = new List<SelectListItem>();
                staff.Add(new SelectListItem() { Text = "", Value = "" });
                if (token.IsAdmin || token.IsSupervisor || token.IsWorker)
                {
                    svcStaff = new Company.StaffServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                    var rspStaff = svcStaff.Search(new StaffSearchRequest() { Status = StaffSearchRequest.StatusEnum.NotInactiveOnly });
                    rspStaff.Staff.Sort((a, b) => a.DisplayName.CompareTo(b.DisplayName));
                    foreach(var s in rspStaff.Staff)
                    {
                        staff.Add(new SelectListItem() { Text = s.DisplayName, Value = s.StaffId.ToString(), Selected = s.StaffId == model.Provider.GetValueOrDefault(0) });

                    }
                }
                else
                {
                    staff.Add(new SelectListItem() { Text = token.User.FirstMILast, Value = token.User.UniqueId, Selected= token.UserId==model.Provider.GetValueOrDefault(0) });

                }
                ViewBag.Staff = staff;
            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.Client.Filter", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {
                if (svcComp != null) svcComp.Dispose();
                if (svcService != null) svcService.Dispose();
                if (svcGovt != null) svcGovt.Dispose();
                if (svcStaff != null) svcStaff.Dispose();
            }
            return View(model);
        }

        public JsonResult ClientList(string name)
        {
            JsonResult result = new JsonResult();
            result.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            Company.ClientServiceClient svcClient = null;
            ClientInfoListResponse rspClients = null;
            PeopleResponse rsp = new PeopleResponse() { PersonList = new People() };
            try
            {
                var token = UserAuthorization.CurrentUser;
        
               
                svcClient = new Company.ClientServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                rsp = svcClient.NameSearch(name,false);
               
            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.Client.ClientList", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                rsp.IsFailure = true;
                rsp.ErrorMessages.Add(bex.Message);
            }
            finally
            {
                if (svcClient != null) svcClient.Dispose();

            }

            result.Data = rsp;
            return result;
        }

        [RequireWorkerFilter]
        [HttpGet]
        public ActionResult Create()
        {
            return View(new Models.CreateClientModel() {  DoB = DateTime.Today.AddYears(-5) });

        }

        [RequireWorkerFilter]
        [HttpPost]
        [AntiForgeryHandleError]
        public ActionResult Create(Models.CreateClientModel model)
        {
            model.Validate(ModelState);
            if (ModelState.IsValid)
            {
                Company.ClientServiceClient svcClient = null;
                ObjectIdResponseBase rsp = null;

                try
                {
                    var token = UserAuthorization.CurrentUser;


                    svcClient = new Company.ClientServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                    rsp = svcClient.Create(model.ToBase());

                }
                catch (Exception ex)
                {
                    var bex = new SoundPower.ErrorTracking.BaseException("Save Processing Failure", "TherapyCorner.Portal.Controllers.Client.Create", ex);
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
                    SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.ClientPages.Created);
                    return RedirectToAction("details", new { id = rsp.ObjectId });
                }
            }
            return View(model);

        }



        public ActionResult Details(int id, int? activeTab)
        {

            Company.ClientServiceClient svcClient = null;
            Company.GovernmentProgramServiceClient svcGvt = null;
            ClientInfoResponse rspClient = null;
            ViewBag.ActiveTab = activeTab;
            Company.SessionNoteServiceClient svcNotes = null;
            Company.PeriodicReportServiceClient svcReports = null;
            Company.ServiceInfoServiceClient svcServices = null;

            try
            {
                var token = UserAuthorization.CurrentUser;

            
                svcClient = new Company.ClientServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                rspClient = svcClient.Details(id);
                SetDiagnosisNames(rspClient.Client.Diagnosis);

                List<SelectListItem> disciplines = new List<SelectListItem>();
                var minDate = DateTime.Today.AddDays(-7);
                if (rspClient.Client.Services.Exists(s => s.End.GetValueOrDefault(DateTime.Today) > minDate))
                {
                    svcServices = new Company.ServiceInfoServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                    var services = svcServices.List(false);
                    foreach(var cs in rspClient.Client.Services)
                    {
                        if (cs.End.GetValueOrDefault(DateTime.Today) < minDate) continue;
                        var s = services.Services.Find(sd => sd.ServiceId.ToString() == cs.Service.UniqueId);
                        if (!disciplines.Exists(d => d.Value == s.Discipline.UniqueId)) disciplines.Add(new SelectListItem() { Value = s.Discipline.UniqueId, Text = s.Discipline.Name });
                    }
                    disciplines.Sort((a, b) => a.Text.CompareTo(b.Text));
                }
                ViewBag.Disciplines = disciplines;

                svcGvt = new Company.GovernmentProgramServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                var rsp = svcGvt.List(true);
                if (rsp.Programs!=null)
                {
                    rsp.Programs.Sort((a, b) => a.Name.CompareTo(b.Name));
                    ViewBag.GovtPrograms = rsp.Programs;
                }

                if (rspClient.Client.GovtProgram!=null)
                {
                    var auths = svcClient.PaymentAuthorizations(id);
                    ViewBag.Authorizations = auths.Authorizations;
                    var progs = StaticData.GovernmentFields(rspClient.Client.GovtProgram.UniqueId);
                    if (rspClient.Client.GovtValues == null) rspClient.Client.GovtValues = new www.therapycorner.com.account.FieldValueList();
                    foreach(var f in progs)
                    {
                        var v = rspClient.Client.GovtValues.Find(i => i.FieldId == f.FieldId);
                        if (v==null)
                        {
                            rspClient.Client.GovtValues.Add(new www.therapycorner.com.account.FieldValue() { FieldId = f.FieldId, Type = f, Label = f.Label });
                        }
                        else
                        {
                            v.Label = f.Label;
                            v.Type = f;
                            if (f.ReferenceList=="CaseWorker" && !string.IsNullOrWhiteSpace(v.Value))
                            {
                                var workers = svcGvt.CaseWorkers();
                                var cw = workers.EntityList.Find(w => w.UniqueId == v.Value);
                                if (cw != null) v.Value = string.Format("{0} ({1})", cw.Name, cw.AlternateId);
                            }
                        }

                    }
                    rspClient.Client.GovtValues.RemoveAll(i => i.Type == null);
                }

                var notes = new List<Models.NoteListing>();
                svcNotes = new Company.SessionNoteServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                var rspNotes = svcNotes.ClientNotes(id);
                if(!rspNotes.IsFailure && rspNotes.Notes != null)
                {
                    foreach(var n in rspNotes.Notes)
                    {
                        notes.Add(new Models.NoteListing()
                        {
                            Contributors=n.Provider.Name,
                            DetailsUrl=Url.Action("details","notes",new { id=n.NoteId}),
                            NoteDate= n.Appointment.Start.Date,
                            Source=n.Service.Name,
                            Status=n.Status,
                            Type= "Session Notes"
                        });
                    }
                }
                svcReports = new Company.PeriodicReportServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                var rspReports = svcReports.ClientReports(id);
                if (!rspReports.IsFailure && rspReports.Reports != null)
                {
                    foreach (var n in rspReports.Reports)
                    {
                        notes.Add(new Models.NoteListing()
                        {
                            Contributors = string.Join(", ", from c in n.Providers select c.Name),
                            DetailsUrl = Url.Action("details", "report", new { id = n.ReportId }),
                            NoteDate = n.CreatedAt.Date,
                            Source = n.Discipline.Name,
                            Status = n.Status,
                            Type = "Report"
                        });
                    }
                }
                ViewBag.Notes = notes;
            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.Client.Details", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {
                if (svcClient != null) svcClient.Dispose();
                if (svcGvt != null) svcGvt.Dispose();

            }

            SoundPower.Web.Notifications.AddResponseNotifications(rspClient);
            if (rspClient.IsFailure || rspClient.Client==null) return RedirectToAction("index");
            if (rspClient.Client.Services != null)
            {
                if(rspClient.Client.Services.Exists(s => !s.End.HasValue && s.LastAppointment.GetValueOrDefault(DateTime.Today) < DateTime.Today.AddDays(14)))
            {
                    SoundPower.Web.Notifications.AddWarningMessage(ResourceText.ClientPages.ServicesExpiring);
                }
                if (rspClient.Client.Services.Exists(s => s.ApprovedFrom.HasValue && s.Start < s.ApprovedFrom.Value))
                {
                    SoundPower.Web.Notifications.AddWarningMessage(ResourceText.ClientPages.ServicesBeforeAuth);
                }
            }
            ViewBag.IsTelehealth = ManageTelehealth(id, false);
            return View(rspClient.Client);
        }


        public ActionResult Schedule(int id)
        {

            Company.ClientServiceClient svcClient = null;
            ClientInfoResponse rspClient = null;

            try
            {
                var token = UserAuthorization.CurrentUser;


                svcClient = new Company.ClientServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                rspClient = svcClient.Details(id);
               
            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.Client.Schedule", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {
                if (svcClient != null) svcClient.Dispose();

            }

            SoundPower.Web.Notifications.AddResponseNotifications(rspClient);
            if (rspClient.IsFailure || rspClient.Client == null) return RedirectToAction("index");
            return View(rspClient.Client);
        }


        [HttpGet]
        [RequireBillerOrWorker]
        public ActionResult GovtValues(int? id)
        {
            if (!id.HasValue)
            {

                SoundPower.Web.Notifications.AddErrorNotification(ResourceText.SharedPages.CouldNotProcessRelogin);
                return RedirectToAction("index");

            }
            Company.ClientServiceClient svcClient = null;
            ClientInfoResponse rspClient = null;
             Company.GovernmentProgramServiceClient svcGovt = null;
           try
            {
                var token = UserAuthorization.CurrentUser;


                svcClient = new Company.ClientServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                rspClient = svcClient.Details(id.Value);
                if (rspClient.Client.GovtProgram == null) return RedirectToAction("details", new { id = id });
                if (rspClient.Client.GovtValues == null) rspClient.Client.GovtValues = new www.therapycorner.com.account.FieldValueList();
                ViewBag.ClientId = id;
                ViewBag.ClientName = rspClient.Client.ToPerson().LastFirst;
                ViewBag.ProgramId = rspClient.Client.GovtProgram.UniqueId;
                var values = rspClient.Client.GovtValues;
                svcGovt = SetGovtValueLists(rspClient.Client.GovtProgram.UniqueId, token, values);
            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.Client.GovtValues", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {
                if (svcClient != null) svcClient.Dispose();
                if (svcGovt != null) svcGovt.Dispose();

            }

            SoundPower.Web.Notifications.AddResponseNotifications(rspClient);
            if (rspClient.IsFailure || rspClient.Client == null) return RedirectToAction("index");
            return View(rspClient.Client.GovtValues);
        }

        [HttpPost]
        [RequireBillerOrWorker]
        [AntiForgeryHandleError]
        public ActionResult RemAuth(int authId, int id)
        {
            
            Company.ClientServiceClient svcClient = null;
            try
            {
                var token = UserAuthorization.CurrentUser;


                svcClient = new Company.ClientServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                var rsp = svcClient.RemovePaymentAuthorization(authId);
                SoundPower.Web.Notifications.AddResponseNotifications(rsp);
          
             
            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Loading Failure", "TherapyCorner.Portal.Controllers.Client.RemAuth", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {
                if (svcClient != null) svcClient.Dispose();

            }

            return RedirectToAction("details", new { id = id });
           
        }


        [HttpGet]
        [RequireBillerOrWorker]
        public ActionResult AddAuth(int? id)
        {
            if (!id.HasValue)
            {

                SoundPower.Web.Notifications.AddErrorNotification(ResourceText.SharedPages.CouldNotProcessRelogin);
                return RedirectToAction("index");

            }
            Company.ClientServiceClient svcClient = null;
            ClientInfoResponse rspClient = null;
            PaymentAuthorization auth = new PaymentAuthorization() { AuthId = -1, Start = DateTime.Today, End = DateTime.Today.AddDays(90), ServiceCategory = new GenericEntity("0", "GovtCat", null), Client = new GenericEntity(id.Value.ToString(), "Client", null) };
            try
            {
                var token = UserAuthorization.CurrentUser;


                svcClient = new Company.ClientServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                rspClient = svcClient.Details(id.Value);
                if (rspClient.Client.GovtProgram == null) return RedirectToAction("details", new { id = id });

                auth.Client.Name = rspClient.Client.ToPerson().LastFirstMI;
                auth.Client.AlternateId = rspClient.Client.GovtProgram.UniqueId;

            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Loading Failure", "TherapyCorner.Portal.Controllers.Client.AddAuth", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {
                if (svcClient != null) svcClient.Dispose();

            }

            SoundPower.Web.Notifications.AddResponseNotifications(rspClient);
            if (rspClient.IsFailure || rspClient.Client == null) return RedirectToAction("index");
            return View(auth);
        }

        [HttpPost]
        [RequireBillerOrWorker]
        [AntiForgeryHandleError]
        public ActionResult AddAuth(PaymentAuthorization auth)
        {
            if (ModelState.IsValid)
            {


                Company.ClientServiceClient svcClient = null;
                ResponseBase rspClient = null;
                try
                {
                    var token = UserAuthorization.CurrentUser;


                    svcClient = new Company.ClientServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                    rspClient = svcClient.AddPaymentAuthorization(auth);
                   

                }
                catch (Exception ex)
                {
                    var bex = new SoundPower.ErrorTracking.BaseException("Loading Failure", "TherapyCorner.Portal.Controllers.Client.AddAuth", ex);
                    SoundPower.Web.Utilities.ReportError(bex);
                    throw bex;
                }
                finally
                {
                    if (svcClient != null) svcClient.Dispose();

                }

                SoundPower.Web.Notifications.AddResponseNotifications(rspClient);
                if (!rspClient.IsFailure)
                {
                    return RedirectToAction("details", new { id = auth.Client.UniqueId, activeTab = 8 });
                }
            }

            return View(auth);
        }

        private Company.GovernmentProgramServiceClient SetGovtValueLists(string programId, www.therapycorner.com.account.Session token, www.therapycorner.com.account.FieldValueList values)
        {
            Company.GovernmentProgramServiceClient svcGovt;
            var fields = StaticData.GovernmentFields(programId);

            foreach (var f in fields)
            {
                var v = values.Find(i => i.FieldId == f.FieldId);
                if (v != null)
                {
                    v.Type = f;
                }
                else
                {
                    values.Add(new www.therapycorner.com.account.FieldValue() { FieldId = f.FieldId, Type = f, Label = f.Label, Version = "NEW" });
                }
            }

            svcGovt = new Company.GovernmentProgramServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
            var rsp = svcGovt.CaseWorkers();
            ViewBag.CaseWorkers = rsp.EntityList;
            return svcGovt;
        }

        [HttpPost]
        [RequireBillerOrWorker]
        public ActionResult GovtValues(int id,string nme, string programId, www.therapycorner.com.account.FieldValueList GovtValues)
        {
            if (ModelState.IsValid)
            {
                Company.ClientServiceClient svcClient = null;
                Company.GovernmentProgramServiceClient svcGovt = null;
                ResponseBase rspClient = null;
                try
                {
                    var token = UserAuthorization.CurrentUser;


                    svcClient = new Company.ClientServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);

                    rspClient=svcClient.SetProgramValues(id, GovtValues);

                    if (rspClient.IsFailure) SetGovtValueLists(programId, token, GovtValues);
                }
                catch (Exception ex)
                {
                    var bex = new SoundPower.ErrorTracking.BaseException("Save Processing Failure", "TherapyCorner.Portal.Controllers.Client.GovtValues", ex);
                    SoundPower.Web.Utilities.ReportError(bex);
                    throw bex;
                }
                finally
                {
                    if (svcClient != null) svcClient.Dispose();
                    if (svcGovt != null) svcGovt.Dispose();

                }

                SoundPower.Web.Notifications.AddResponseNotifications(rspClient);
                if(!rspClient.IsFailure)
                {
                    SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.ClientPages.GovtValuesUpdated);
                    return RedirectToAction("details", new { id = id });
                }
                else
                {
                    SiteUtilities.ApplyFieldIssues(rspClient, ModelState);
                }
            }
            ViewBag.ClientId = id;
            ViewBag.ClientName = nme;
            ViewBag.ProgramId = programId;
            return View(GovtValues);
        }
        [HttpGet]
        [RequireWorkerFilter]
        public ActionResult Update(int? id)
        {
            if (!id.HasValue)
            {

                SoundPower.Web.Notifications.AddErrorNotification(ResourceText.SharedPages.CouldNotProcessRelogin);
                return RedirectToAction("index");

            }
            Company.ClientServiceClient svcClient = null;
            ClientInfoResponse rspClient = null;
            try
            {
                var token = UserAuthorization.CurrentUser;


                svcClient = new Company.ClientServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                rspClient = svcClient.Details(id.Value);
                SetDiagnosisNames(rspClient.Client.Diagnosis);
             

            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.Client.Update", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {
                if (svcClient != null) svcClient.Dispose();

            }

            SoundPower.Web.Notifications.AddResponseNotifications(rspClient);
            if (rspClient.IsFailure || rspClient.Client == null) return RedirectToAction("index");
            return View(rspClient.Client);
        }

        [HttpGet]
        [RequireSupervisorFilter]
        public ActionResult CreateGoal(int? id)
        {
            if (!id.HasValue)
            {

                SoundPower.Web.Notifications.AddErrorNotification(ResourceText.SharedPages.CouldNotProcessRelogin);
                return RedirectToAction("index");

            }
            try
            {
                if (!SetClientGoalAreas(id.Value)) return RedirectToAction("index");


            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Load Processing Failure", "TherapyCorner.Portal.Controllers.Client.CreateGoal", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
      

       

            return View(new GoalInfo() { Units = "attempts", Success = 3, Attempts = 5 , Area=new GenericEntity("?","GoalArea",null)} );
        }


        [HttpPost]
        [AntiForgeryHandleError]
        [RequireSupervisorFilter]
        public ActionResult CreateGoal(GoalInfo request, int clientId)
        {
            if (ModelState.IsValid)
            {
                Company.GoalServiceClient svcStaff = null;
                ResponseBase rspStaff = null;
                try
                {
                    var token = UserAuthorization.CurrentUser;


                    svcStaff = new Company.GoalServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                    rspStaff = svcStaff.Create(request,clientId);

                }
                catch (Exception ex)
                {
                    var bex = new SoundPower.ErrorTracking.BaseException("Save Processing Failure", "TherapyCorner.Portal.Controllers.Client.CreateGoal", ex);
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

                    SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.ClientPages.GoalCreated);
                    return RedirectToAction("details", new { id = clientId });
                }
                else
                {
                    SiteUtilities.ApplyFieldIssues(rspStaff, ModelState);
                }
            }
            try
            {
                if (!SetClientGoalAreas(clientId,request.Area.UniqueId)) return RedirectToAction("index");


            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Validation Return Processing Failure", "TherapyCorner.Portal.Controllers.Client.CreateGoal", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }

            return View(request);
        }
        private  bool SetClientGoalAreas(int id, string area="")
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
                var serviceList = svcService.List(false);

                if (rspClient.Client!= null && rspClient.Client.Services!=null)
                {
                    var disciplines = (from s in rspClient.Client.Services
                                       join sd in serviceList.Services on s.Service.UniqueId equals sd.ServiceId.ToString()
                                    where !s.End.HasValue
                                    select int.Parse(sd.Discipline.UniqueId)).Distinct();
                    foreach(var d in disciplines)
                    {
                        var discipline = svcDisc.Details(d);
                        areas.AddRange(discipline.Discipline.GoalAreas);
                    }

                }
            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.Client.SetClientGoalAreas", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {
                if (svcClient != null) svcClient.Dispose();

            }

            SoundPower.Web.Notifications.AddResponseNotifications(rspClient);
            if (rspClient.IsFailure || rspClient.Client == null) return false;

            ViewBag.ClientId = rspClient.Client.ClientId;
            ViewBag.ClientName = rspClient.Client.ToPerson().LastFirstMI;
            areas.Sort((a, b) => a.Name.CompareTo(b.Name));
            ViewBag.Areas = (from a in areas
                             orderby a.Name 
                             select new SelectListItem() { Text = a.Name, Value = a.UniqueId, Selected = a.UniqueId == area }).ToList() ;
            return true;
        }

        [RequireWorkerFilter]
        [HttpPost]
        [AntiForgeryHandleError]
        public ActionResult Update(ClientInfo model)
        {
            if (ModelState.IsValid)
            {
                Company.ClientServiceClient svcClient = null;
                ResponseBase rsp = null;

                try
                {
                    var token = UserAuthorization.CurrentUser;


                    svcClient = new Company.ClientServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                    rsp = svcClient.Update(model);

                }
                catch (Exception ex)
                {
                    var bex = new SoundPower.ErrorTracking.BaseException("Save Processing Failure", "TherapyCorner.Portal.Controllers.Client.Update", ex);
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
                    SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.ClientPages.Updated);
                    return RedirectToAction("details", new { id = model.ClientId });
                }
            }
            return View(model);

        }

        [RequireWorkerFilter]
        public ActionResult Comment(int id, string comment)
        {
            if (!string.IsNullOrWhiteSpace(comment))
            {
                Company.ClientServiceClient svcClient = null;
                ResponseBase rsp = null;

                try
                {
                    var token = UserAuthorization.CurrentUser;


                    svcClient = new Company.ClientServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                    rsp = svcClient.Comment(new CommentRequest() { CommentText = comment, Id = id });

                }
                catch (Exception ex)
                {
                    var bex = new SoundPower.ErrorTracking.BaseException("Save Processing Failure", "TherapyCorner.Portal.Controllers.Client.Comment", ex);
                    SoundPower.Web.Utilities.ReportError(bex);
                    throw bex;
                }
                finally
                {
                    if (svcClient != null) svcClient.Dispose();

                }

                SoundPower.Web.Notifications.AddResponseNotifications(rsp);

                if (!rsp.IsFailure)
                {
     
                    SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.ClientPages.Commented);
                }
                     
           }
            return RedirectToAction("details", new { id = id, activeTab = 4 });
        }

        [RequireBillerOrWorker]
        [HttpPost]
        public JsonResult QuickComment(int id, string comment)
        {
            JsonResult result = new JsonResult();
                ResponseBase rsp = new ResponseBase();
            if (!string.IsNullOrWhiteSpace(comment))
            {
                Company.ClientServiceClient svcClient = null;

                try
                {
                    var token = UserAuthorization.CurrentUser;


                    svcClient = new Company.ClientServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                    rsp = svcClient.Comment(new CommentRequest() { CommentText = comment, Id = id });

                }
                catch (Exception ex)
                {
                    var bex = new SoundPower.ErrorTracking.BaseException("Save Processing Failure", "TherapyCorner.Portal.Controllers.Client.Comment", ex);
                    SoundPower.Web.Utilities.ReportError(bex);
                    throw bex;
                }
                finally
                {
                    if (svcClient != null) svcClient.Dispose();

                }

                SoundPower.Web.Notifications.AddResponseNotifications(rsp);

                if (!rsp.IsFailure)
                {

                    SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.ClientPages.Commented);
                }

            }
            else
            {
                rsp.IsFailure = true;
                rsp.ErrorMessages.Add(ResourceText.ClientPages.CommentTextRequired);
            }
            result.Data = rsp;
            return result;
        }

        [HttpGet]
        [RequireWorkerFilter]
        public ActionResult MakeAppt(int? id)
        {
            if (!id.HasValue)
            {

                SoundPower.Web.Notifications.AddErrorNotification(ResourceText.SharedPages.CouldNotProcessRelogin);
                return RedirectToAction("index");

            }
            Company.ClientServiceClient svcStaff = null;
            ClientInfoResponse rspStaff = null;
            try
            {
                var token = UserAuthorization.CurrentUser;


                svcStaff = new Company.ClientServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                rspStaff = svcStaff.Details(id.Value);
                ViewBag.ClientId = rspStaff.Client.ClientId;
                ViewBag.ClientName = rspStaff.Client.ToPerson().LastFirstMI;
            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.Client.MakeAppt", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {
                if (svcStaff != null) svcStaff.Dispose();

            }

            return View(new CreateAppointmentRequest() { ClientServiceId = 0, InitialDate = DateTime.Today.AddDays(1), MeetingCount = 1, StartTime = new TimeSpan(8, 0, 0) });
        }

        [HttpPost]
        [AntiForgeryHandleError]
        [RequireWorkerFilter]
        public ActionResult MakeAppt(CreateAppointmentRequest request, int clientId, string clientName)
        {
            ViewBag.ClientId = clientId;
            ViewBag.ClientName = clientName;
            if (ModelState.IsValid)
            {
                Company.AppointmentServiceClient svcStaff = null;
                ResponseBase rspStaff = null;
                try
                {
                    var token = UserAuthorization.CurrentUser;


                    svcStaff = new Company.AppointmentServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                    rspStaff = svcStaff.Create(request);

                }
                catch (Exception ex)
                {
                    var bex = new SoundPower.ErrorTracking.BaseException("Save Processing Failure", "TherapyCorner.Portal.Controllers.Client.MakeAppt", ex);
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

                    SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.StaffPages.AppointmentsScheduled);
                    return RedirectToAction("schedule", new { id = clientId  });
                }
                else
                {
                    SiteUtilities.ApplyFieldIssues(rspStaff, ModelState);
                }
            }
          
            return View(request);
        }

        [HttpPost]
        [AntiForgeryHandleError]
        public ActionResult CreateReport(int discipline, PeriodicReportTypeEnum type , int clientId)
        {
     
            if (ModelState.IsValid)
            {
                Company.PeriodicReportServiceClient svcStaff = null;
                ResponseBase rspStaff = null;
                try
                {
                    var token = UserAuthorization.CurrentUser;


                    svcStaff = new Company.PeriodicReportServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                    rspStaff = svcStaff.Create(clientId,discipline,type);

                }
                catch (Exception ex)
                {
                    var bex = new SoundPower.ErrorTracking.BaseException("Save Processing Failure", "TherapyCorner.Portal.Controllers.Client.CreateReport", ex);
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

                    SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.ClientPages.ReportCreated);
                }
              
            }

            return RedirectToAction("details", new { id = clientId, activeTab = 8 });
        }

        private static void SetDiagnosisNames(GenericEntityList diagnosis)
        {
            if (diagnosis == null || diagnosis.Count == 0) return;

            var defs = StaticData.Diagnosis;

            foreach(var d in diagnosis)
            {
                var i = defs.Find(df => df.UniqueId == d.UniqueId);
                d.Name = i?.Name;
            }
        }

        [HttpPost]
        public JsonResult UpdateTeleHealth(int clientId, bool isTelehealth)
        {
            bool rsp = ManageTelehealth(clientId, isTelehealth,true);
            return Json(rsp, JsonRequestBehavior.AllowGet);
        }

        private  bool ManageTelehealth(int clientId, bool isTelehealth, bool isUpdate = false)
        {
            Company.ClientServiceClient svcClient = null;
            var token = UserAuthorization.CurrentUser;
            svcClient = new Company.ClientServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
             return svcClient.UpdateTelehealth(clientId, isTelehealth, isUpdate);
        }
    }
}