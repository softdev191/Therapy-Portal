using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using www.soundpower.biz.common;
using www.therapycorner.com.account;
using www.therapycorner.com.account.MessageContracts;
using www.therapycorner.com.company;
using www.therapycorner.com.company.MessageContracts;

namespace TherapyCorner.Portal.Controllers
{
    [RequireUser]
    [RequireHttps]
    [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]

    public class CredentialController : Controller
    {
        [HttpGet]
        public ActionResult Create(string id)
        {
            return View(new CredentialInfo() { CredentialId = -1, Type = new GenericEntity() { Context = "CredentialType", UniqueId = id }, User = UserAuthorization.CurrentUser.User, ValidFrom = DateTime.Today, ValidTo = DateTime.Today });
        }


        [HttpPost]
        [AntiForgeryHandleError]
        public ActionResult Create(HttpPostedFileBase file, CredentialInfo model)
        {
            bool fileGood = true;
            if (file != null && file.ContentLength > 20000000)
            {
                SoundPower.Web.Notifications.AddErrorNotification(ResourceText.CredentialPages.FileTooLarge);
                fileGood = false;
            }
            if (ModelState.IsValid && fileGood)
            {


                Account.CredentialClientService svc = null;
                ResponseBase rsp = null;
                CredentialRequest req = new CredentialRequest() { Credential = model };
                if (file != null && file.ContentLength > 0)
                {

                    req.FileData = new byte[file.InputStream.Length];
                    file.InputStream.Read(req.FileData, 0, req.FileData.Length);
                    req.Credential.ImageType = file.ContentType;
                    var parts = file.FileName.Split('.');
                    req.Credential.ImageExt = parts[parts.Length - 1];
                }
                try
                {
                    var token = UserAuthorization.CurrentUser;
                    svc = new Account.CredentialClientService(SiteUtilities.AccountService, token, SiteUtilities.CurrentCulture);
                    rsp = svc.Create(req);

                }
                catch (Exception ex)
                {
                    var bex = new SoundPower.ErrorTracking.BaseException("Save Failure", "TherapyCorner.Controllers.Credential.Create", ex);
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

                    SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.CredentialPages.SuccessCreatedCredential);
                    return RedirectToAction("index", "profile");
                }
                else
                {
                    SiteUtilities.ApplyFieldIssues(rsp, ModelState);

                }

            }
            return View(model);
        }

        [HttpGet]
        public ActionResult Image(int id)
        {


            Account.CredentialClientService svc = null;
            FileResponse rsp = null;

            try
            {
                var token = UserAuthorization.CurrentUser;
                svc = new Account.CredentialClientService(SiteUtilities.AccountService, token, SiteUtilities.CurrentCulture);
                rsp = svc.Image(id);

            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.Credential.Image", ex);
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
            return RedirectToAction("index", "home");


        }

        [HttpGet]
        public ActionResult Edit(int? id)
        {
            Company.StaffServiceClient svcStaff = null;
            if (!id.HasValue)
            {

                SoundPower.Web.Notifications.AddErrorNotification(ResourceText.SharedPages.CouldNotProcessRelogin);
                return RedirectToAction("index", "profile");

            }

            Account.CredentialClientService svc = null;
            CredentialResponse rsp = null;

            var isAdmin = false;
            try
            {
                var token = UserAuthorization.CurrentUser;
                isAdmin = token.IsAdmin;
                svc = new Account.CredentialClientService(SiteUtilities.AccountService, token, SiteUtilities.CurrentCulture);
                rsp = svc.Details(id.Value);

                svcStaff = new Company.StaffServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);

                var rspStaff = svcStaff.UserDetails(Convert.ToInt32(rsp.Credential.User?.UniqueId), token.CurrentCompany);
                ViewBag.StaffID = rspStaff?.Staff?.StaffId;

            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.Credential.Edit", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {
                if (svc != null) svc.Dispose();
                if (svcStaff != null) svcStaff.Dispose();
            }


            SoundPower.Web.Notifications.AddResponseNotifications(rsp);
            if (rsp.IsFailure)
            {
                return RedirectToAction("index", "profile");

            }
            if (rsp.Credential == null)
            {
                SoundPower.Web.Notifications.AddErrorNotification(ResourceText.CredentialPages.UnknownCredential);
                return RedirectToAction("index", "profile");

            }
            if (rsp.Credential.User.UniqueId != UserAuthorization.CurrentUser.User.UniqueId && !isAdmin)
            {
                SoundPower.Web.Notifications.AddErrorNotification(ResourceText.Messages.AccessDenied);
                return RedirectToAction("index", "profile");

            }

            //var   svcStaff = new Company.StaffServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
            //  svcStaff.

            //  var staff = staff.Staff.Find(s => s.User.UniqueId == rspCred.Credential.User.UniqueId);


            return View(rsp.Credential);

        }

