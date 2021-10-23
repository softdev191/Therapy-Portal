using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using www.soundpower.biz.common;
using www.therapycorner.com.company;
using www.therapycorner.com.company.MessageContracts;

namespace TherapyCorner.Portal.Controllers
{
    [RequireHttps]
    [RequireBiller]
    [CompanyFilter]
    [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]

    public class ClaimController : Controller
    {
        public const string slPendingClaims = "0,1,2,3,4,7,9,10,11";
        public const string slClaimsReqAttn = "9,10,11";
        public ActionResult Comment(long id)
        {
            SoundPower.Web.Notifications.AddErrorNotification(ResourceText.SharedPages.CouldNotProcessRelogin);
            return RedirectToAction("details", new { id = id, activeTab = 1 });

        }

        public class ClaimsByAge
        {
           public string Name { get; private set; }
            public int Count { get; set; }
           public  decimal Value { get; set; }
            public ClaimsByAge(string name)
            {
                Name = name;
                Count = 0;
                Value = 0;
            }
        }
        public ActionResult AgePie()
        {
            List<ClaimsByAge> mdl = null;
            mdl = PendingByAge();

            return View(mdl);
        }

        public ActionResult AgeBar()
        {
            List<ClaimsByAge> mdl = null;
            mdl = PendingByAge();

            return View(mdl);
        }

        private static List<ClaimsByAge> PendingByAge()
        {
            List<ClaimsByAge> mdl = new List<ClaimsByAge>();
            mdl.Add(new ClaimsByAge(ResourceText.SharedPages.Claims30));
            mdl.Add(new ClaimsByAge(ResourceText.SharedPages.Claims60));
            mdl.Add(new ClaimsByAge(ResourceText.SharedPages.Claims90));

            Company.ClaimServiceClient svcClaim = null;

            ClaimInfoListResponse rspClients = null;
            try
            {
                var token = UserAuthorization.CurrentUser;

                svcClaim = new Company.ClaimServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                rspClients = svcClaim.Search(new ClaimSearchRequest() { Statuses = slPendingClaims });
                int indx = 0;
                double age = 0;
                foreach (var c in rspClients.Claims)
                {
                    age = DateTime.Now.Subtract(c.ClaimDate).TotalDays;
                    if (age <= 30)
                    {
                        indx = 0;
                    }
                    else if (age <= 60)
                    {
                        indx = 1;
                    }
                    else
                    {
                        indx = 2;
                    }
                    mdl[indx].Count++;
                    mdl[indx].Value += c.AmountDue;
                }
            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.Claim.PendingByAge", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {
                if (svcClaim != null) svcClaim.Dispose();

            }

            return mdl;
        }
        [HttpGet]
        public ActionResult Void(long id)
        {
            SoundPower.Web.Notifications.AddErrorNotification(ResourceText.SharedPages.CouldNotProcessRelogin);
            return RedirectToAction("details", new { id = id });

        }

        [HttpGet]
        public ActionResult Resolve(string ids)
        {
            if (string.IsNullOrWhiteSpace(ids))
            {
                SoundPower.Web.Notifications.AddErrorNotification(ResourceText.SharedPages.CouldNotProcessRelogin);
              return RedirectToAction("index");
          }

            if(ids.Contains(","))
            {
                Company.ClaimServiceClient svcClient = null;

                try
                {
                    var token = UserAuthorization.CurrentUser;


                    svcClient = new Company.ClaimServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                    ClaimInfoListResponse rspLst =  svcClient.SimpleList(new ObjectIdRequestBase() { ObjectId = ids }) ;

                    if (rspLst.Claims != null && rspLst.Claims.Count > 0)
                    {
                      
                        rspLst.Claims.RemoveAll(c => c.Status != ClaimStatusEnum.PendInsSubmissionIssue && c.Status != ClaimStatusEnum.PendGovtIssue);
                        if (rspLst.Claims.Count>0)
                        {
                            var fc = rspLst.Claims[0];
                            rspLst.Claims.RemoveAll(c => c.Client.UniqueId != fc.Client.UniqueId || c.PendingWith.UniqueId != fc.PendingWith.UniqueId || c.PendingWith.Context != fc.PendingWith.Context);
                        }
                    }

                    var validIds = from i in ids.Split(',')
                                   where rspLst.Claims.Exists(c => c.ClaimId.ToString() == i)
                                   select i;

                    if (validIds.Count()==0)
                    {
                        SoundPower.Web.Notifications.AddErrorNotification("No claims pending issue were selected");
                        return RedirectToAction("index", new { status = slClaimsReqAttn });
                    }
                    ids = string.Join(",", validIds);
                }
                catch (Exception ex)
                {
                    var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.Client.Resolve", ex);
                    SoundPower.Web.Utilities.ReportError(bex);
                    throw bex;

                }
                finally
                {
                    if (svcClient != null) svcClient.Dispose();

                }

            }

            return View(new ResolveClaimIssueRequest { ClaimIds = ids });
        }

