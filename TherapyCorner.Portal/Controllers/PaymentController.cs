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
    [CompanyFilter]
    [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
    [RequireBillerOrWorker]
    public class PaymentController : Controller
    {
        [HttpPost]
        public ActionResult Index(ClaimSearchRequest model, www.soundpower.biz.common.PersonField selClient)
        {
            if (selClient != null && !string.IsNullOrWhiteSpace(selClient.UniqueId))
            {
                model.Client = int.Parse(selClient.UniqueId);
            }
        
            Company.ClientServiceClient svcClient = null;
            Company.PaymentServiceClient svcClaim = null;
            List<string> serviceNames = new List<string>();
            string insurances = "";
            string govts = "";
            PaymentListResponse rspClients = null;
            string clientName = "";
            try
            {
                var token = UserAuthorization.CurrentUser;
                if (model.Client.HasValue)
                {
                    svcClient = new Company.ClientServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                    var rStaff = svcClient.Details(model.Client.Value);
                    clientName = rStaff.Client.ToPerson().LastFirstMI;
                }


                if (model.Insurance.HasValue)
                {
                    var ins = StaticData.AllInsuranceCompanies;
                    var i = ins.Find(t => t.CompanyId == model.Insurance.Value);
                    if (i != null) insurances = i.Name;

                }
                if (model.GovernmentProgram.HasValue)
                {
                    var g = StaticData.GovernmentPrograms;
                    var i = g.Find(t => t.ProgramId == model.GovernmentProgram.Value);
                    if (i != null) govts = i.Name;

                }
                svcClaim = new Company.PaymentServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                rspClients = svcClaim.Search(model);

            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.PaymentController.Index", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {
                if (svcClient != null) svcClient.Dispose();
                if (svcClaim != null) svcClaim.Dispose();

            }

            SoundPower.Web.Notifications.AddResponseNotifications(rspClients);
            if (rspClients.IsFailure) return RedirectToAction("billing", "home");
            ViewBag.Insurances = insurances;
            ViewBag.GovtPrograms = govts;
            ViewBag.Results = rspClients.Payments;
            ViewBag.Client = clientName;


            return View(model);
        }

        [HttpPost]
        public ActionResult Filter(ClaimSearchRequest model)
        {
            Company.InsuranceCompanyServiceClient svcComp = null;
            Company.GovernmentProgramServiceClient svcGovt = null;
            Company.ClientServiceClient svcClient = null;

            try
            {
                var token = UserAuthorization.CurrentUser;
                if (model.Client.HasValue)
                {
                    svcClient = new Company.ClientServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                    var rStaff = svcClient.Details(model.Client.Value);
                    ViewBag.ClientName = rStaff.Client.ToPerson().LastFirstMI;
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
                ViewBag.Govt = rspGovt.Programs;


            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.Payment.Filter", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {
                if (svcComp != null) svcComp.Dispose();
                if (svcGovt != null) svcGovt.Dispose();
                if (svcClient != null) svcClient.Dispose();
            }
            return View(model);
        }


        [HttpGet]
        public ActionResult Index(int? fromdays, int? todays,  int? client)
        {
            ClaimSearchRequest mdl = new ClaimSearchRequest();

            mdl.ToDays = todays.GetValueOrDefault(15);
            mdl.FromDays = fromdays;
         
            mdl.Client = client;

            return Index(mdl, null);
        }

        [HttpGet]
        public ActionResult Filter()
        {
            ClaimSearchRequest mdl = new ClaimSearchRequest();

            mdl.FromDays = 15;



            return Filter(mdl);
        }


        [HttpGet]
        public ActionResult Details(long id, int? activeTab)
        {

            Company.PaymentServiceClient svcClaim = null;
            ViewBag.ActiveTab = activeTab;
            PaymentResponse rspClients = null;
            try
            {
                var token = UserAuthorization.CurrentUser;

                svcClaim = new Company.PaymentServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                rspClients = svcClaim.Details(id);

            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.Payment.Details", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {
                if (svcClaim != null) svcClaim.Dispose();

            }

            SoundPower.Web.Notifications.AddResponseNotifications(rspClients);

            if (rspClients.IsFailure || rspClients.Payment == null)
            {
                return RedirectToAction("index");
            }

            return View(rspClients.Payment);
        }
    }
}