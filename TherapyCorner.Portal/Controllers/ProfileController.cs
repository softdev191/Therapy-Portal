using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using reCAPTCHA.MVC;
using SoundPower.ErrorTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using TherapyCorner.Portal.Models;
using TherapyCorner.Portal.ResourceText;
using www.soundpower.biz.common;
using www.therapycorner.com.account.MessageContracts;

namespace TherapyCorner.Portal.Controllers
{
    [RequireHttps]
    public class ProfileController : Controller
    {
        private const string XsrfKey = "XsrfId";
        public CustomUserManager PCustomUserManager { get; private set; }

        public ProfileController()
        {
            PCustomUserManager = new CustomUserManager();

        }

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;

            }
        }
        [HttpGet]
        [AllowAnonymous]
        public ActionResult Register(string code)
        {
            return View(new RegistrationRequest()
            {
                User = new www.therapycorner.com.account.UserInfo() { Address = new www.therapycorner.com.account.AddressInfo(), UserId = -1, Version = "NEW", DoB=DateTime.Today.AddYears(-20) },
                InviteCode = code
            }
                );
        }

        [HttpPost]
        [AntiForgeryHandleError]
        [AllowAnonymous]
        public ActionResult Register(RegistrationRequest model)
        {
            if (ModelState.IsValid)
            {
                Account.UserServiceClient svc = null;
                ResponseBase rsp = null;
                var deviceInfo = GetDeviceInfo();
                model.DeviceCode = deviceInfo.DeviceCode;
                model.DeviceIP = deviceInfo.DeviceIP;
                try
                {
                    var token = UserAuthorization.CurrentUser;
                    svc = new Account.UserServiceClient(SiteUtilities.AccountService, token, SiteUtilities.CurrentCulture);
                    rsp = svc.Register(model);

                }
               
                catch (Exception ex)
                {
                    var ctx = new Dictionary<string, string>();
                    ctx.Add("Model", Utilities.SerializeDataContractToXML(model));
                    var bex = new SoundPower.ErrorTracking.BaseException("Save Processing Failure", "TherapyCorner.Controllers.Profile.Register", ex,ctx);
                    SoundPower.Web.Utilities.ReportError(bex);
                    throw bex;
                }
                finally
                {
                    if (svc != null) svc.Dispose();
                }
                HttpCookie cookie = new HttpCookie("tcdid", deviceInfo.HashedCode );
                cookie.Expires = DateTime.Now.AddDays(30);
                Response.Cookies.Add(cookie);

                SoundPower.Web.Notifications.AddResponseNotifications(rsp);
                if (!rsp.IsFailure)
                {
                    return View("RegisterSuccess");
                }
                SiteUtilities.ApplyFieldIssues(rsp, ModelState);
            }

            return View(model);
        }
        // GET: Profile
        [RequireUser]
        public ActionResult Index(int? tab)
        {
            Account.UserServiceClient svc = null;
            Account.CredentialClientService svcCred = null;
            UserInfoResponse rsp = null;
            try
            {
                var token = UserAuthorization.CurrentUser;
                svc = new Account.UserServiceClient(SiteUtilities.AccountService, token, SiteUtilities.CurrentCulture);
                svcCred = new Account.CredentialClientService(SiteUtilities.AccountService, token, SiteUtilities.CurrentCulture);
                rsp = svc.Details(token.UserId);
                var rspCred = svcCred.List(token.UserId);
                SoundPower.Web.Notifications.AddResponseNotifications(rspCred);
                if (!rspCred.IsFailure) ViewBag.Credentials = rspCred.Credentials;
            }
            catch (Exception ex)
            {
                var bex= new SoundPower.ErrorTracking.BaseException("Authentication Failure", "TherapyCorner.Controllers.Profile.Index", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {
                if (svc != null) svc.Dispose();
            }

            if (rsp.IsFailure)
            {
                throw new BaseException("Failure Received", "TherapyCorner.Controllers.Profile.Index");
            }
              SoundPower.Web.Notifications.AddResponseNotifications(rsp);
            ViewBag.Tab = tab;
          return View(rsp.User);
        }

        [HttpGet]
        [RequireUser]
        public ActionResult Availability(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {

                SoundPower.Web.Notifications.AddErrorNotification(ResourceText.SharedPages.CouldNotProcessRelogin);
                return RedirectToAction("index");

            }

            Account.UserServiceClient svc = null;
            UserInfoResponse rsp = null;
            www.therapycorner.com.account.AvailabilityList availabilities = new www.therapycorner.com.account.AvailabilityList();
            try
            {
                var token = UserAuthorization.CurrentUser;
                svc = new Account.UserServiceClient(SiteUtilities.AccountService, token, SiteUtilities.CurrentCulture);
                rsp = svc.Details(token.UserId);
                availabilities.AddRange(rsp.User.Availabilities.FindAll(a => a.CompanyId == id));
                availabilities.Sort((a, b) => a.Day == b.Day ? a.StartTicks.CompareTo(b.StartTicks) : a.Day.CompareTo(b.Day));
                ViewBag.CompanyId = id;
                ViewBag.CompanyName = rsp.User.Companies.Find(c => c.AddressId == id).CareOf;
                ViewBag.AvailList = availabilities;
            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Loading Failure", "TherapyCorner.Controllers.Profile.Availability", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {
                if (svc != null) svc.Dispose();
            }

            if (rsp.IsFailure)
            {
                throw new BaseException("Load Failure Received", "TherapyCorner.Controllers.Profile.Availability");
            }
            SoundPower.Web.Notifications.AddResponseNotifications(rsp);
            return View(new www.therapycorner.com.account.Availability() { CompanyId = id, Day = 1 });
        }

        [HttpPost]
        [RequireUser]
        public ActionResult Availability(www.therapycorner.com.account.Availability request,DateTime startTimeText, DateTime endTimeText)
        {
            Account.UserServiceClient svc = null;
            request.StartTime = new TimeSpan(startTimeText.Hour, startTimeText.Minute, 0);
            request.EndTime = new TimeSpan(endTimeText.Hour, endTimeText.Minute,0);
            Account.ScheduleServiceClient svcSchedule = null;

            UserInfoResponse rsp = null;
            www.therapycorner.com.account.AvailabilityList availabilities = new www.therapycorner.com.account.AvailabilityList();
                 var token = UserAuthorization.CurrentUser;

            if (ModelState.IsValid)
            {
                try
                {
                     svcSchedule = new Account.ScheduleServiceClient(SiteUtilities.AccountService, token, SiteUtilities.CurrentCulture);
                   var rspSave = svcSchedule.AddAvailability(request );
                    SoundPower.Web.Notifications.AddResponseNotifications(rspSave);
                    if(rspSave.IsFailure)
                    {
                        SiteUtilities.ApplyFieldIssues(rspSave, ModelState);
                    }
                    else
                    {
                        SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.ProfilePages.AvailabilitySaved);
                    }

                }
                catch (Exception ex)
                {
                    var context = new Dictionary<string, string>();
                    context.Add("Request", www.soundpower.biz.common.Utilities.SerializeDataContractToXML(request));
                    var bex = new SoundPower.ErrorTracking.BaseException("Save Failure", "TherapyCorner.Controllers.Profile.Availability", ex,context );
                    SoundPower.Web.Utilities.ReportError(bex);
                    throw bex;
                }
                finally
                {
                    if (svcSchedule != null) svcSchedule.Dispose();
                }
            }

           try
            {
                svc = new Account.UserServiceClient(SiteUtilities.AccountService, token, SiteUtilities.CurrentCulture);
                rsp = svc.Details(token.UserId);
                availabilities.AddRange(rsp.User.Availabilities.FindAll(a => a.CompanyId == request.CompanyId ));
                availabilities.Sort((a, b) => a.Day == b.Day ? a.StartTicks.CompareTo(b.StartTicks) : a.Day.CompareTo(b.Day));
                ViewBag.CompanyId = request.CompanyId;
                ViewBag.CompanyName = rsp.User.Companies.Find(c => c.AddressId == request.CompanyId).CareOf;
                ViewBag.AvailList = availabilities;

            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Controllers.Profile.Availability", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {
                if (svc != null) svc.Dispose();
            }

            if (rsp.IsFailure)
            {
                throw new BaseException("Failure Received", "TherapyCorner.Controllers.Profile.Availability");
            }
            SoundPower.Web.Notifications.AddResponseNotifications(rsp);
            return View(request);
        }


        [RequireUser]
        public ActionResult RemoveAvailability(long? id,string company)
        {
            if (!id.HasValue)
            {

                SoundPower.Web.Notifications.AddErrorNotification(ResourceText.SharedPages.CouldNotProcessRelogin);
                return RedirectToAction("index");

            }

            Account.ScheduleServiceClient svcSchedule = null;

            www.therapycorner.com.account.AvailabilityList availabilities = new www.therapycorner.com.account.AvailabilityList();
            var token = UserAuthorization.CurrentUser;

       
                try
                {
                    svcSchedule = new Account.ScheduleServiceClient(SiteUtilities.AccountService, token, SiteUtilities.CurrentCulture);
                    var rspSave = svcSchedule.RemoveAvailability(id.Value );
                    SoundPower.Web.Notifications.AddResponseNotifications(rspSave);
                    if (!rspSave.IsFailure)

                    {
                        SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.ProfilePages.AvailabilityRemoved);
                    }

                }
                catch (Exception ex)
                {
                    var context = new Dictionary<string, string>();
                    context.Add("Id",id.ToString());
                    var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Controllers.Profile.RemoveAvailability", ex, context);
                    SoundPower.Web.Utilities.ReportError(bex);
                    throw bex;
                }
                finally
                {
                    if (svcSchedule != null) svcSchedule.Dispose();
                }


            return RedirectToAction("Availability", new { id = company });
        }

        [HttpGet]
        [RequireUser]
        public ActionResult LinkLogin(string id)
        {
            // Request a redirect to the external login provider to link a login for the current user
            return new ProfileController.ChallengeResult(id, Url.Action("LinkLoginCallback", "Profile"), UserAuthorization.CurrentUser.UserId.ToString());

        }

        
        public async Task<ActionResult> LinkLoginCallback()
        {

            var t = await AuthenticationManager.GetExternalLoginInfoAsync();
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync(XsrfKey, UserAuthorization.CurrentUser.UserId.ToString());
            if (loginInfo == null)
            {
                SoundPower.Web.Notifications.AddErrorNotification(ResourceText.ProfilePages.FailedExternalLink);
                return RedirectToAction("index",new { badurl = Request.RawUrl });

            }

            Account.UserServiceClient svc = null;
            ResponseBase rsp = null;
            string src = loginInfo.Login.LoginProvider;

            try
            {
                var token = UserAuthorization.CurrentUser;
                svc = new Account.UserServiceClient(SiteUtilities.AccountService, token, SiteUtilities.CurrentCulture);
                rsp = svc.AssociateExternal(src,loginInfo.Login.ProviderKey);

            }
            catch (Exception ex)
            {
                var bex= new SoundPower.ErrorTracking.BaseException("Save Failure", "TherapyCorner.Controllers.Profile.LinkLoginCallback", ex);
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
                UserAuthorization.ResetCachedToken();

                SoundPower.Web.Notifications.AddSuccessMessages(string.Format(ResourceText.ProfilePages.SuccessfulExternalLink, loginInfo.Email?? loginInfo.ExternalIdentity.Name));
            }

            return RedirectToAction("index");
        }

        [RequireUser]
        [HttpGet]
        public ActionResult Edit()
        {
            Account.UserServiceClient svc = null;
            UserInfoResponse rsp = null;
            try
            {
                var token = UserAuthorization.CurrentUser;
                svc = new Account.UserServiceClient(SiteUtilities.AccountService, token, SiteUtilities.CurrentCulture);
                rsp = svc.Details(token.UserId);

            }
            catch (Exception ex)
            {
                var bex= new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Controllers.Profile.Edit", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {
                if (svc != null) svc.Dispose();
            }

            if (rsp.IsFailure)
            {
                throw new BaseException("Failure Received", "TherapyCorner.Controllers.Profile.Edit");
            }
            SoundPower.Web.Notifications.AddResponseNotifications(rsp);
            return View(rsp.User);
        }

        [RequireUser]
        [AntiForgeryHandleError]
        [HttpPost]
        public ActionResult Edit(www.therapycorner.com.account.UserInfo model)
        {
            if (ModelState.IsValid)
            {
                Account.UserServiceClient svc = null;
                ResponseBase rsp = null;
                try
                {
                    var token = UserAuthorization.CurrentUser;
                    svc = new Account.UserServiceClient(SiteUtilities.AccountService, token, SiteUtilities.CurrentCulture);
                    rsp = svc.Update(model);

                }
                catch (Exception ex)
                {
                    var bex= new SoundPower.ErrorTracking.BaseException("Save Processing Failure", "TherapyCorner.Controllers.Profile.Edit", ex);
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
                     UserAuthorization.ResetCachedToken();
                   SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.ProfilePages.SaveSuccess);
                    return RedirectToAction("index");
                }
                SiteUtilities.ApplyFieldIssues(rsp, ModelState);
            }
            return View(model);
        }


        [RequireUser]
        [AntiForgeryHandleError]
        [HttpPost]
        public ActionResult RemoveExternal()
        {
    
                Account.UserServiceClient svc = null;
                ResponseBase rsp = null;
                try
                {
                    var token = UserAuthorization.CurrentUser;
                    svc = new Account.UserServiceClient(SiteUtilities.AccountService, token, SiteUtilities.CurrentCulture);
                    rsp = svc.RemoveExternal();

                }
                catch (Exception ex)
                {
                    var bex= new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Controllers.Profile.RemoveExternal", ex);
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
                    SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.ProfilePages.RemoveExternalSuccess );
                }
            UserAuthorization.ResetCachedToken();
            return RedirectToAction("index");


        }
        [RequireUser]
        [AntiForgeryHandleError]
        [HttpPost]
        public ActionResult RemoveCompany(string company)
        {
            if (ModelState.IsValid)
            {
                Account.UserServiceClient svc = null;
                ResponseBase rsp = null;
                try
                {
                    svc = new Account.UserServiceClient(SiteUtilities.AccountService, UserAuthorization.CurrentUser, SiteUtilities.CurrentCulture);
                    rsp = svc.RemoveCompany(company);


                }

                catch (Exception ex)
                {

                    BaseException iex = new BaseException("Processing Failure", "TherapyCorner.Controllers.Profile.RemoveCompany", ex);
                    SoundPower.Web.Utilities.ReportError(iex);
                    throw iex;
                }
                finally
                {
                    if (svc != null) svc.Dispose();
                }

                SoundPower.Web.Notifications.AddResponseNotifications(rsp);
                if (!rsp.IsFailure)
                {
                    UserAuthorization.ResetCachedToken();
                    SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.ProfilePages.RemoveCompanySuccess);
                }

            }

            return RedirectToAction("index");
        }

        [HttpGet]
        public ActionResult RemoveCompany()
        {
            SoundPower.Web.Notifications.AddErrorNotification(ResourceText.SharedPages.CouldNotProcessRelogin);
            return RedirectToAction("index");
        }
        [RequireUser]
        [HttpGet]
        public ActionResult SetDefault(string id)
        {
            if (ModelState.IsValid)
            {
                Account.UserServiceClient svc = null;
                ResponseBase rsp = null;
                try
                {
                    svc = new Account.UserServiceClient(SiteUtilities.AccountService, UserAuthorization.CurrentUser, SiteUtilities.CurrentCulture);
                    rsp = svc.DefaultCompany(id);


                }

                catch (Exception ex)
                {

                    BaseException iex = new BaseException("Processing Failure", "TherapyCorner.Controllers.Profile.SetDefault", ex);
                    SoundPower.Web.Utilities.ReportError(iex);
                    throw iex;
                }
                finally
                {
                    if (svc != null) svc.Dispose();
                }

                SoundPower.Web.Notifications.AddResponseNotifications(rsp);
                if (!rsp.IsFailure)
                {
                    UserAuthorization.ResetCachedToken();
                    SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.ProfilePages.RemoveCompanySuccess);
                }

            }

            return RedirectToAction("index");
        }

        [RequireUser]

        public ActionResult AddCompany(string inviteCode)
        {
            if (string.IsNullOrWhiteSpace(inviteCode))
            {
                SoundPower.Web.Notifications.AddErrorNotification(ProfilePages.InviteCodeRequired);
                return RedirectToAction("index");
            }
            if (ModelState.IsValid)
            {
                Account.CompanyServiceClient svc = null;
                ResponseBase rsp = null;
                try
                {
                    svc = new Account.CompanyServiceClient(SiteUtilities.AccountService, UserAuthorization.CurrentUser, SiteUtilities.CurrentCulture);
                    rsp = svc.UseInvite(inviteCode);


                }

                catch (Exception ex)
                {

                    BaseException iex = new BaseException("Processing Failure", "TherapyCorner.Controllers.Profile.AddCompany", ex);
                    SoundPower.Web.Utilities.ReportError(iex);
                    throw iex;
                }
                finally
                {
                    if (svc != null) svc.Dispose();
                }

                SoundPower.Web.Notifications.AddResponseNotifications(rsp);
                if (!rsp.IsFailure)
                {
                    SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.ProfilePages.AddCompanySuccess);
                    UserAuthorization.ResetCachedToken();
                }

            }

            return RedirectToAction("index");
        }

        [RequireUser]
        [HttpGet]
        public ActionResult ChangePassword()
        {
            return View(new Models.ChangePasswordModel());
        }

        [RequireUser]
        [AntiForgeryHandleError]
        [HttpPost]
        public ActionResult ChangePassword(Models.ChangePasswordModel model)
        {
            if (string.IsNullOrWhiteSpace(model.CurrentPassword))
            {
                ModelState.AddModelError("CurrentPassword", www.therapycorner.com.account.ResStrings.ValidationText.Required);
            }
            if (model.CurrentPassword == model.Password)
            {
                ModelState.AddModelError("Password", ResourceText.ProfilePages.NewNotEqualOldPassword);

            }
            if (ModelState.IsValid)
            {
                Account.UserServiceClient svc = null;
                ResponseBase rsp = null;
                try
                {
                    svc = new Account.UserServiceClient(SiteUtilities.AccountService, UserAuthorization.CurrentUser, SiteUtilities.CurrentCulture);
                    int id = int.Parse(UserAuthorization.CurrentUser.User.UniqueId);
                    var rsp2 = svc.Login(id);

                    var rsp3 = svc.VerifyPassword(new PasswordRequest() { Password = Account.Security.EncryptProviderPassword(rsp2.ObjectId, model.CurrentPassword) });
                    if (rsp3.IsFailure)
                    {
                        rsp = rsp3;
                    }
                    else
                    {
                        rsp = svc.ChangePassword(new PasswordRequest() { Password = Account.Security.EncryptProviderPassword(rsp2.ObjectId, model.Password) });
                    }

                }

                catch (Exception ex)
                {

                    BaseException iex = new BaseException("Processing Failure", "TherapyCorner.Controllers.Profile.ChangePassword", ex);
                    SoundPower.Web.Utilities.ReportError(iex);
                    throw iex;
                }
                finally
                {
                    if (svc != null) svc.Dispose();
                }

                SoundPower.Web.Notifications.AddResponseNotifications(rsp);
                if (!rsp.IsFailure)
                {
                    SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.ProfilePages.ChangePasswordSuccess);
                    return RedirectToAction("index");
                }
                else
                {
                    SiteUtilities.ApplyFieldIssues(rsp, ModelState);

                }
            }
            model.Password = "";
            model.RptPassword = "";
            return View(model);
        }


        [HttpPost]
        [AllowAnonymous]
        public ActionResult LogOff()
        {
            var token = UserAuthorization.CurrentUser;
            if (token != null)
            {
                Account.SessionServiceClient svc = null;
                www.soundpower.biz.common.ResponseBase rsp = null;
                try
                {
                    svc = new Account.SessionServiceClient(SiteUtilities.AccountService, UserAuthorization.CurrentUser, SiteUtilities.CurrentCulture);
                    rsp = svc.KillSession();
                }
                catch (Exception ex)
                {
                    throw new SoundPower.ErrorTracking.BaseException("Authentication Failure", "TherapyCorner.Controllers.Profile.Logoff", ex);
                }
                finally
                {
                    if (svc != null) svc.Dispose();
                }
            SoundPower.Web.Notifications.AddInformationNotification(ResourceText.SharedPages.LoggedOut);
             AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
           }
            return RedirectToAction("login");
        }


        [HttpGet]
        [AllowAnonymous]
        public ActionResult ForgotId()
        {


            return View(new www.therapycorner.com.account.MessageContracts.ContactInfoRequest());
        }

        [HttpPost]
        [CaptchaValidator]
        [AntiForgeryHandleError]
        [AllowAnonymous]
        public ActionResult ForgotId(www.therapycorner.com.account.MessageContracts.ContactInfoRequest model, bool captchaValid)
        {
         
            if (ModelState.IsValid)
            {
                Account.UserServiceClient svc = null;
                ResponseBase rsp = null;
                try
                {
                    svc = new Account.UserServiceClient(SiteUtilities.AccountService, null, SiteUtilities.CurrentCulture);

                    rsp = svc.SendLogin(model);


                }

                catch (Exception ex)
                {
                    var ctx = new Dictionary<string, string>();
                    ctx.Add("Model", Utilities.SerializeDataContractToXML(model));
                    BaseException iex = new BaseException("Processing Failure", "TherapyCorner.Controllers.Profile.Login", ex,ctx);
                    SoundPower.Web.Utilities.ReportError(iex);
                    throw iex;
                }
                finally
                {
                    if (svc != null) svc.Dispose();
                }

                SoundPower.Web.Notifications.AddResponseNotifications(rsp);
                if (!rsp.IsFailure)
                {
                    return View("forgotidconfirm");
                }
                else
                {
                    SiteUtilities.ApplyFieldIssues(rsp, ModelState);

                }
            }

            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View(new Models.ForgotPasswordLogin());
        }


        [RequireUser]
        [HttpGet]
        public ActionResult Validate(string returnUrl)
        {

            Account.UserServiceClient svc = null;
            ContactInfoResponse rsp = null;
            try
            {
                svc = new Account.UserServiceClient(SiteUtilities.AccountService, UserAuthorization.CurrentUser, SiteUtilities.CurrentCulture);
                rsp = svc.ContactInfo("0");


            }

            catch (Exception ex)
            {

                BaseException iex = new BaseException("Processing Failure", "TherapyCorner.Controllers.Profile.Validate", ex);
                SoundPower.Web.Utilities.ReportError(iex);
                throw iex;
            }
            finally
            {
                if (svc != null) svc.Dispose();
            }

            SoundPower.Web.Notifications.AddResponseNotifications(rsp);
            if (!rsp.IsFailure)
            {
                ViewBag.Email = rsp.Email;
                ViewBag.Mobile = rsp.Mobile;
                ViewBag.ReturnUrl = returnUrl;
            }
            else
            {
                throw new BaseException("Failure Received", "TherapyCorner.Controllers.Profile.Validate");

            }

            return View(new CreateVerificationRequest { Id=(UserAuthorization.CurrentUser.User.UniqueId) });
        }

        [RequireUser]
        [AntiForgeryHandleError]
        [HttpPost]
        public ActionResult Validate(www.therapycorner.com.account.MessageContracts.CreateVerificationRequest model, string email, string mobile,string returnUrl,bool? resend)
        {
            if (ModelState.IsValid )
            {
          
                    Account.VerificationClientService svc = null;
                    ObjectIdResponseBase rsp = null;
                    try
                    {
                        svc = new Account.VerificationClientService(SiteUtilities.AccountService, UserAuthorization.CurrentUser, SiteUtilities.CurrentCulture);

                        rsp = svc.Create(model);
                    
                    }

                    catch (Exception ex)
                    {
                    Dictionary<string, string> ctx = new Dictionary<string, string>();
                    ctx.Add("Email", email ?? "");
                    ctx.Add("UserId", model.Id ?? "");
                    ctx.Add("ByEmail", model.ByEmail.ToString());
                        BaseException iex = new BaseException("Processing Failure", "TherapyCorner.Controllers.Profile.Validate", ex,ctx);
                        SoundPower.Web.Utilities.ReportError(iex);
                        throw iex;
                    }
                    finally
                    {
                        if (svc != null) svc.Dispose();
                    }

                    SoundPower.Web.Notifications.AddResponseNotifications(rsp);
                    if (!rsp.IsFailure)
                    {
                    if (resend.GetValueOrDefault(false)) SoundPower.Web.Notifications.AddSuccessMessages("Verification code was successfully resent.");
                        return View("validateconfirm", new Models.ValidationCode() { ReturnURL = returnUrl, ByEmail = model.ByEmail, UserId = int.Parse(model.Id) , ValidationId= int.Parse(rsp.ObjectId)});
                    }
                    else
                    {
                        SiteUtilities.ApplyFieldIssues(rsp, ModelState);

                    }
                
            }
            ViewBag.Email = email;
            ViewBag.Mobile = mobile;
            ViewBag.ReturnUrl = returnUrl;

            return View(model);


        }

        [RequireUser]
        [HttpGet]
        public ActionResult PasswordExpired(string returnUrl)
        {
            return View(new Models.ChangePasswordModel() { ReturnURL = returnUrl });
        }

        [RequireUser]
        [AntiForgeryHandleError]
        [HttpPost]
        public  ActionResult ValidateConfirm(Models.ValidationCode model)
        {

            if (ModelState.IsValid)
            {
                var deviceInfo = GetDeviceInfo();

                Account.VerificationClientService svc = null;
                Account.UserServiceClient usvc = null;
                ObjectIdResponseBase rsp = null;
                try
                {
                    svc = new Account.VerificationClientService(SiteUtilities.AccountService, UserAuthorization.CurrentUser, SiteUtilities.CurrentCulture);

                    rsp = svc.Verify(new VerificationRequest() { Code = model.Code, Id = model.ValidationId, UserId = model.UserId, DeviceCode = deviceInfo.DeviceCode, DeviceIP = deviceInfo.DeviceIP });

                    if (!rsp.IsFailure && model.HomeDevice)
                    {
                        usvc = new Account.UserServiceClient(SiteUtilities.AccountService, UserAuthorization.CurrentUser, SiteUtilities.CurrentCulture);
                       var rsp2= usvc.SetHomeDevice(deviceInfo.DeviceCode, deviceInfo.DeviceIP);
                        SoundPower.Web.Notifications.AddResponseNotifications(rsp2);

                    }
                }

                catch (Exception ex)
                {

                    BaseException iex = new BaseException("Processing Failure", "TherapyCorner.Controllers.Profile.ValidateConfirm", ex);
                    SoundPower.Web.Utilities.ReportError(iex);
                    throw iex;
                }
                finally
                {
                    if (svc != null) svc.Dispose();
                    if (usvc != null) usvc.Dispose();
                }



                SoundPower.Web.Notifications.AddResponseNotifications(rsp);
                if (!rsp.IsFailure)
                {
                    UserAuthorization.ResetCachedToken();
                    return RedirectToLocal(model.ReturnURL);
                }
                else
                {
                    SiteUtilities.ApplyFieldIssues(rsp, ModelState);

                }
            }

            return View(model);
        }


        [RequireUser]
        [AntiForgeryHandleError]
        [HttpPost]
        public ActionResult PasswordExpired(Models.ChangePasswordModel model)
        {
            if (ModelState.IsValid)
            {
                Account.UserServiceClient svc = null;
                ResponseBase rsp = null;
                try
                {
                    svc = new Account.UserServiceClient(SiteUtilities.AccountService, UserAuthorization.CurrentUser, SiteUtilities.CurrentCulture);
                    int id = int.Parse(UserAuthorization.CurrentUser.User.UniqueId);
                    var rsp2 = svc.Login(id);
                    rsp = svc.ChangePassword(new PasswordRequest() { Password = Account.Security.EncryptProviderPassword(rsp2.ObjectId, model.Password) });


                }

                catch (Exception ex)
                {

                    BaseException iex = new BaseException("Processing Failure", "TherapyCorner.Controllers.Profile.PasswordExpired", ex);
                    SoundPower.Web.Utilities.ReportError(iex);
                    throw iex;
                }
                finally
                {
                    if (svc != null) svc.Dispose();
                }

                SoundPower.Web.Notifications.AddResponseNotifications(rsp);
                if (!rsp.IsFailure)
                {
                    UserAuthorization.ResetCachedToken();
                    return RedirectToLocal(model.ReturnURL);
                }
                else
                {
                    SiteUtilities.ApplyFieldIssues(rsp, ModelState);

                }
            }
            model.Password = "";
            model.RptPassword = "";
            return View(model);
        }

        [AntiForgeryHandleError]
        [HttpPost]
        public ActionResult ForgotPassword(Models.ForgotPasswordLogin model)
        {
            if (ModelState.IsValid)
            {
                Account.UserServiceClient svc = null;
                ContactInfoResponse rsp = null;
                try
                {
                    svc = new Account.UserServiceClient(SiteUtilities.AccountService, null, SiteUtilities.CurrentCulture);

                    rsp = svc.ContactInfo(model.Login);


                }

                catch (Exception ex)
                {
                    var ctx = new Dictionary<string, string>();
                    ctx.Add("Model", Utilities.SerializeDataContractToXML(model));
                    BaseException iex = new BaseException("Processing Failure", "TherapyCorner.Controllers.Profile.ForgotPassword", ex,ctx);
                    SoundPower.Web.Utilities.ReportError(iex);
                    throw iex;
                }
                finally
                {
                    if (svc != null) svc.Dispose();
                }

                SoundPower.Web.Notifications.AddResponseNotifications(rsp);
                if (!rsp.IsFailure)
                {
                    ViewBag.Email = rsp.Email;
                    ViewBag.Mobile = rsp.Mobile;
                    return View("forgotpasswordcontact", new CreateVerificationRequest() { Id = rsp.Id.ToString() } );
                }
                else
                {
                    SiteUtilities.ApplyFieldIssues(rsp, ModelState);

                }
            }

            return View(model);
        }

        [AntiForgeryHandleError]
        [HttpPost]
        public ActionResult ForgotPasswordContact(www.therapycorner.com.account.MessageContracts.CreateVerificationRequest model, string email, string mobile)
        {
            if (ModelState.IsValid)
            {
               if (int.Parse(model.Id)<1)
                {
                    return View("forgotpasswordconfirm", new Models.ValidationCode() { UserId = 0, ValidationId = 0 });
                }
               else
                {
                    Account.VerificationClientService svc = null;
                    ObjectIdResponseBase rsp = null;
                    try
                    {
                        svc = new Account.VerificationClientService(SiteUtilities.AccountService, null, SiteUtilities.CurrentCulture);

                        rsp = svc.Create(model);


                    }

                    catch (Exception ex)
                    {
                        var ctx = new Dictionary<string, string>();
                        ctx.Add("Model", Utilities.SerializeDataContractToXML(model));
                        BaseException iex = new BaseException("Processing Failure", "TherapyCorner.Controllers.Profile.ForgotPasswordContact", ex,ctx);
                        SoundPower.Web.Utilities.ReportError(iex);
                        throw iex;
                    }
                    finally
                    {
                        if (svc != null) svc.Dispose();
                    }

                    SoundPower.Web.Notifications.AddResponseNotifications(rsp);
                    if (!rsp.IsFailure)
                    {
 
                        return View("forgotpasswordconfirm", new Models.ValidationCode() { UserId = int.Parse(model.Id), ValidationId = int.Parse( rsp.ObjectId) });
                    }
                    else
                    {
                        SiteUtilities.ApplyFieldIssues(rsp, ModelState);

                    }
                }
            }
            ViewBag.Email = email;
            ViewBag.Mobile = mobile;
            return View(model);
        }


        [AntiForgeryHandleError]
        [HttpPost]
        public async Task<ActionResult> ForgotPasswordConfirm(Models.ValidationCode model)
        {
            if (model.ValidationId <1)
            {
                ModelState.AddModelError("Code", ProfilePages.InvalidCode);
            }
            if (ModelState.IsValid)
            {
                var deviceInfo = GetDeviceInfo();

                Account.VerificationClientService svc = null;
                SessionResponse rsp = null;
                try
                {
                    svc = new Account.VerificationClientService(SiteUtilities.AccountService, null, SiteUtilities.CurrentCulture);

                    rsp = svc.StartSessionVerify(new VerificationRequest() { Code = model.Code, Id = model.ValidationId, UserId = model.UserId, DeviceCode = deviceInfo.DeviceCode, DeviceIP = deviceInfo.DeviceIP }  );


                }

                catch (Exception ex)
                {
                    var ctx = new Dictionary<string, string>();
                    ctx.Add("Model", Utilities.SerializeDataContractToXML(model));
                    BaseException iex = new BaseException("Processing Failure", "TherapyCorner.Controllers.Profile.ForgotPasswordConfirm", ex,ctx);
                    SoundPower.Web.Utilities.ReportError(iex);
                    throw iex;
                }
                finally
                {
                    if (svc != null) svc.Dispose();
                }

                HttpCookie cookie = new HttpCookie("tcdid", deviceInfo.HashedCode);
                cookie.Expires = DateTime.Now.AddDays(30);
                Response.Cookies.Add(cookie);

                SoundPower.Web.Notifications.AddResponseNotifications(rsp);
                if (!rsp.IsFailure)
                {
                    await SignInAsync(new ApplicationUser(rsp.Session), false);
                    System.Web.HttpContext.Current.Items["NewUsrId"] = rsp.Session.SessionId;
                    System.Web.HttpContext.Current.Items["UserToken"] = rsp.Session;

                    return RedirectToAction("passwordExpired");
                }
                else
                {
                    SiteUtilities.ApplyFieldIssues(rsp, ModelState);

                }
            }

            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "None")]
        public ActionResult Login(string returnUrl, string ic)
        {
            if (User.Identity.IsAuthenticated)
            {
                AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);

            }

            return View(new Models.LoginViewModel() { ReturnURL = returnUrl, InitialCompany=ic });
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult ExternalLogin(string provider, string returnUrl, int? timeoffset,string InitialCompany)
        {
            // Request a redirect to the external login provider
            ChallengeResult result = new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Profile", new { ReturnUrl = returnUrl, InitialCompany = InitialCompany }));
            return result;
        }


        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl,string InitialCompany)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                SoundPower.Web.Notifications.AddErrorNotification("Failed external login attempt!");
                return View("Login", new Models.LoginViewModel() { ReturnURL = returnUrl, InitialCompany= InitialCompany });
            }

            var deviceInfo = GetDeviceInfo();
            Account.SessionServiceClient svc = null;
            SessionResponse rsp = null;
            string src = loginInfo.Login.LoginProvider;

            try
            {
                svc = new Account.SessionServiceClient(SiteUtilities.AccountService, null, SiteUtilities.CurrentCulture);

                rsp = svc.CreateExternal(new www.therapycorner.com.account.MessageContracts.LoginRequest()
                {
                    LoginId = loginInfo.Login.ProviderKey,
                    Password = src,
                    DeviceIP = deviceInfo.DeviceIP,
                    DeviceCode = deviceInfo.DeviceCode,
                    CompanyId= InitialCompany
                });


            }

            catch (Exception ex)
            {
                var ctx = new Dictionary<string, string>();
                ctx.Add("LoginId",loginInfo.Login.ProviderKey);
                BaseException iex = new BaseException("Processing Failure", "TherapyCorner.Controllers.Profile.ExternalLoginCallback", ex,ctx);
                SoundPower.Web.Utilities.ReportError(iex);
                throw iex;
            }
            finally
            {
                if (svc != null) svc.Dispose();
            }


            HttpCookie cookie = new HttpCookie("tcdid", deviceInfo.HashedCode );
            cookie.Expires = DateTime.Now.AddDays(30);
            Response.Cookies.Add(cookie);

            SoundPower.Web.Notifications.AddResponseNotifications(rsp);
            if (!rsp.IsFailure)
            {
                //If good go to sign in
                await SignInAsync(new ApplicationUser(rsp.Session), false);
                System.Web.HttpContext.Current.Items["NewUsrId"] = rsp.Session.SessionId;
                System.Web.HttpContext.Current.Items["UserToken"] = rsp.Session;

                return RedirectToLocal(returnUrl);
            }

        
            // If we got this far, something failed,go back to login
            SoundPower.Web.Notifications.AddErrorNotification("Failed external login attempt!");
            return View("Login", new Models.LoginViewModel() { ReturnURL = returnUrl,InitialCompany= InitialCompany });
        }

        private Models.DeviceInfoModel GetDeviceInfo()
        {
            var result = new Models.DeviceInfoModel();
            if (Request.Cookies["tcdid"] != null)
            {
                // if the mvcvalue exists as a cookie, use the cookie to get its value
                result.HashedCode  = Request.Cookies["tcdid"].Value;
            }
            else
            {
                result.DeviceCode = Account.Security.RandomPassword();
            }
            result.DeviceIP = Request.UserHostAddress;
            return result;
        }

      
        [HttpPost]
        [AllowAnonymous]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "None")]
        public async Task<ActionResult> Login(Models.LoginViewModel model)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToLocal(model.ReturnURL);
            }
         

            if (ModelState.IsValid)
            {
                var deviceInfo = GetDeviceInfo();
                Account.SessionServiceClient svc = null;
                SessionResponse rsp = null;
                try
                {
                    svc = new Account.SessionServiceClient(SiteUtilities.AccountService, null, SiteUtilities.CurrentCulture);

                     rsp=svc.CreateInternal(new www.therapycorner.com.account.MessageContracts.LoginRequest()
                    {
                        LoginId = model.Login,
                        Password = Account.Security.EncryptProviderPassword(model.Login, model.Password),
                        DeviceIP=deviceInfo.DeviceIP,
                        DeviceCode= deviceInfo.DeviceCode,
                        CompanyId= model.InitialCompany
                     });


                }
      
                catch (Exception ex)
                {
                    var ctx = new Dictionary<string, string>();
                    model.Password = "BLOCKED";
                    ctx.Add("Model", Utilities.SerializeDataContractToXML(model));
                    BaseException iex = new BaseException("Processing Failure", "TherapyCorner.Controllers.Profile.Login", ex,ctx);
                    SoundPower.Web.Utilities.ReportError(iex);
                    throw iex;
                }
                finally
                {
                    if (svc != null) svc.Dispose();
                }


                HttpCookie cookie = new HttpCookie("tcdid", deviceInfo.HashedCode);
                cookie.Expires = DateTime.Now.AddDays(30);
                Response.Cookies.Add(cookie);

                SoundPower.Web.Notifications.AddResponseNotifications(rsp);
                if (!rsp.IsFailure)
                {
                    //If good go to sign in
                    await SignInAsync(new ApplicationUser(rsp.Session), false);
                    System.Web.HttpContext.Current.Items["NewUsrId"] = rsp.Session.SessionId;
                    System.Web.HttpContext.Current.Items["UserToken"] = rsp.Session;
                  
                    return RedirectToLocal(model.ReturnURL);
                }
                else
                {
                    SiteUtilities.ApplyFieldIssues(rsp, ModelState);
                }
            }
            return View(model);
            }

 

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (!string.IsNullOrWhiteSpace(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        private async Task SignInAsync(ApplicationUser user, bool isPersistent)
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);


            try
            {
                var identity = await PCustomUserManager.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie);

                AuthenticationManager.SignIn(new AuthenticationProperties() { IsPersistent = false, ExpiresUtc = DateTime.UtcNow.AddHours(8) }, identity);
            }
            catch (Exception ex)
            {
                int x = 1;
            }
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
    }
}