        [HttpPost]
        [AntiForgeryHandleError]
        public ActionResult Edit(CredentialInfo model)
        {

            if (ModelState.IsValid)
            {

                var isAdmin = false;
                Account.CredentialClientService svc = null;
                Company.StaffServiceClient svcStaff = null;
                int? staffID = null;
                ResponseBase rsp = null;
                CredentialRequest req = new CredentialRequest() { Credential = model };

                try
                {
                    var token = UserAuthorization.CurrentUser;
                    isAdmin = token.IsAdmin;
                    svc = new Account.CredentialClientService(SiteUtilities.AccountService, token, SiteUtilities.CurrentCulture);
                    rsp = svc.Update(req);

                    svcStaff = new Company.StaffServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                    var rspStaff = svcStaff.UserDetails(Convert.ToInt32(model.User.UniqueId), token.CurrentCompany);
                    staffID = rspStaff?.Staff?.StaffId;

                }
                catch (Exception ex)
                {
                    var bex = new SoundPower.ErrorTracking.BaseException("Save Failure", "TherapyCorner.Portal.Controllers.Credential.Edit", ex);
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

                    SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.CredentialPages.SuccessUpdatedCredential);
                    if (isAdmin && staffID.HasValue)
                    {
                        return RedirectToAction("details", "staff", new { id = staffID, activeTab = 1 });
                    }
                    else
                    {
                        return RedirectToAction("index", "profile");
                    }
                }
                else
                {
                    SiteUtilities.ApplyFieldIssues(rsp, ModelState);

                }

            }
            return View(model);
        }

        [HttpPost]
        [AntiForgeryHandleError]
        public ActionResult SetImage(HttpPostedFileBase file, CredentialInfo model)
        {
            bool fileGood = true;
            if (file != null && file.ContentLength > 20000000)
            {
                SoundPower.Web.Notifications.AddErrorNotification(ResourceText.CredentialPages.FileTooLarge);
                fileGood = false;
            }
            if (ModelState.IsValid && fileGood)
            {


                Account.CredentialClientService svc = null;
                Company.StaffServiceClient svcStaff = null;
                int? staffID = null;
                var isAdmin = false;
                ResponseBase rsp = null;
                CredentialRequest req = new CredentialRequest() { Credential = model };
                if (file != null && file.ContentLength > 0)
                {

                    req.FileData = new byte[file.InputStream.Length];
                    file.InputStream.Read(req.FileData, 0, req.FileData.Length);
                    req.Credential.ImageType = file.ContentType;
                    var parts = file.FileName.Split('.');
                    req.Credential.ImageExt = parts[parts.Length - 1];
                }
                try
                {
                    var token = UserAuthorization.CurrentUser;
                    isAdmin = token.IsAdmin;
                    svc = new Account.CredentialClientService(SiteUtilities.AccountService, token, SiteUtilities.CurrentCulture);
                    rsp = svc.Update(req);

                    svcStaff = new Company.StaffServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                    var rspStaff = svcStaff.UserDetails(Convert.ToInt32(model?.User?.UniqueId), token.CurrentCompany);
                    staffID = rspStaff?.Staff?.StaffId;

                }
                catch (Exception ex)
                {
                    var bex = new SoundPower.ErrorTracking.BaseException("Save Failure", "TherapyCorner.Portal.Controllers.Credential.SetImage", ex);
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

                    SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.CredentialPages.SuccessImageChanged);
                    if (isAdmin && staffID.HasValue)
                    {
                        return RedirectToAction("details", "staff", new { id = staffID, activeTab = 1 });
                    }
                    else
                    {
                        return RedirectToAction("index", "profile");
                    }
                }
                else
                {
                    SiteUtilities.ApplyFieldIssues(rsp, ModelState);

                }

            }
            return View(model);
        }

        [HttpGet]
        public ActionResult SetImage()
        {
            SoundPower.Web.Notifications.AddErrorNotification(ResourceText.SharedPages.CouldNotProcessRelogin);
            return RedirectToAction("index", "profile");
        }

        [HttpGet]
        public ActionResult RemoveImage()
        {
            SoundPower.Web.Notifications.AddErrorNotification(ResourceText.SharedPages.CouldNotProcessRelogin);
            return RedirectToAction("index", "profile");
        }

        [HttpGet]
        public ActionResult Delete()
        {
            SoundPower.Web.Notifications.AddErrorNotification(ResourceText.SharedPages.CouldNotProcessRelogin);
            return RedirectToAction("index", "profile");
        }

        [HttpPost]
        [AntiForgeryHandleError]
        public ActionResult RemoveImage(CredentialInfo model)
        {

            if (ModelState.IsValid)
            {


                Account.CredentialClientService svc = null;
                Company.StaffServiceClient svcStaff = null;
                int? staffID = null;
                ResponseBase rsp = null;
                CredentialRequest req = new CredentialRequest() { Credential = model };
                var isAdmin = false;
                try
                {
                    var token = UserAuthorization.CurrentUser;
                    isAdmin = token.IsAdmin;
                    svc = new Account.CredentialClientService(SiteUtilities.AccountService, token, SiteUtilities.CurrentCulture);
                    rsp = svc.Update(req);

                    svcStaff = new Company.StaffServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                    var rspStaff = svcStaff.UserDetails(Convert.ToInt32(model.User?.UniqueId), token.CurrentCompany);
                    staffID = rspStaff?.Staff?.StaffId;

                }
                catch (Exception ex)
                {
                    var bex = new SoundPower.ErrorTracking.BaseException("Save Failure", "TherapyCorner.Portal.Controllers.Credential.RemoveImage", ex);
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

                    SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.CredentialPages.SuccessImageRemoved);
                    if (isAdmin && staffID.HasValue)
                    {
                        return RedirectToAction("details", "staff", new { id = staffID, activeTab = 1 });
                    }
                    else
                    {
                        return RedirectToAction("index", "profile");
                    }
                }
                else
                {
                    SiteUtilities.ApplyFieldIssues(rsp, ModelState);

                }

            }
            return View(model);
        }


        [HttpPost]
        [AntiForgeryHandleError]
        public ActionResult Delete(int id)
        {

            if (ModelState.IsValid)
            {


                Account.CredentialClientService svc = null;
                Company.StaffServiceClient svcStaff = null;
                ResponseBase rsp = null;
                CredentialResponse credentialResponse = null;
                var isAdmin = false;
                int? staffID = null;
                try
                {
                    var token = UserAuthorization.CurrentUser;
                    isAdmin = token.IsAdmin;
                    svc = new Account.CredentialClientService(SiteUtilities.AccountService, token, SiteUtilities.CurrentCulture);
                    credentialResponse = svc.Details(id);
                    rsp = svc.Delete(id);

                    svcStaff = new Company.StaffServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                    var rspStaff = svcStaff.UserDetails(Convert.ToInt32(credentialResponse?.Credential?.User?.UniqueId), token.CurrentCompany);
                    staffID = rspStaff?.Staff?.StaffId;

                }
                catch (Exception ex)
                {
                    var bex = new SoundPower.ErrorTracking.BaseException("Save Failure", "TherapyCorner.Portal.Controllers.Credential.Delete", ex);
                    SoundPower.Web.Utilities.ReportError(bex);
                    throw bex;
                }
                finally
                {
                    if (svc != null) svc.Dispose();
                    if (svcStaff != null) svcStaff.Dispose();
                }


                SoundPower.Web.Notifications.AddResponseNotifications(rsp);
                if (!rsp.IsFailure)
                {

                    SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.CredentialPages.DeleteSuccess);
                    if (isAdmin && staffID.HasValue)
                    {
                        return RedirectToAction("details", "staff", new { id = staffID, activeTab = 1 });
                    }
                    else
                    {
                        return RedirectToAction("index", "profile");
                    }
                }


            }
            return RedirectToAction("details", new { id = id });
        }

        [HttpGet]
        [RequireAdmin]
        public ActionResult Pending()
        {

            return View();
        }

        [RequireAdmin]
        public JsonResult Ignore(int id)
        {
            JsonResult result = new JsonResult();
            result.JsonRequestBehavior = JsonRequestBehavior.AllowGet;

            Account.CredentialClientService svc = null;
            ResponseBase rsp = null;

            try
            {
                var token = UserAuthorization.CurrentUser;
                svc = new Account.CredentialClientService(SiteUtilities.AccountService, token, SiteUtilities.CurrentCulture);
                rsp = svc.Ignore(id);

            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.Credential.Ignore", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                rsp = new ResponseBase() { IsFailure = true };
                rsp.ErrorMessages.Add(string.Format("{0} ({1})", bex.Message, bex.UniqueId));
            }
            finally
            {
                if (svc != null) svc.Dispose();
            }


            result.Data = rsp;

            return result;

        }

        [RequireAdmin]
        public JsonResult Watch(int id)
        {
            JsonResult result = new JsonResult();
            result.JsonRequestBehavior = JsonRequestBehavior.AllowGet;


            Account.CredentialClientService svc = null;
            ResponseBase rsp = null;

            try
            {
                var token = UserAuthorization.CurrentUser;
                svc = new Account.CredentialClientService(SiteUtilities.AccountService, token, SiteUtilities.CurrentCulture);
                rsp = svc.Watch(id);

            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.Credential.Watch", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                rsp = new ResponseBase() { IsFailure = true };
                rsp.ErrorMessages.Add(string.Format("{0} ({1})", bex.Message, bex.UniqueId));
            }
            finally
            {
                if (svc != null) svc.Dispose();
            }


            result.Data = rsp;

            return result;

        }

        [RequireAdmin]
        public JsonResult Verify(int id)
        {
            JsonResult result = new JsonResult();
            result.JsonRequestBehavior = JsonRequestBehavior.AllowGet;


            Account.CredentialClientService svc = null;
            ResponseBase rsp = null;

            try
            {
                var token = UserAuthorization.CurrentUser;
                svc = new Account.CredentialClientService(SiteUtilities.AccountService, token, SiteUtilities.CurrentCulture);
                rsp = svc.Verify(id);

            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.Credential.Verfiy", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                rsp = new ResponseBase() { IsFailure = true };
                rsp.ErrorMessages.Add(string.Format("{0} ({1})", bex.Message, bex.UniqueId));
            }
            finally
            {
                if (svc != null) svc.Dispose();
            }


            result.Data = rsp;

            return result;

        }

        [RequireAdmin]
        public JsonResult QuickMessage(int id, string message)
        {
            JsonResult result = new JsonResult();
            result.JsonRequestBehavior = JsonRequestBehavior.AllowGet;

            Account.CredentialClientService svcCred = null;
            Company.MessageClientService svcMsg = null;
            Company.StaffServiceClient svcStaff = null;
            ResponseBase rsp = null;
            try
            {
                var token = UserAuthorization.CurrentUser;
                svcCred = new Account.CredentialClientService(SiteUtilities.AccountService, token, SiteUtilities.CurrentCulture);
                var rspCred = svcCred.Details(id);
                if (!rspCred.IsFailure)
                {
                    svcStaff = new Company.StaffServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                    var staff = svcStaff.Search(new www.therapycorner.com.company.MessageContracts.StaffSearchRequest() { Status = www.therapycorner.com.company.MessageContracts.StaffSearchRequest.StatusEnum.ActiveOnly });
                    if (!staff.IsFailure)
                    {
                        var usr = staff.Staff.Find(s => s.User.UniqueId == rspCred.Credential.User.UniqueId);
                        svcMsg = new Company.MessageClientService(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                        var req = new www.therapycorner.com.company.MessageInfo()
                        {
                            Contents = message,
                            MessageId = -1,
                            Recipients = new www.therapycorner.com.company.MessageRecipientList(),
                            SentAt = DateTime.Now,
                            Subject = string.Format("{0} Valid From {1} to {2} {3}", rspCred.Credential.Type.Name, rspCred.Credential.ValidFrom.ToShortDateString(), rspCred.Credential.ValidTo.ToShortDateString(), string.IsNullOrWhiteSpace(rspCred.Credential.DocumentId) ? "" : string.Format("({0})", rspCred.Credential.DocumentId))
                        };
                        req.Recipients.Add(new www.therapycorner.com.company.MessageRecipient()
                        {
                            UniqueId = usr.StaffId,
                            HasRead = false
                        });
                        rsp = svcMsg.Create(req);
                    }
                    else
                    {
                        rsp = staff;
                    }
                }
                else
                {
                    rsp = rspCred;
                }

            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.Credential.QuickMessage", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                rsp = new ResponseBase() { IsFailure = true };
                rsp.ErrorMessages.Add(string.Format("{0} ({1})", bex.Message, bex.UniqueId));
            }
            finally
            {
                if (svcCred != null) svcCred.Dispose();
                if (svcMsg != null) svcMsg.Dispose();
                if (svcStaff != null) svcStaff.Dispose();
            }

            result.Data = rsp;
            return result;

        }

        [RequireAdmin]
        public JsonResult UserCredentials(int id)
        {
            JsonResult result = new JsonResult();
            result.JsonRequestBehavior = JsonRequestBehavior.AllowGet;


            Account.CredentialClientService svc = null;
            CredentialListResponse rsp = null;

            try
            {
                var token = UserAuthorization.CurrentUser;
                svc = new Account.CredentialClientService(SiteUtilities.AccountService, token, SiteUtilities.CurrentCulture);
                rsp = svc.List(id);

            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.Credential.UserCredentials", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                rsp = new CredentialListResponse() { IsFailure = true, Credentials = new CredentialInfoList() };
                rsp.ErrorMessages.Add(string.Format("{0} ({1})", bex.Message, bex.UniqueId));
            }
            finally
            {
                if (svc != null) svc.Dispose();
            }


            result.Data = rsp;

            return result;

        }

        [RequireAdmin]
        public JsonResult PendingCredentials()
        {
            JsonResult result = new JsonResult();
            result.JsonRequestBehavior = JsonRequestBehavior.AllowGet;

            Account.CredentialClientService svc = null;
            CredentialListResponse rsp = null;

            try
            {
                var token = UserAuthorization.CurrentUser;
                svc = new Account.CredentialClientService(SiteUtilities.AccountService, token, SiteUtilities.CurrentCulture);
                rsp = svc.Pending(token.CurrentCompany);

            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.Credential.Pending", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                rsp = new CredentialListResponse() { IsFailure = true, Credentials = new CredentialInfoList() };
                rsp.ErrorMessages.Add(string.Format("{0} ({1})", bex.Message, bex.UniqueId));
            }
            finally
            {
                if (svc != null) svc.Dispose();
            }

            if (rsp.Credentials != null)
            {
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

        [HttpGet]
        public ActionResult Requirements()
        {


            Company.CredentialRequirementServiceClient svc = null;
            CredentialRequirementListResponse rsp = null;

            try
            {
                var token = UserAuthorization.CurrentUser;
                svc = new Company.CredentialRequirementServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                rsp = svc.List();

                var types = StaticData.CredentialTypes;
                foreach (var t in types)
                {
                    var req = rsp.Requirements.Find(r => r.Type.UniqueId == t.UniqueId);
                    if (req == null)
                    {
                        rsp.Requirements.Add(new www.therapycorner.com.company.CredentialRequirement()
                        {
                            Type = t

                        });
                    }
                    else
                    {
                        req.Type.Name = t.Name;
                    }
                }
            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.Credential.Requirements", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {
                if (svc != null) svc.Dispose();
            }


            SoundPower.Web.Notifications.AddResponseNotifications(rsp);
            if (rsp.IsFailure)
            {
                return RedirectToAction("index", "home");

            }
            rsp.Requirements.Sort((a, b) => a.Type.Name.CompareTo(b.Type.Name));
            return View(rsp.Requirements);

        }

        [HttpPost]
        [AntiForgeryHandleError]
        public ActionResult Requirements(CredentialRequirementList model)
        {

            if (ModelState.IsValid)
            {

                Company.CredentialRequirementServiceClient svc = null;
                ResponseBase rsp = null;

                try
                {
                    var token = UserAuthorization.CurrentUser;
                    svc = new Company.CredentialRequirementServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                    rsp = svc.Update(model);
                }
                catch (Exception ex)
                {
                    var bex = new SoundPower.ErrorTracking.BaseException("Save Processing Failure", "TherapyCorner.Portal.Controllers.Credential.Requirements", ex);
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
                    SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.CredentialPages.RequirementsSaved);

                }

            }
            return View(model);

        }

        public ActionResult Missing()
        {
            var token = UserAuthorization.CurrentUser;
            Account.CredentialClientService svc = null;
            Company.CredentialRequirementServiceClient svcReqs = null;
            var result = new List<Models.MissingCredentialCounts>();

            Company.StaffServiceClient staffDetails = null;
            staffDetails = new Company.StaffServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
            var staffInfo = staffDetails.UserDetails(Convert.ToInt32(token.UserId), token.CurrentCompany);
            var disciplines = staffInfo.Staff.Disciplines.ToList();

            try
            {
                svc = new Account.CredentialClientService(SiteUtilities.AccountService, token, SiteUtilities.CurrentCulture);

                var types = StaticData.CredentialTypes;

                var rspCreds = svc.List(token.UserId);
                SoundPower.Web.Notifications.AddResponseNotifications(rspCreds);
                svcReqs = new Company.CredentialRequirementServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                var rspReqs = svcReqs.List();
                SoundPower.Web.Notifications.AddResponseNotifications(rspReqs);
                foreach (var r in rspReqs.Requirements)
                {
                    var tpe = types.Find(t => t.UniqueId == r.Type.UniqueId);
                    var uploaded = rspCreds.Credentials.FindAll(c => c.Type.UniqueId == r.Type.UniqueId && c.ValidTo >= DateTime.Today);
                    int ready = uploaded.Count(c => c.Validations.Exists(v => v.Company.AddressId == token.CurrentCompany && v.VerifiedAt.HasValue));
                    var entry = new Models.MissingCredentialCounts()
                    {
                        TypeId = tpe.UniqueId,
                        Name = tpe.Name,
                        Blocking = ResourceText.Dictionary.No,
                        Uploaded = uploaded.Count,
                        Verified = ready
                    };

                    if (token.IsAdmin && r.AdminRequired > ready)
                    {
                        if (r.AdminBlocking) entry.Blocking = ResourceText.Dictionary.Yes;
                        entry.Required = r.AdminRequired;
                    }
                    if (token.IsProvider && r.ProviderRequired > ready)
                    {
                        if (r.ProviderBlocking) entry.Blocking = ResourceText.Dictionary.Yes;
                        if (r.ProviderRequired > entry.Required) entry.Required = r.ProviderRequired;
                    }
                    if (token.IsWorker && r.WorkerRequired > ready)
                    {
                        if (r.WorkerBlocking) entry.Blocking = ResourceText.Dictionary.Yes;
                        if (r.WorkerRequired > entry.Required) entry.Required = r.WorkerRequired;
                    }

                    if (token.IsSupervisor && r.SupervisorRequired > ready)
                    {
                        if (r.SupervisorBlocking) entry.Blocking = ResourceText.Dictionary.Yes;
                        if (r.SupervisorRequired > entry.Required) entry.Required = r.SupervisorRequired;
                    }

                    if (entry.Required > entry.Verified) result.Add(entry);

                }
            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Controllers.Credential.Missing", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {
                if (svc != null) svc.Dispose();
                if (svcReqs != null) svcReqs.Dispose();

            }

            var toSkip = new string[] { "Speech Therapy", "Physical Therapy", "Occupational Therapy" };
            var toRemove = false;
            if (token.IsSupervisor && !disciplines.Any(x => toSkip.Contains(x.Name))) toRemove = true;

            if (token.IsProvider && !disciplines.Any(x => x.Name == "Speech Therapy")) toRemove = true;

            if (toRemove)
            {
                result.RemoveAll(n => n.Name == "National Provider ID (NPI)" || n.Name == "State Medicaid ID");
            }

            return View(result);
        }
    }
}