using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace TherapyCorner.Portal.Controllers
{
    [RequireUser]
    [CompanyFilter]
    [RequireHttps]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            Account.CredentialClientService svc = null;
            Company.StaffServiceClient svcStaff = null;
            //Company.ClientServiceServiceClient svcClientService = null;
            Company.CredentialRequirementServiceClient svcReqs = null;
            Account.UserServiceClient svcUser = null;

            try
            {
                var token = UserAuthorization.CurrentUser;
                    svc = new Account.CredentialClientService(SiteUtilities.AccountService, token, SiteUtilities.CurrentCulture);

                Company.StaffServiceClient staffDetails = null;
                staffDetails = new Company.StaffServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                var staffInfo = staffDetails.UserDetails(Convert.ToInt32(token.UserId), token.CurrentCompany);
                var staffDisciplines = staffInfo.Staff.Disciplines.ToList();

                var rspCreds = svc.List(token.UserId);
                SoundPower.Web.Notifications.AddResponseNotifications(rspCreds);
                svcReqs = new Company.CredentialRequirementServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                var rspReqs = svcReqs.List();
                SoundPower.Web.Notifications.AddResponseNotifications(rspReqs);
                int missingCount = 0;
                var toSkip = new string[] { "Speech Therapy", "Physical Therapy", "Occupational Therapy" };


                foreach (var r in rspReqs.Requirements)
                {
                    var credentialID = r.Type.UniqueId;

                    int count = rspCreds.Credentials.Count(c => c.Type.UniqueId == r.Type.UniqueId && c.ValidTo >= DateTime.Today && c.Validations.Exists(v => v.Company.AddressId == token.CurrentCompany && v.VerifiedAt.HasValue));
                    int reqCount = 0;
                    if (token.IsAdmin && r.AdminRequired > 0)
                    {
                        reqCount = r.AdminRequired;
                    }
                    if (token.IsProvider && r.ProviderRequired > reqCount)
                    {
                        reqCount = r.ProviderRequired;

                        if ((credentialID == "7" || credentialID == "10") && !staffDisciplines.Any(x => x.Name == "Speech Therapy"))
                        {
                            reqCount = reqCount - r.ProviderRequired;
                        }
                    }
                    if (token.IsWorker && r.WorkerRequired > reqCount)
                    {
                        reqCount = r.WorkerRequired;
                    }
                    if (token.IsSupervisor && r.SupervisorRequired > reqCount)
                    {
                        reqCount = r.SupervisorRequired;
                        if ((credentialID == "7" || credentialID == "10") && !staffDisciplines.Any(x => toSkip.Contains(x.Name)))
                        {
                            reqCount = reqCount - r.SupervisorRequired;
                        }

                    }
                    if (count < reqCount) missingCount += reqCount - count;
                }

                ViewBag.MissingCredCount = missingCount;
                var expDt = DateTime.Today.AddDays(30);
                var ignoreDT = DateTime.Today.AddDays(-20);
                int expCount = 0;
                foreach (var c in rspCreds.Credentials)
                {
                    if (c.ValidTo > expDt || c.ValidTo < ignoreDT) continue;
                    if (!rspCreds.Credentials.Exists(r => r.Type.UniqueId == c.Type.UniqueId && r.ValidTo > expDt))
                    {
                        expCount++;
                    }
                }
                ViewBag.ExpiringCount = expCount;

        

                if (token.IsAdmin || token.IsWorker)
                {
                    if (token.IsAdmin)
                    {
                      

                     


                        var rspCred = svc.Pending(token.CurrentCompany);
                        SoundPower.Web.Notifications.AddResponseNotifications(rspCred);
                        if (!rspCred.IsFailure && rspCred.Credentials != null)
                        {
                            ViewBag.PendingCredentials = rspCred.Credentials.Count();
                        }
                        else
                        {
                            ViewBag.PendingCredentials = 0;
                        }

                        svcUser = new Account.UserServiceClient(SiteUtilities.AccountService, token, SiteUtilities.CurrentCulture);
                        var rspOIG = svcUser.OIGMatches();
                        SoundPower.Web.Notifications.AddResponseNotifications(rspOIG);
                        if (!rspOIG.IsFailure && rspOIG.Matches != null)
                        {
                            ViewBag.OIG = rspOIG.Matches.Count();
                        }
                        else
                        {
                            ViewBag.OIG = 0;
                        }

                    }
                   
                    
                   


                }

              

                svcStaff = new Company.StaffServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);

                

                var rspStaff = svcStaff.UserDetails(token.UserId,token.CurrentCompany);
                ViewBag.StaffId = rspStaff.Staff.StaffId;

          
            }
            catch (Exception ex)
            {
                var bex= new SoundPower.ErrorTracking.BaseException("Homepage Processing Failure", "TherapyCorner.Controllers.Home.Index", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {
                if (svc != null) svc.Dispose();
                if (svcStaff != null) svcStaff.Dispose();

            }
            return View();
        }

   
   
        private void GetMakeupCounts(Company.ClientServiceServiceClient svc)
        {
            var rsp = svc.MakeupCounts();

            if (!rsp.IsFailure && rsp.Services != null) ViewBag.MakeupCounts = rsp.Services.Sum(s=>s.MakeupCount.GetValueOrDefault(0));
        }

 
       

        public ActionResult ChangeCompany(string id)
        {
            Account.SessionServiceClient svc = null;
            try
            {
                svc = new Account.SessionServiceClient(SiteUtilities.AccountService, UserAuthorization.CurrentUser, SiteUtilities.CurrentCulture);
               var rsp = svc.ChangeCompany(id);
                SoundPower.Web.Notifications.AddResponseNotifications(rsp);
                UserAuthorization.ResetCachedToken();
                UserAuthorization.SetMessageCount();
            }
            catch (Exception ex)
            {
                throw new SoundPower.ErrorTracking.BaseException("Authentication Failure", "TherapyCorner.Controllers.Home.ChangeCompany", ex);
            }
            finally
            {
                if (svc != null) svc.Dispose();
            }

            return RedirectToAction("index");
        }
      
        public ActionResult Finances()
        {
            Company.ExpenseClientService svcExp = null;
            Company.PayrollServiceClient svcPay = null;


            try
            {
                var token = UserAuthorization.CurrentUser;

                svcExp = new Company.ExpenseClientService(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                var rExp = svcExp.List(new www.therapycorner.com.company.MessageContracts.ExpenseSearchRequest()
                {
                    FromDT=DateTime.UtcNow.AddDays(-30),
                    ToDT = DateTime.UtcNow,
                    StaffId=-1
                });
                SoundPower.Web.Notifications.AddResponseNotifications(rExp);

                if (rExp.Expenses != null && rExp.Expenses.Count > 0) rExp.Expenses.Sort((a, b) => a.MadeOn.CompareTo(b.MadeOn));
                ViewBag.Expenses = rExp.Expenses;

                svcPay = new Company.PayrollServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                var rPay = svcPay.StaffList(new www.therapycorner.com.company.MessageContracts.PayrollSearchRequest() { Status = www.therapycorner.com.company.MessageContracts.PayrollSearchRequest.STATUS_PENDING } );
                SoundPower.Web.Notifications.AddResponseNotifications(rPay);

                List<Models.PayrollGroup> pays = new List<Models.PayrollGroup>();
                if (rPay.Entries != null && rPay.Entries.Count>0)
                {
                    var grps = from e in rPay.Entries
                               let grp = string.Format("{0}-{1}-{2}", e.SrcType.Context, e.SrcType.UniqueId,e.ForApproval)
                               group e by grp into g
                               select new { GroupKey = g.Key, Entries = g.ToList() };

                    foreach(var g in grps)
                    {
                        pays.Add(new Models.PayrollGroup()
                        {
                            Title = string.Format("{0} {1}", g.Entries[0].SrcType.Name, g.Entries[0].ForApproval ? "(Review)" : ""),
                            FromDT=g.Entries.Min(i=>i.DoneOn.Date),
                            ToDT = g.Entries.Max(i=>i.DoneOn.Date),
                            Amount=g.Entries.Sum(i=>i.Amount*i.Rate)
                        });
                    }
                }
                ViewBag.Payroll = pays;
            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Controllers.Home.Finances", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {
                if (svcExp != null) svcExp.Dispose();
                if (svcPay != null) svcPay.Dispose();


            }
            return View();
        }

        [RequireBiller]
        public ActionResult Billing()
        {
            Company.ClaimServiceClient svcExp = null;

            www.therapycorner.com.company.ClaimInfoList claims = null;
            try
            {
                var token = UserAuthorization.CurrentUser;

                svcExp = new Company.ClaimServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                var rExp = svcExp.Search(new www.therapycorner.com.company.MessageContracts.ClaimSearchRequest()
                {
                   Statuses= ClaimController.slPendingClaims
                });
                SoundPower.Web.Notifications.AddResponseNotifications(rExp);
                claims = rExp.Claims;
               
            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Controllers.Home.Billing", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {
                if (svcExp != null) svcExp.Dispose();


            }
            return View(claims);
        }
    }
}