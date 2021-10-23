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
    public class StaffController : Controller
    {
        // GET: Staff
        [RequireStaffManagerFilter]
        [HttpGet]
        public ActionResult Index(int? discipline, int? services, bool? provider, bool? supervisor)
        {
            StaffSearchRequest mdl = new StaffSearchRequest();
            if (discipline.HasValue)
            {
                mdl.Disciplines = new List<int>();
                mdl.Disciplines.Add(discipline.Value);
            }
            if (services.HasValue)
            {
                mdl.Services = new List<int>();
                mdl.Services.Add(services.Value);
            }
            if (provider.HasValue)
            {
                mdl.Provider = provider.Value;
            }
            if (supervisor.HasValue)
            {
                mdl.Supervisor = supervisor.Value;
            }
            mdl.Status = StaffSearchRequest.StatusEnum.NotInactiveOnly;

            return Index(mdl);
        }

        [RequireStaffManagerFilter]
        [HttpPost]
        public ActionResult Index(StaffSearchRequest model)
        {

            Company.ServiceInfoServiceClient svcSvc = null;
            Company.DisciplineServiceClient svcD = null;
            Company.StaffServiceClient svcStaff = null;
            List<string> serviceNames = new List<string>();
            List<string> disciplineNames = new List<string>();
            StaffInfoListResponse rspStaff = null;
                 var token = UserAuthorization.CurrentUser;

           try
            {
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
                if (model.Disciplines  != null && model.Disciplines.Count > 0)
                {
                    svcD = new Company.DisciplineServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                    var rsp = svcD.List();
                    if (!rsp.IsFailure && rsp.Disciplines.Count > 0)
                    {
                        disciplineNames = (from s in rsp.Disciplines
                                        where model.Disciplines.Contains(s.DisciplineId)
                                        select s.Name).ToList();
                    }
                }
                svcStaff = new Company.StaffServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                rspStaff = svcStaff.Search(model);

            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.Staff.Index", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {
                if (svcSvc != null) svcSvc.Dispose();
                if (svcStaff != null) svcStaff.Dispose();
                if (svcD != null) svcD.Dispose();

            }

            //If just a supervisor remove everyone that does not report to the user
            if (!token.IsAdmin && !token.IsWorker && rspStaff.Staff !=null && rspStaff.Staff.Count>0)
            {
                rspStaff.Staff.RemoveAll(s => s.Supervisor == null || s.Supervisor.AlternateId != token.User.UniqueId);
            }
            SoundPower.Web.Notifications.AddResponseNotifications(rspStaff);
            if (rspStaff.IsFailure) return RedirectToAction("index", "home");
            ViewBag.ServiceNames = string.Join(",", serviceNames);
            ViewBag.DisciplineNames = string.Join(",", disciplineNames);
            ViewBag.Results = rspStaff.Staff;
            return View(model);
        }

        [RequireStaffManagerFilter]
        [HttpPost]
        public ActionResult Filter(StaffSearchRequest model)
        {
            ServiceInfoListResponse rsp = null;
            Company.ServiceInfoServiceClient svcComp = null;
            Company.DisciplineServiceClient  svcD = null;
            try
            {
                var token = UserAuthorization.CurrentUser;

                svcComp = new Company.ServiceInfoServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                rsp = svcComp.List(true);
                if (!rsp.IsFailure && rsp.Services.Count > 0)
                {
                    rsp.Services.Sort((a, b) => a.Name.CompareTo(b.Name));
                    ViewBag.Services = rsp.Services;
                }

                svcD = new Company.DisciplineServiceClient (SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                var rspD = svcD.List();
                if (!rspD.IsFailure && rspD.Disciplines.Count > 0)
                {
                    rspD.Disciplines.Sort((a, b) => a.Name.CompareTo(b.Name));
                    ViewBag.Disciplines = rspD.Disciplines;
                }
            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.Staff.Filter", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {
                if (svcComp != null) svcComp.Dispose();
                if (svcD != null) svcD.Dispose();
            }
            return View(model);
        }

        [RequireStaffManagerFilter]
        [HttpGet]
        public ActionResult Filter()
        {
            StaffSearchRequest mdl = new StaffSearchRequest();
           
            mdl.Status = StaffSearchRequest.StatusEnum.NotInactiveOnly;

            return Filter(mdl);
        }


               [RequireStaffManagerFilter]
        public ActionResult Details(int id, int? activeTab)
        {
            Company.StaffServiceClient svcStaff = null;
            Account.UserServiceClient svcUser = null;
            StaffInfoResponse rspStaff = null;
                  var token = UserAuthorization.CurrentUser;
          ViewBag.ActiveTab = activeTab;
            try
            {

                
                svcStaff = new Company.StaffServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                rspStaff = svcStaff.Details(id);
                ViewBag.Fields = svcStaff.FieldList().Fields;

                if (rspStaff.Staff.User != null)
                {
                    svcUser = new Account.UserServiceClient(SiteUtilities.AccountService, token, SiteUtilities.CurrentCulture);
                    if (rspStaff.Staff.Status == StaffInfo.StatusEnum.Inactive)
                    {
                        ViewBag.User = new www.therapycorner.com.account.UserInfo()
                        {
                            UserId= int.Parse(rspStaff.Staff.User.UniqueId),
                            FirstName=rspStaff.Staff.User.FirstName,
                            MiddleName=rspStaff.Staff.User.MiddleName,
                            LastName =rspStaff.Staff.User.LastName,
                            Suffix=rspStaff.Staff.User.Suffix,
                            DoB = DateTime.MinValue,
                            
                        };
                    }
                    else
                    {
                        var rspUser = svcUser.Details(int.Parse(rspStaff.Staff.User.UniqueId));
                       
                        ViewBag.User = rspUser.User;
                    }
            
                }
            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.Staff.Details", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {
                if (svcStaff != null) svcStaff.Dispose();

            }

            if (rspStaff.Staff != null)
            {
                if (rspStaff.Staff.ReportCount.GetValueOrDefault(0) > 0) SoundPower.Web.Notifications.AddWarningMessage(string.Format(ResourceText.StaffPages.ReportCountWarning, rspStaff.Staff.ReportCount.GetValueOrDefault(0).ToString()));
                if (!token.IsAdmin && !token.IsWorker && (rspStaff.Staff.Supervisor==null || rspStaff.Staff.Supervisor.AlternateId!= token.User.UniqueId) )
                {
                    SoundPower.Web.Notifications.AddErrorNotification(ResourceText.Messages.AccessDenied);
                    return RedirectToAction("index");
                }
                
                }
            SoundPower.Web.Notifications.AddResponseNotifications(rspStaff);
            return View(rspStaff.Staff);
        }

        [RequireAdmin]
        [HttpGet]
        public ActionResult EditPayRate(int? id, int staff)
        {
            if (!id.HasValue)
            {

                SoundPower.Web.Notifications.AddErrorNotification(ResourceText.SharedPages.CouldNotProcessRelogin);
                return RedirectToAction("index");

            }

            Company.StaffServiceClient svcStaff = null;
            StaffInfoResponse rspStaff = null;
            PayrollRate rate = null;
            try
            {
                var token = UserAuthorization.CurrentUser;


                svcStaff = new Company.StaffServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                rspStaff = svcStaff.Details(staff);
                rate = rspStaff.Staff.PayRates.Find(r => r.PayRateId == id);
                if (rate != null) rate.Staff = new GenericEntity(staff.ToString(), "Staff", rspStaff.Staff.DisplayName);
            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Get Processing Failure", "TherapyCorner.Portal.Controllers.Staff.EditPayRate", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {
                if (svcStaff != null) svcStaff.Dispose();

            }

            SoundPower.Web.Notifications.AddResponseNotifications(rspStaff);
            if (rspStaff.IsFailure || rate==null)
            {
                return RedirectToAction("details", new { id = staff });
            }
            return View(rate);
        }

        [RequireAdmin]
        [HttpPost]
        public ActionResult EditPayRate(PayrollRate rate)
        {
            if (ModelState.IsValid)
            {
                Company.StaffServiceClient svcStaff = null;
                ResponseBase rspStaff = null;
                try
                {
                    var token = UserAuthorization.CurrentUser;


                    svcStaff = new Company.StaffServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                    rspStaff = svcStaff.SetPayRate(rate);
                
                }
                catch (Exception ex)
                {
                    var bex = new SoundPower.ErrorTracking.BaseException("Save Processing Failure", "TherapyCorner.Portal.Controllers.Staff.EditPayRate", ex);
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
                    SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.StaffPages.PayRateSuccess);
                    return RedirectToAction("details", new { id = rate.Staff.UniqueId, activeTab=2 });
                }
               
            }
            return View(rate);
        }

        [RequireAdmin]
        public ActionResult OIG()
        {
            Account.UserServiceClient svcUser = null;
            www.therapycorner.com.account.MessageContracts.OIGMatchListResponse rspStaff = null;
            try
            {
                var token = UserAuthorization.CurrentUser;

                    svcUser = new Account.UserServiceClient(SiteUtilities.AccountService, token, SiteUtilities.CurrentCulture);
                rspStaff = svcUser.OIGMatches();
             
            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.Staff.OIG", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {
                if (svcUser != null) svcUser.Dispose();

            }

            SoundPower.Web.Notifications.AddResponseNotifications(rspStaff);
            return View(rspStaff.Matches);
        }

        [RequireAdmin]
        public ActionResult OIGExport()
        {
            Account.UserServiceClient svcUser = null;
            www.therapycorner.com.account.MessageContracts.OIGMatchListResponse rspStaff = null;
            byte[] renderedBytes = null;
            string mimeType = null;
            string extension = null;

            try
            {
                var token = UserAuthorization.CurrentUser;

                svcUser = new Account.UserServiceClient(SiteUtilities.AccountService, token, SiteUtilities.CurrentCulture);
                rspStaff = svcUser.OIGMatches();
                if (!rspStaff.IsFailure)
                {
                    var exporter = new Company.Exports.OIGExport(rspStaff.Matches, token.CompanyName);
                    renderedBytes = Company.Exports.ExportUtilities.ToPDF(exporter.GenerateReport(), out mimeType, out extension);
                }
            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.Staff.OIGExport", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {
                if (svcUser != null) svcUser.Dispose();

            }

            if (rspStaff.IsFailure || rspStaff.Matches == null)
            {
                SoundPower.Web.Notifications.AddResponseNotifications(rspStaff);
                return RedirectToAction("index", "home");

            }


            Response.AddHeader("Content-Disposition",
           "attachment; filename=OIGMatches." + extension);

            //Step 7 : Return file content result
            return new FileContentResult(renderedBytes, mimeType);
        }

        [RequireAdmin]
        public JsonResult AvailableServices(int id)
        {
            JsonResult result = new JsonResult();
            result.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            Company.ServiceInfoServiceClient svcSvc = null;
            Company.StaffServiceClient svcStaff = null;
         
            www.soundpower.biz.common.GenericEntityListResponse options = new www.soundpower.biz.common.GenericEntityListResponse();
            options.EntityList = new www.soundpower.biz.common.GenericEntityList();
            try
            {
                var token = UserAuthorization.CurrentUser;

                    svcSvc = new Company.ServiceInfoServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                var rspSvc = svcSvc.List(true);
                
                svcStaff = new Company.StaffServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                var rspStaff = svcStaff.Details(id);
                
                if (rspStaff.Staff!=null && rspSvc.Services!=null)
                {
                    if (rspStaff.Staff.Services == null) rspStaff.Staff.Services = new www.soundpower.biz.common.GenericEntityList();
                    options.EntityList.AddRange(from s in rspSvc.Services
                                                where !rspStaff.Staff.Services.Exists(ss => ss.UniqueId == s.ServiceId.ToString())
                                                && rspStaff.Staff.Disciplines.Exists(d=>d.UniqueId == s.Discipline.UniqueId )
                                                orderby s.Name
                                                select new www.soundpower.biz.common.GenericEntity(s.ServiceId.ToString(), "Service", s.Name));
                }

            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.Staff.AvailableServices", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                options.ErrorMessages.Add(bex.Message);
                options.IsFailure = true;
            }
            finally
            {
                if (svcSvc != null) svcSvc.Dispose();
                if (svcStaff != null) svcStaff.Dispose();

            }

            result.Data = options;

            return result;
        }

        [HttpGet]
        public ActionResult AddService()
        {
            SoundPower.Web.Notifications.AddErrorNotification(ResourceText.SharedPages.CouldNotProcessRelogin);
            return RedirectToAction("index");
        }

        [RequireAdmin]
        [HttpPost]
        [AntiForgeryHandleError]

        public ActionResult AddService(int id, int serviceId)
        {
            Company.StaffServiceClient svcStaff = null;
            www.soundpower.biz.common.ResponseBase rspStaff = null;
            try
            {
                var token = UserAuthorization.CurrentUser;


                svcStaff = new Company.StaffServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                rspStaff = svcStaff.AddService (id,serviceId);

            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.Staff.AddService", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {
                if (svcStaff != null) svcStaff.Dispose();

            }

            SoundPower.Web.Notifications.AddResponseNotifications(rspStaff);
            if (!rspStaff.IsFailure) SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.StaffPages.ServiceAdded);
            return RedirectToAction("details", new { id = id, activeTab=2 });
        }

        [HttpGet]
        public ActionResult RemoveService()
        {
            SoundPower.Web.Notifications.AddErrorNotification(ResourceText.SharedPages.CouldNotProcessRelogin);
            return RedirectToAction("index");
        }

        [RequireAdmin]
        [HttpPost]
        [AntiForgeryHandleError]

        public ActionResult RemoveService(int id, int serviceId)
        {
            Company.StaffServiceClient svcStaff = null;
            www.soundpower.biz.common.ResponseBase rspStaff = null;
            try
            {
                var token = UserAuthorization.CurrentUser;


                svcStaff = new Company.StaffServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                rspStaff = svcStaff.RemoveService(id, serviceId);
               
            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.Staff.RemoveService", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {
                if (svcStaff != null) svcStaff.Dispose();

            }

            SoundPower.Web.Notifications.AddResponseNotifications(rspStaff);
            return RedirectToAction("details", new { id = id, activeTab = 2 });
        }
        [RequireAdmin]
        [AntiForgeryHandleError]
        [HttpPost]
        public ActionResult Update(StaffInfo model, int[] disciplineId)
        {
            model.Disciplines = SiteUtilities.ArrayToGenericEntityList(disciplineId, "Discipline");
            Company.StaffServiceClient svcStaff = null;
            if (model.Supervisor != null && model.Supervisor.UniqueId == "-1") model.Supervisor = null;

            if (ModelState.IsValid)
            {
                ResponseBase rsp = null;
                try
                {
                    var token = UserAuthorization.CurrentUser;


                    svcStaff = new Company.StaffServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                    rsp = svcStaff.Update(model);

                  
                }
                catch (Exception ex)
                {
                    var bex = new SoundPower.ErrorTracking.BaseException("Save Processing Failure", "TherapyCorner.Portal.Controllers.Staff.Update", ex);
                    SoundPower.Web.Utilities.ReportError(bex);
                    throw bex;
                }
                finally
                {
                    if (svcStaff != null) svcStaff.Dispose();

                }

                SoundPower.Web.Notifications.AddResponseNotifications(rsp);
                if (!rsp.IsFailure)
                {
                    SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.StaffPages.UpdateSuccess);
                    return RedirectToAction("details", new { id = model.StaffId });
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
                var bex = new SoundPower.ErrorTracking.BaseException("Reload Processing Failure", "TherapyCorner.Portal.Controllers.Staff.Update", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
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

            Company.StaffServiceClient svcStaff = null;
            StaffInfoResponse rspStaff = null;
           
            try
            {
                var token = UserAuthorization.CurrentUser;


                svcStaff = new Company.StaffServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                rspStaff = svcStaff.Details(id.Value );

                SetViewBagLists();
            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.Staff.Update", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {
                if (svcStaff != null) svcStaff.Dispose();

            }

            SoundPower.Web.Notifications.AddResponseNotifications(rspStaff);
            return View(rspStaff.Staff);
        }

        [RequireAdmin]
        [AntiForgeryHandleError]
        [HttpPost]
        public ActionResult Create(StaffInfo model, int[] disciplineId)
        {
            model.Disciplines = SiteUtilities.ArrayToGenericEntityList(disciplineId, "Discipline");
            Company.StaffServiceClient svcStaff = null;
            if (model.Supervisor != null && model.Supervisor.UniqueId == "-1") model.Supervisor = null;

            if (ModelState.IsValid)
            {
                ResponseBase rsp = null;
                try
                {
                    var token = UserAuthorization.CurrentUser;


                    svcStaff = new Company.StaffServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                    rsp = svcStaff.Create(model);


                }
                catch (Exception ex)
                {
                    var bex = new SoundPower.ErrorTracking.BaseException("Save Processing Failure", "TherapyCorner.Portal.Controllers.Staff.Create", ex);
                    SoundPower.Web.Utilities.ReportError(bex);
                    throw bex;
                }
                finally
                {
                    if (svcStaff != null) svcStaff.Dispose();

                }

                SoundPower.Web.Notifications.AddResponseNotifications(rsp);
                if (!rsp.IsFailure)
                {
                    SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.StaffPages.InviteSuccess);
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
                var bex = new SoundPower.ErrorTracking.BaseException("Reload Processing Failure", "TherapyCorner.Portal.Controllers.Staff.Create", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            return View(model);
        }

        [RequireAdmin]
        public ActionResult ResendInvite(int id)
        {
            Account.CompanyServiceClient svcStaff = null;


                ResponseBase rsp = null;
                try
                {
                    var token = UserAuthorization.CurrentUser;

                    svcStaff = new Account.CompanyServiceClient(SiteUtilities.AccountService, token, SiteUtilities.CurrentCulture);
                    rsp = svcStaff.ResendInvite(id);


                }
                catch (Exception ex)
                {
                    var bex = new SoundPower.ErrorTracking.BaseException("Save Processing Failure", "TherapyCorner.Portal.Controllers.Staff.ResendInvite", ex);
                    SoundPower.Web.Utilities.ReportError(bex);
                    throw bex;
                }
                finally
                {
                    if (svcStaff != null) svcStaff.Dispose();

                }

                SoundPower.Web.Notifications.AddResponseNotifications(rsp);
                if (!rsp.IsFailure)
                {
                    SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.StaffPages.ResendSuccess);
                }
          
            return RedirectToAction("details",new { id = id });
        }

        [RequireAdmin]
        [OutputCache(Duration =0, NoStore =true)]
        public JsonResult Credentials(int id)
        {
            Account.CredentialClientService svcStaff = null;
            JsonResult result = new JsonResult();
            result.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            www.therapycorner.com.account.CredentialListResponse rsp = null;
            try
            {
                var token = UserAuthorization.CurrentUser;

                svcStaff = new Account.CredentialClientService(SiteUtilities.AccountService, token, SiteUtilities.CurrentCulture);
                rsp = svcStaff.List(id);

            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Save Processing Failure", "TherapyCorner.Portal.Controllers.Staff.Credentials", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                rsp = new www.therapycorner.com.account.CredentialListResponse();
                rsp.IsFailure = true;
                rsp.ErrorMessages.Add(bex.Message);
            }
            finally
            {
                if (svcStaff != null) svcStaff.Dispose();

            }

            if (rsp.Credentials != null)
            {
                rsp.Credentials.Sort((b, a) => a.ValidTo.CompareTo(b.ValidTo));
                foreach (var c in rsp.Credentials)
                {
                    foreach (var v in c.Validations)
                    {
                        if (v.VerifiedAt.HasValue)
                        {
                            var utcdt = DateTime.SpecifyKind(v.VerifiedAt.Value, DateTimeKind.Utc);
                            v.VerifiedAt = null;
                            v.VerifiedAt = utcdt;
                        }
                    }
                }
            }
            result.Data = rsp;
             

            return result;
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
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.Staff.Create", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
       

            return View(new StaffInfo() { StaffId = -1, StartDate = DateTime.Today, Version = "NEW" } );
        }

        private void SetViewBagLists()
        {

            Company.StaffServiceClient svcProvider = null;
            try
            {
                var token = UserAuthorization.CurrentUser;
              


                //Get Supervisors
                svcProvider = new Company.StaffServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                var rspProvider = svcProvider.Search(new StaffSearchRequest() { Supervisor = true, Status = StaffSearchRequest.StatusEnum.NotInactiveOnly });
                SoundPower.Web.Notifications.AddResponseNotifications(rspProvider);
                if (rspProvider.IsFailure) throw new Exception("Failure received from supervisor list call.");
                if (rspProvider.Staff != null && rspProvider.Staff.Count > 0)
                {
                    rspProvider.Staff.Sort((a, b) => a.DisplayName.CompareTo(b.DisplayName));
                }
                ViewBag.Supervisors = rspProvider.Staff;

                //Get fields
                var rspFields = svcProvider.FieldList();
                SoundPower.Web.Notifications.AddResponseNotifications(rspFields);
                if (rspFields.IsFailure) throw new Exception("Failure received from field list call.");
                rspFields.Fields.RemoveAll(f => !f.IsActive.Value);
                ViewBag.Fields = rspFields.Fields;
            }
            catch (Exception ex)
            {
                throw new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.Staff.SetViewBagLists", ex);

            }
            finally
            {

                if (svcProvider != null) svcProvider.Dispose();
            }
        }

        [RequireStaffManagerFilter]
        public ActionResult Schedule(int id)
        {
            Company.StaffServiceClient svcStaff = null;
            StaffInfoResponse rspStaff = null;
            try
            {
                var token = UserAuthorization.CurrentUser;


                svcStaff = new Company.StaffServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                rspStaff = svcStaff.Details(id);
              
            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.Staff.Schedule", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {
                if (svcStaff != null) svcStaff.Dispose();

            }

            SoundPower.Web.Notifications.AddResponseNotifications(rspStaff);
            return View(rspStaff.Staff);
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
            Company.StaffServiceClient svcStaff = null;
            StaffInfoResponse rspStaff = null;
            try
            {
                var token = UserAuthorization.CurrentUser;


                svcStaff = new Company.StaffServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                rspStaff = svcStaff.Details(id.Value );
                ViewBag.StaffId = id;
                ViewBag.StaffName = rspStaff.Staff.DisplayName;
                 GetClientList(id.Value , token);
            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.Staff.MakeAppt", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {
                if (svcStaff != null) svcStaff.Dispose();

            }

            return View(new CreateAppointmentRequest() { ClientServiceId = 0, InitialDate = DateTime.Today.AddDays(1), MeetingCount = 1, StartTime = new TimeSpan(8, 0, 0) }  );
        }

        [HttpPost]
        [AntiForgeryHandleError]
        [RequireWorkerFilter]
        public ActionResult MakeAppt(CreateAppointmentRequest request, int staffId, string staffName)
        {
            ViewBag.StaffId = staffId;
            ViewBag.StaffName = staffName;
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
                    var bex = new SoundPower.ErrorTracking.BaseException("Save Processing Failure", "TherapyCorner.Portal.Controllers.Staff.MakeAppt", ex);
                    SoundPower.Web.Utilities.ReportError(bex);
                    throw bex;
                }
                finally
                {
                    if (svcStaff != null) svcStaff.Dispose();

                }
                SoundPower.Web.Notifications.AddResponseNotifications(rspStaff);
                if(!rspStaff.IsFailure )
                {

                    SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.StaffPages.AppointmentsScheduled);
                    return RedirectToAction("schedule", new { id = staffId });
                }
                else
                {
                    SiteUtilities.ApplyFieldIssues(rspStaff, ModelState);
                }
            }
            try
            {
                GetClientList(staffId, UserAuthorization.CurrentUser);
            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Validation Return Processing Failure", "TherapyCorner.Portal.Controllers.Staff.MakeAppt", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            return View(request);
        }

        private void GetClientList(int id, www.therapycorner.com.account.Session token)
        {
            Company.ClientServiceClient svcClient = new Company.ClientServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
            try
            { 
            var rspClients = svcClient.StaffClients(id);
            var lst = new www.soundpower.biz.common.GenericEntityList();
                if (!rspClients.IsFailure && rspClients.Clients != null)
                {
                    lst.AddRange(from c in rspClients.Clients
                                 orderby c.LastName, c.FirstName
                                 let p = c.ToPerson()
                                 select new GenericEntity(c.ClientId.ToString(), "Client", p.LastFirstMI));

                }
                ViewBag.Clients = lst;
                }
                catch (Exception ex)
                {
                    throw new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.Staff.GetClientList", ex);
                    
                }
                finally
                {
                svcClient.Dispose();

                }
            }
    }
}