        [AntiForgeryHandleError]
        [HttpPost]
        public ActionResult Resolve(ResolveClaimIssueRequest request)
        {
            if (ModelState.IsValid)
            {
                Company.ClaimServiceClient svcClient = null;
                ResponseBase rsp = null;

                try
                {
                    var token = UserAuthorization.CurrentUser;


                    svcClient = new Company.ClaimServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                    rsp = svcClient.Resolve(request);

                }
                catch (Exception ex)
                {
                    var bex = new SoundPower.ErrorTracking.BaseException("Save Processing Failure", "TherapyCorner.Portal.Controllers.Claim.Resolve", ex);
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
                      SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.ClaimsPages.ClaimIssueResolved);
                  if (request.ClaimIds.Contains(","))
                    {
                        return RedirectToAction("index", new { status = slClaimsReqAttn });

                    }
                    else
                    {
                    return RedirectToAction("details", new { id = request.ClaimIds });

                    }
                }
                else
                {
                    SiteUtilities.ApplyFieldIssues(rsp, ModelState);

                }
            }
            return View(request);
        }

        [AntiForgeryHandleError]
        [HttpPost]
        public ActionResult Comment(long id, string comment)
        {
            if (!string.IsNullOrWhiteSpace(comment))
            {
                Company.ClaimServiceClient svcClient = null;
                ResponseBase rsp = null;

                try
                {
                    var token = UserAuthorization.CurrentUser;


                    svcClient = new Company.ClaimServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                    rsp = svcClient.Comment(new CommentRequest() { CommentText = comment, Id = id });

                }
                catch (Exception ex)
                {
                    var bex = new SoundPower.ErrorTracking.BaseException("Save Processing Failure", "TherapyCorner.Portal.Controllers.Claim.Comment", ex);
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

                    SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.ClaimsPages.Commented);
                }

            }
            return RedirectToAction("details", new { id = id, activeTab = 1 });
        }

        [AntiForgeryHandleError]
        [HttpPost]
        public ActionResult Void(long id, string comment, long subid)
        {
            if (!string.IsNullOrWhiteSpace(comment))
            {
                Company.ClaimServiceClient svcClient = null;
                ResponseBase rsp = null;

                try
                {
                    var token = UserAuthorization.CurrentUser;


                    svcClient = new Company.ClaimServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                    rsp = svcClient.Void(new CommentRequest() { CommentText = comment, Id = id , SubId=subid});

                }
                catch (Exception ex)
                {
                    var bex = new SoundPower.ErrorTracking.BaseException("Save Processing Failure", "TherapyCorner.Portal.Controllers.Claim.Void", ex);
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

                    SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.ClaimsPages.PaymentVoided);
                }

            }
            return RedirectToAction("details", new { id = id });
        }

        [OutputCache(NoStore = true, Duration = 0)]
        public ActionResult ToPDF(long id)
        {
            Company.ClaimServiceClient svcNotes = null;
            ClaimInfoResponse rsp = null;
            var context = new Dictionary<string, string>();
            context.Add("id", id.ToString());
            Company.Exports.ClaimExport exporter = null;

            byte[] renderedBytes = null;
            string mimeType = null;
            string extension = null;
            Account.CompanyServiceClient svcCompany = null;

            try
            {
                var token = UserAuthorization.CurrentUser;

                svcNotes = new Company.ClaimServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);

                rsp = svcNotes.Details(id);

           
                    svcCompany = new Account.CompanyServiceClient(SiteUtilities.AccountService, token, SiteUtilities.CurrentCulture);
                    var rspCompany = svcCompany.Details(token.CurrentCompany);

                    exporter = new Company.Exports.ClaimExport(rsp.Claim, rspCompany.Company);
                    renderedBytes = Company.Exports.ExportUtilities.ToPDF(exporter.GenerateReport(), out mimeType, out extension);

                

            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Report Processing Failure", "TherapyCorner.Controllers.Claim.PDF", ex, context);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {

                if (svcNotes != null) svcNotes.Dispose();
                if (svcCompany != null) svcCompany.Dispose();

            }
            if (rsp.IsFailure || rsp.Claim == null)
            {
                SoundPower.Web.Notifications.AddResponseNotifications(rsp);
                return RedirectToAction("index", "claim");

            }


            Response.AddHeader("Content-Disposition",
           "attachment; filename=ClaimDetails." + extension);

            //Step 7 : Return file content result
            return new FileContentResult(renderedBytes, mimeType);
        }

        [HttpPost]
        public ActionResult Index(ClaimSearchRequest model, www.soundpower.biz.common.PersonField selClient)
        {
            if (selClient != null && !string.IsNullOrWhiteSpace(selClient.UniqueId))
            {
                model.Client = int.Parse(selClient.UniqueId);
            }
           if(model.ClaimId.HasValue)
            {
                model.Client = null;
                model.FromDays = null;
                model.GovernmentProgram = null;
                model.Insurance = null;
                model.Statuses = null;
                model.ToDays = null;
                
            }
            Company.ClientServiceClient svcClient = null;
            Company.ClaimServiceClient svcClaim = null;
            List<string> serviceNames = new List<string>();
            string insurances = "";
            string govts = "";
            ClaimInfoListResponse rspClients = null;
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
                svcClaim = new Company.ClaimServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                rspClients = svcClaim.Search(model);

            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.Claim.Index", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {
                if (svcClient != null) svcClient.Dispose();
                if (svcClaim != null) svcClaim.Dispose();

            }

            SoundPower.Web.Notifications.AddResponseNotifications(rspClients);
            if (rspClients.IsFailure) return RedirectToAction("index", "home");
            ViewBag.Insurances =  insurances;
            ViewBag.GovtPrograms =  govts;
            ViewBag.Results = rspClients.Claims;
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
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.Claim.Filter", ex);
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
        public ActionResult Index(string status,int? fromdays, int? todays, int? grouping, int? pendingId, string pendingType, int? client)
        {
            ClaimSearchRequest mdl = new ClaimSearchRequest();

            mdl.Statuses = status ?? slClaimsReqAttn;
            mdl.FromDays = fromdays;
            mdl.ToDays = todays;
            mdl.Grouping = grouping.GetValueOrDefault(1);
            if (pendingType== "InsurancePolicy")
            {
                mdl.Insurance = pendingId;
            }
            else if (pendingType== "GovtProgram")
            {
                mdl.GovernmentProgram = pendingId;
            }
            mdl.Client = client;
            if (mdl.Client.HasValue) mdl.Statuses = null;
            return Index(mdl,null);
        }

        [HttpGet]
        public ActionResult Filter()
        {
            ClaimSearchRequest mdl = new ClaimSearchRequest();

            mdl.Statuses = slClaimsReqAttn;
            mdl.Grouping = 1;



            return Filter(mdl);
        }


        [HttpGet]
        public ActionResult Details(long id, int? activeTab)
        {
          
            Company.ClaimServiceClient svcClaim = null;
            ViewBag.ActiveTab = activeTab;
               ClaimInfoResponse rspClients = null;
            try
            {
                var token = UserAuthorization.CurrentUser;

                svcClaim = new Company.ClaimServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                rspClients = svcClaim.Details(id);

            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.Claim.Details", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {
                if (svcClaim != null) svcClaim.Dispose();

            }

            SoundPower.Web.Notifications.AddResponseNotifications(rspClients);
       
            if (rspClients.IsFailure || rspClients.Claim==null)
            {
                return RedirectToAction("index");
            }

            return View(rspClients.Claim);
        }

        [HttpPost]
        public JsonResult QuickFilter(string ids, bool forInsurance)
        {
            JsonResult result = new JsonResult();
            ObjectIdResponseBase rsp = new ObjectIdResponseBase();
            if (!string.IsNullOrWhiteSpace(ids))
            {
                Company.ClaimServiceClient svcClient = null;

                try
                {
                    var token = UserAuthorization.CurrentUser;


                    svcClient = new Company.ClaimServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                    ClaimInfoListResponse  rspLst = forInsurance ? svcClient.SimpleList(new ObjectIdRequestBase() { ObjectId = ids }) : svcClient.SimpleCredentialList(new ObjectIdRequestBase() { ObjectId = ids });
                   
                    if (rspLst.Claims != null && rspLst.Claims.Count>0)
                    {
                        if (forInsurance)
                        {
                            rspLst.Claims.RemoveAll(c => c.Status != ClaimStatusEnum.PendInsSubmission);
                        }
                        else
                        {
                            rspLst.Claims.RemoveAll(c => c.Status != ClaimStatusEnum.PendGovtSubmission || string.IsNullOrWhiteSpace(c.ProviderNPI) || string.IsNullOrWhiteSpace(c.ProviderStateMedicaid));

                        }
                        
                        if (rspLst.Claims.Count>0)
                        {
                            rsp.ObjectId = string.Join(",", from c in rspLst.Claims select c.ClaimId.ToString());

                        }
                    }
                }
                catch (Exception ex)
                {
                    var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.Client.QuickFilter", ex);
                    SoundPower.Web.Utilities.ReportError(bex);
                    rsp.IsFailure = true;
                    rsp.ErrorMessages.Add(bex.Message);
                }
                finally
                {
                    if (svcClient != null) svcClient.Dispose();

                }

                SoundPower.Web.Notifications.AddResponseNotifications(rsp);


            }
            if (string.IsNullOrWhiteSpace(rsp.ObjectId))
            {
                rsp.IsFailure = true;
                rsp.ErrorMessages.Add( forInsurance ? ResourceText.ClaimsPages.NoClaimsPendingInsSubSelected : ResourceText.ClaimsPages.NoClaimsPendingGovtSubSelected);
            }
            result.Data = rsp;
            return result;
        }

        [HttpPost]
        [AntiForgeryHandleError]
        public ActionResult Close(long id)
        {

            Company.ClaimServiceClient svcClaim = null;

            ResponseBase rspClients = null;
            try
            {
                var token = UserAuthorization.CurrentUser;

                svcClaim = new Company.ClaimServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                rspClients = svcClaim.Close(id);

            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.Claim.Close", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {
                if (svcClaim != null) svcClaim.Dispose();

            }

            SoundPower.Web.Notifications.AddResponseNotifications(rspClients);

            if (!rspClients.IsFailure )
            {
                SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.ClaimsPages.ClaimClosed);
            }

            return RedirectToAction("details",new { id = id });
        }

        [HttpGet]
        public ActionResult Close()
        {
            SoundPower.Web.Notifications.AddErrorNotification(ResourceText.SharedPages.CouldNotProcessRelogin);
            return RedirectToAction("index");
        }

        public ActionResult HCFA(string id)
        {

            Company.ClaimServiceClient svcClaim = null;

            ObjectIdResponseBase rspClients = null;
            try
            {
                var token = UserAuthorization.CurrentUser;

                svcClaim = new Company.ClaimServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                rspClients = svcClaim.HCFA(new ObjectIdRequestBase() { ObjectId = id });

            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.Claim.HCFA", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {
                if (svcClaim != null) svcClaim.Dispose();

            }

            SoundPower.Web.Notifications.AddResponseNotifications(rspClients);

            if (!rspClients.IsFailure)
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    TextWriter tw = new StreamWriter(memoryStream);

                    tw.Write(rspClients.ObjectId);
                    tw.Flush();
                    tw.Close();

                    return File(memoryStream.GetBuffer(),"text/plain","hcfa.txt");
                        }

            }

            return RedirectToAction("index");
        }

        public ActionResult DDDHCFA(string id)
        {

            Company.ClaimServiceClient svcClaim = null;

            ObjectIdResponseBase rspClients = null;
            try
            {
                var token = UserAuthorization.CurrentUser;

                svcClaim = new Company.ClaimServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                rspClients = svcClaim.DDDHCFA(new ObjectIdRequestBase() { ObjectId = id });

            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.Claim.DDDHCFA", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {
                if (svcClaim != null) svcClaim.Dispose();

            }

            SoundPower.Web.Notifications.AddResponseNotifications(rspClients);

            if (!rspClients.IsFailure)
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    TextWriter tw = new StreamWriter(memoryStream);

                    tw.Write(rspClients.ObjectId);
                    tw.Flush();
                    tw.Close();

                    return File(memoryStream.GetBuffer(), "text/plain", "hcfa.txt");
                }

            }

            return RedirectToAction("index");
        }
        [HttpPost]
        [AntiForgeryHandleError]
        public ActionResult Advance(string id)
        {

            Company.ClaimServiceClient svcClaim = null;

            ResponseBase rspClients = null;
            try
            {
                var token = UserAuthorization.CurrentUser;

                svcClaim = new Company.ClaimServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                rspClients = svcClaim.Advance(new ObjectIdRequestBase() { ObjectId = id });

            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.Claim.Advance", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {
                if (svcClaim != null) svcClaim.Dispose();

            }

            SoundPower.Web.Notifications.AddResponseNotifications(rspClients);

            if (!rspClients.IsFailure)
            {
                SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.ClaimsPages.SubmissionConfirmed);

            }

            return RedirectToAction("index");
        }

        [HttpGet]
        public ActionResult Advance()
        {
            SoundPower.Web.Notifications.AddErrorNotification(ResourceText.SharedPages.CouldNotProcessRelogin);
            return RedirectToAction("index");
        }

        [OutputCache(NoStore = true, Duration = 0)]
        public ActionResult GovtSub(string id)
        {
  
            var context = new Dictionary<string, string>();
            context.Add("id", id);
           
            byte[] renderedBytes = null;
            string mimeType = null;
            string extension = null;

            Company.ClaimServiceClient svcClaim = null;
            Account.CompanyServiceClient svcCompany = null;
            Company.GovernmentProgramServiceClient svcProgram = null;

            ClaimInfoListResponse rspClients = null;
            try
            {
                var token = UserAuthorization.CurrentUser;

                svcClaim = new Company.ClaimServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                rspClients = svcClaim.List(new ObjectIdRequestBase() { ObjectId = id });
             


                if (!rspClients.IsFailure)
                {
                    svcProgram = new Company.GovernmentProgramServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                var rspProg = svcProgram.List(true);
                string gid = "";
                if (!rspProg.IsFailure && rspProg.Programs != null && rspProg.Programs.Count > 0) gid = rspProg.Programs[0].Code;

                    svcCompany = new Account.CompanyServiceClient(SiteUtilities.AccountService, token, SiteUtilities.CurrentCulture);
                    var rspCompany = svcCompany.Details(token.CurrentCompany);
                    if(rspCompany.IsFailure)
                    {
                        throw new Exception("Company Service Call Failed");
                    }

                    var exporter = new Company.Exports.ClaimListExports(rspClients.Claims,true,true, SiteUtilities.EnableTelehealth);
                    renderedBytes = Company.Exports.ExportUtilities.ToExcel(exporter.AZMedicaidClaimSubmission(rspCompany.Company.NPI,rspCompany.Company.TaxId,gid), out mimeType, out extension);
                }

            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.Claim.GovtSub", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {
                if (svcClaim != null) svcClaim.Dispose();
                if (svcProgram != null) svcProgram.Dispose();
                if (svcCompany != null) svcCompany.Dispose();

            }

            SoundPower.Web.Notifications.AddResponseNotifications(rspClients);

            if (!rspClients.IsFailure)
            {

                Response.AddHeader("Content-Disposition",
               "attachment; filename=GovtSubmission." + extension);

                //Step 7 : Return file content result
                return new FileContentResult(renderedBytes, mimeType);

            }

            return RedirectToAction("index");

       


        }

        [HttpGet]
        public ActionResult PayPrivate(int? id)
        {

            Company.ClaimServiceClient svcClaim = null;
            Company.ClientServiceClient svcClient = null;

            PaymentInfo mdl = new PaymentInfo()
            {
                Amount=0, 
                Type= PaymentTypeEnum.Private,
                Claims=new ClaimPaymentList(),
                PaymentId=-1,
                MadeOn=DateTime.Today,
                Description="",
                Notes=""
            };
            try
            {
                var token = UserAuthorization.CurrentUser;
                if (id.HasValue)
                {
                     svcClient = new Company.ClientServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                   var rspSearch = svcClient.Details(id.Value);
                    if (rspSearch.Client!=null )
                    {
                        ViewBag.ClientId = id.Value;
                        svcClaim = new Company.ClaimServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                        mdl.Description = rspSearch.Client.ToPerson().LastFirstMI;
                        var rspClaims = svcClaim.Search(new ClaimSearchRequest()
                        {
                            Statuses = "0,1,2,3,4",
                            Client = id
                        });
                        SoundPower.Web.Notifications.AddResponseNotifications(rspClaims);
                        PopulateClaimPayments(mdl, rspClaims,true);
                    }
                }


            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Load Processing Failure", "TherapyCorner.Portal.Controllers.Claim.PayPrivate", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {
                if (svcClaim != null) svcClaim.Dispose();
                if (svcClient != null) svcClient.Dispose();

            }

           

            return View(mdl);
        }

        private GenericEntity SetInsuranceList(int? id)
        {
            Company.InsuranceCompanyServiceClient svc = null;
            List<SelectListItem> lst = new List<SelectListItem>();
            GenericEntity result = null;
            try
            {
                var token = UserAuthorization.CurrentUser;
                           svc = new Company.InsuranceCompanyServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                var rsp = svc.List(true);
                if (rsp.Companies!=null && rsp.Companies.Count>0)
                {
                    rsp.Companies.Sort((a, b) => a.PayerId.CompareTo(b.PayerId));
                    foreach(var c in rsp.Companies)
                    {
                        if (!id.HasValue) id = c.CompanyId;
                        bool selected = false;
                        if (c.CompanyId==id.Value)
                        {
                            selected = true;
                            result = new GenericEntity(c.CompanyId.ToString(), "InsCompany", string.Format("{0} - {1}", c.PayerId, c.Name));
                        }
                        lst.Add(new SelectListItem() { Value = c.CompanyId.ToString(), Text = string.Format("{0} - {1}", c.PayerId, c.Name), Selected = selected });

                    }
                }
       


            }
            catch (Exception ex)
            {
                throw new SoundPower.ErrorTracking.BaseException("Load Processing Failure", "TherapyCorner.Portal.Controllers.Claim.SetInsuranceList", ex);

            }
            finally
            {
                if (svc != null) svc.Dispose();

            }
            ViewBag.Choices = lst;
            ViewBag.ChoiceId = id;
            return result;
        }

        private GenericEntity SetGovtList(int? id)
        {
            Company.GovernmentProgramServiceClient svc = null;
            List<SelectListItem> lst = new List<SelectListItem>();
            GenericEntity result = null;
            try
            {
                var token = UserAuthorization.CurrentUser;
                svc = new Company.GovernmentProgramServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                var rsp = svc.List(true);
                if (rsp.Programs != null && rsp.Programs.Count > 0)
                {
                    rsp.Programs.Sort((a, b) => a.Name.CompareTo(b.Name));
                    foreach (var c in rsp.Programs)
                    {
                        if (!id.HasValue) id = c.ProgramId;
                        bool selected = false;
                        if (c.ProgramId == id.Value)
                        {
                            selected = true;
                            result = new GenericEntity(c.ProgramId.ToString(), "GovtProgram", c.Name);
                        }
                        lst.Add(new SelectListItem() { Value = c.ProgramId.ToString(), Text = c.Name, Selected = selected });

                    }
                }



            }
            catch (Exception ex)
            {
                throw new SoundPower.ErrorTracking.BaseException("Load Processing Failure", "TherapyCorner.Portal.Controllers.Claim.SetInsuranceList", ex);

            }
            finally
            {
                if (svc != null) svc.Dispose();

            }
            ViewBag.Choices = lst;
            ViewBag.ChoiceId = id;
            return result;
        }
        [HttpGet]
        public ActionResult PayInsurance(int? id)
        {

            Company.ClaimServiceClient svcClaim = null;

            PaymentInfo mdl = new PaymentInfo()
            {
                Amount = 0,
                Type = PaymentTypeEnum.Insurance,
                Claims = new ClaimPaymentList(),
                PaymentId = -1,
                MadeOn = DateTime.Today,
                Description = "",
                Notes = ""
            };
            try
            {
                var token = UserAuthorization.CurrentUser;
                var comp = SetInsuranceList(id);
                if (comp==null)
                {
                    SoundPower.Web.Notifications.AddErrorNotification(ResourceText.ClaimsPages.NoActiveInsurance);
                    return RedirectToAction("index");
                }
                mdl.Description = comp.Name;

                  
                        svcClaim = new Company.ClaimServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                        var rspClaims = svcClaim.Search(new ClaimSearchRequest()
                        {
                            Statuses = "1",
                            Insurance = int.Parse(comp.UniqueId)
                        });
                        SoundPower.Web.Notifications.AddResponseNotifications(rspClaims);
                        PopulateClaimPayments(mdl, rspClaims, false);
                    
                


            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Load Processing Failure", "TherapyCorner.Portal.Controllers.Claim.PayInsurance", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {
                if (svcClaim != null) svcClaim.Dispose();

            }



            return View(mdl);
        }
        [HttpGet]
        public ActionResult Paygovt(int? id)
        {

            Company.ClaimServiceClient svcClaim = null;

            PaymentInfo mdl = new PaymentInfo()
            {
                Amount = 0,
                Type = PaymentTypeEnum.Govt,
                Claims = new ClaimPaymentList(),
                PaymentId = -1,
                MadeOn = DateTime.Today,
                Description = "",
                Notes = ""
            };
            try
            {
                var token = UserAuthorization.CurrentUser;
                var comp = SetGovtList(id);
                if (comp == null)
                {
                    SoundPower.Web.Notifications.AddErrorNotification(ResourceText.ClaimsPages.NoActiveGovtPrograms);
                    return RedirectToAction("index");
                }
                mdl.Description = comp.Name;


                svcClaim = new Company.ClaimServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                var rspClaims = svcClaim.Search(new ClaimSearchRequest()
                {
                    Statuses = "3",
                    GovernmentProgram = int.Parse(comp.UniqueId)
                });
                SoundPower.Web.Notifications.AddResponseNotifications(rspClaims);
                PopulateClaimPayments(mdl, rspClaims, false);




            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Load Processing Failure", "TherapyCorner.Portal.Controllers.Claim.PayGovt", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {
                if (svcClaim != null) svcClaim.Dispose();

            }



            return View(mdl);
        }

        private static void PopulateClaimPayments(PaymentInfo mdl, ClaimInfoListResponse rspClaims, bool forPrivate)
        {
            if (rspClaims.Claims != null && rspClaims.Claims.Count > 0)
            {
                rspClaims.Claims.Sort((a, b) => a.ClaimDate.CompareTo(b.ClaimDate));
                foreach (var c in rspClaims.Claims)
                {

                    var cp = new ClaimPayment()
                    {
                        Claim = new GenericEntity(c.ClaimId.ToString(), "Claim",c.Provider.Name) ,
                        Client = c.Client,
                        ClaimDate = c.ClaimDate,
                        PaymentId = -1,
                        AmountDue = c.AmountDue - c.AmountPaid,
                        Source = forPrivate ? c.Client : c.PendingWith,
                        DenialReason = new GenericEntity("N/A","DenialReason",null)

                    };
                    mdl.Claims.Add(cp);
                }
            }
        }

        [HttpPost]
        public ActionResult PayPrivate(PaymentInfo payment, int clientId)
        {

            Company.ClaimServiceClient svcClaim = null;
            ViewBag.ClientId = clientId;

            if (payment.Amount==0 )
            {
                SoundPower.Web.Notifications.AddErrorNotification(ResourceText.ClaimsPages.NoClaimsProcessed);
                return View(payment);
            }
            if (ModelState.IsValid)
            {
                ResponseBase rsp = null;

                try
                {
                    var token = UserAuthorization.CurrentUser;
                    payment.Claims.RemoveAll(c => c.Amount.GetValueOrDefault(0) == 0);
                    payment.ProcessedBy = new GenericEntity(token.User.UniqueId, "User", token.User.FirstMiddleLast);
                   
                            svcClaim = new Company.ClaimServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                    rsp = svcClaim.Pay(payment);
                            SoundPower.Web.Notifications.AddResponseNotifications(rsp);
        


                }
                catch (Exception ex)
                {
                    var bex = new SoundPower.ErrorTracking.BaseException("Save Processing Failure", "TherapyCorner.Portal.Controllers.Claim.PayPrivate", ex);
                    SoundPower.Web.Utilities.ReportError(bex);
                    throw bex;
                }
                finally
                {
                    if (svcClaim != null) svcClaim.Dispose();

                }
                if (!rsp.IsFailure)
                {
                    SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.ClaimsPages.PaymentRecorded);
                    return RedirectToAction("payprivate", new { id = clientId });
                }
            }
            return View(payment);



        }


        [HttpPost]
        public ActionResult PayInsurance(PaymentInfo payment, int choiceId)
        {

            Company.ClaimServiceClient svcClaim = null;
            ViewBag.ChoiceId = choiceId;
            foreach (var c in payment.Claims)
            {
                if (c.DenialReason != null && c.DenialReason.UniqueId == "N/A")
                {
                    c.DenialReason = null;
                }
                else
                {
                    c.Denial = true;
                }
            }
            if (payment.Claims.Count(c => c.Amount.HasValue || c.Denial|| !string.IsNullOrWhiteSpace(c.Comment)) ==0)
            {
                SoundPower.Web.Notifications.AddErrorNotification(ResourceText.ClaimsPages.NoClaimsProcessed);
                
            }
            else if (ModelState.IsValid)
            {
                ResponseBase rsp = null;

                payment.Claims.RemoveAll(c => !c.Amount.HasValue && !c.Denial && string.IsNullOrWhiteSpace(c.Comment));
               
                try
                {
                    var token = UserAuthorization.CurrentUser;
                    payment.ProcessedBy = new GenericEntity(token.User.UniqueId, "User", token.User.FirstMiddleLast);

                    svcClaim = new Company.ClaimServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                    rsp = svcClaim.Pay(payment);
                    SoundPower.Web.Notifications.AddResponseNotifications(rsp);



                }
                catch (Exception ex)
                {
                    var bex = new SoundPower.ErrorTracking.BaseException("Save Processing Failure", "TherapyCorner.Portal.Controllers.Claim.PayInsurance", ex);
                    SoundPower.Web.Utilities.ReportError(bex);
                    throw bex;
                }
                finally
                {
                    if (svcClaim != null) svcClaim.Dispose();

                }
                if (!rsp.IsFailure)
                {
                    SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.ClaimsPages.PaymentRecorded);
                    return RedirectToAction("payinsurance", new { id = choiceId });
                }
            }

            try
            {
                SetInsuranceList(choiceId);



            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Save Failure Display Processing Failure", "TherapyCorner.Portal.Controllers.Claim.PayInsurance", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
          

            return View(payment);



        }


        [HttpPost]
        public ActionResult PayGovt(PaymentInfo payment, int choiceId)
        {

            Company.ClaimServiceClient svcClaim = null;
            ViewBag.ChoiceId = choiceId;

            if (payment.Claims.Count(c => c.Amount.HasValue || c.Denial || !string.IsNullOrWhiteSpace(c.Comment)) == 0)
            {
                SoundPower.Web.Notifications.AddErrorNotification(ResourceText.ClaimsPages.NoClaimsProcessed);

            }
            else if (ModelState.IsValid)
            {
                ResponseBase rsp = null;
                payment.Claims.RemoveAll(c =>c.Amount.HasValue && c.Amount.GetValueOrDefault(0)==0);
                payment.Claims.RemoveAll(c => !c.Amount.HasValue && !c.Denial && string.IsNullOrWhiteSpace(c.Comment));

                try
                {
                    var token = UserAuthorization.CurrentUser;
                    payment.ProcessedBy = new GenericEntity(token.User.UniqueId, "User", token.User.FirstMiddleLast);

                    svcClaim = new Company.ClaimServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                    rsp = svcClaim.Pay(payment);
                    SoundPower.Web.Notifications.AddResponseNotifications(rsp);



                }
                catch (Exception ex)
                {
                    var bex = new SoundPower.ErrorTracking.BaseException("Save Processing Failure", "TherapyCorner.Portal.Controllers.Claim.PayGovt", ex);
                    SoundPower.Web.Utilities.ReportError(bex);
                    throw bex;
                }
                finally
                {
                    if (svcClaim != null) svcClaim.Dispose();

                }
                if (!rsp.IsFailure)
                {
                    SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.ClaimsPages.PaymentRecorded);
                    return RedirectToAction("paygovt", new { id = choiceId });
                }
            }

            try
            {
                SetGovtList(choiceId);



            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Save Failure Display Processing Failure", "TherapyCorner.Portal.Controllers.Claim.PayGovt", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }


            return View(payment);



        }

        [HttpGet]
        public ActionResult ReconcileGovt()
        {
            return View();
        }

        [HttpPost]
        [AntiForgeryHandleError]
        public ActionResult ReconcileGovtResults(HttpPostedFileBase file)
        {
            bool fileGood = true;
            if (file == null || file.ContentLength==0 )
            {
                SoundPower.Web.Notifications.AddErrorNotification(ResourceText.ClaimsPages.NoFile);
                fileGood = false;
            }
            if (ModelState.IsValid && fileGood)
            {


                Company.ClaimServiceClient svc = null;
                ReconcileResponse rsp = null;
                ReconcileRequest req = new ReconcileRequest() {  Source= "Arizona DDD" };
                if (file != null && file.ContentLength > 0)
                {

                    req.FileData = new byte[file.InputStream.Length];
                    file.InputStream.Read(req.FileData, 0, req.FileData.Length);
                  
                }
                try
                {
                    var token = UserAuthorization.CurrentUser;
                    svc = new Company.ClaimServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                    rsp = svc.GovtReconcile(req);

                }
                catch (Exception ex)
                {
                    var bex = new SoundPower.ErrorTracking.BaseException("Save Failure", "TherapyCorner.Controllers.Claim.ReconcileGovtResults", ex);
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

                    return View(rsp);
                }
               

            }
            return RedirectToAction("ReconcileGovt");
        }

        [HttpGet]
        public ActionResult ReconcileGovtResults()
        {
            SoundPower.Web.Notifications.AddErrorNotification(ResourceText.SharedPages.CouldNotProcessRelogin);
            return RedirectToAction("index");
        }

        [HttpPost]
        [RequireBillerOrWorker]
        public JsonResult StatusCounts(string statuses)
        {
            Company.ClaimServiceClient svcClient = null;
            var rsp = new ObjectIdResponseBase(); ;
            JsonResult result = new JsonResult();

            try
            {
                var token = UserAuthorization.CurrentUser;


                svcClient = new Company.ClaimServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
               var rspSrch = svcClient.Search(new ClaimSearchRequest() { Statuses = statuses });
                rsp.ErrorMessages = rspSrch.ErrorMessages;
                rsp.WarningMessages = rspSrch.WarningMessages;
                rsp.IsFailure = rspSrch.IsFailure;
                rsp.ObjectId = rsp.IsFailure ? "-" : rspSrch.Claims.Count.ToString();
            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Claim Counter Processing Failure", "TherapyCorner.Portal.Controllers.Claim.StatusCounts", ex);
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
        [RequireBiller]
        public JsonResult DenialReasons()
        {
            Company.ClaimServiceClient svcClient = null;
            var rsp = new GenericEntityListResponse(); ;
            JsonResult result = new JsonResult();

            try
            {
                var token = UserAuthorization.CurrentUser;


                svcClient = new Company.ClaimServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                rsp = svcClient.DenialReasons();
                
            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Claim Counter Processing Failure", "TherapyCorner.Portal.Controllers.Claim.DenialReasons", ex);
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


        //[HttpPost]
        //[AntiForgeryHandleError]
        public ActionResult StartOver(long id)
        {

            Company.ClaimServiceClient svcClaim = null;

            ResponseBase rspClients = null;
            try
            {
                var token = UserAuthorization.CurrentUser;

                svcClaim = new Company.ClaimServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                rspClients = svcClaim.StartOver(id);

            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.Claim.StartOver", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {
                if (svcClaim != null) svcClaim.Dispose();

            }

            SoundPower.Web.Notifications.AddResponseNotifications(rspClients);

            if (!rspClients.IsFailure)
            {
                SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.ClaimsPages.ClaimReset);
            }

            return RedirectToAction("details", new { id = id });
        }

        public ActionResult Vet(long id)
        {

            Company.ClaimServiceClient svcClaim = null;

            ResponseBase rspClients = null;
            try
            {
                var token = UserAuthorization.CurrentUser;

                svcClaim = new Company.ClaimServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                rspClients = svcClaim.StartOver(id);

            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.Claim.Vet", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {
                if (svcClaim != null) svcClaim.Dispose();

            }

            SoundPower.Web.Notifications.AddResponseNotifications(rspClients);

            if (!rspClients.IsFailure)
            {
                SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.ClaimsPages.Vetted);
            }

            return RedirectToAction("index", new { status = 9 });
        }

        public ActionResult NonTelehealth(long id)
        {

            Company.ClaimServiceClient svcClaim = null;

            ResponseBase rspClients = null;
            try
            {
                var token = UserAuthorization.CurrentUser;

                svcClaim = new Company.ClaimServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                svcClaim.ReinstateServiceCPT(id);
                rspClients = svcClaim.StartOver(id);

            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.Claim.VetException", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {
                if (svcClaim != null) svcClaim.Dispose();

            }

            SoundPower.Web.Notifications.AddResponseNotifications(rspClients);

            if (!rspClients.IsFailure)
            {
                SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.ClaimsPages.Vetted);
            }

            return RedirectToAction("index", new { status = 9 });
        }
    }
}