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
    [RequireBillerOrWorker]
    [OutputCache(NoStore =true, Duration =0)]
        public class InsurancePolicyController : Controller
    {
        // GET: InsurancePolicy
        public ActionResult Create(int? id)
        {
            if (!id.HasValue)
            {

                SoundPower.Web.Notifications.AddErrorNotification(ResourceText.SharedPages.CouldNotProcessRelogin);
                return RedirectToAction("index", "client");

            }
            PersonField client = null;
            try
            {
                client = GetLists(id.Value);
            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.InsurancePolicy.Create", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            return View(new InsurancePolicy()
            {
                Client = client,
                PolicyId = -1,
                Company =   new GenericEntity("?", "InsuranceCompany", null),
                Start = DateTime.Today,
                Version = "NEW"
            });

        }

        [HttpPost]
        [AntiForgeryHandleError]
        public ActionResult Create(InsurancePolicy model)
        {
  
            if (ModelState.IsValid )
            {
                Company.InsurancePolicyServiceClient svc = null;


                ResponseBase rsp = null;
           
                try
                {
                    var token = UserAuthorization.CurrentUser;
                    svc = new Company.InsurancePolicyServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                    rsp = svc.Create(model);

                }
                catch (Exception ex)
                {
                    var bex = new SoundPower.ErrorTracking.BaseException("Save Failure", "TherapyCorner.Controllers.InsurancePolicy.Create", ex);
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

                    SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.ClientPages.PolicyCreated);
                    return RedirectToAction("details", "client", new { id = model.Client.UniqueId });
                }
                else
                {
                    SiteUtilities.ApplyFieldIssues(rsp, ModelState);

                }

            }
            try
            {
                GetLists(int.Parse(model.Client.UniqueId));
            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Return Processing Failure", "TherapyCorner.Portal.Controllers.InsurancePolicy.Create", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            return View(model);
        }

        [HttpPost]
        [AntiForgeryHandleError]
        public ActionResult Update( InsurancePolicy model)
        {
         
            if (ModelState.IsValid )
            {


                Company.InsurancePolicyServiceClient svc = null;
                ResponseBase rsp = null;
              
                try
                {
                    var token = UserAuthorization.CurrentUser;
                    svc = new Company.InsurancePolicyServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                    rsp = svc.Update(model);

                }
                catch (Exception ex)
                {
                    var bex = new SoundPower.ErrorTracking.BaseException("Save Failure", "TherapyCorner.Controllers.InsurancePolicy.Update", ex);
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

                    SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.ClientPages.PolicyUpdated);
                    return RedirectToAction("details", "client", new { id = model.Client.UniqueId });
                }
                else
                {
                    SiteUtilities.ApplyFieldIssues(rsp, ModelState);

                }

            }

            try
            {
                GetLists(int.Parse(model.Client.UniqueId));
            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Return Processing Failure", "TherapyCorner.Portal.Controllers.InsurancePolicy.Update", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            return View(model);
        }


        [HttpGet]
        public ActionResult Update(int? id)
        {
            if (!id.HasValue)
            {

                SoundPower.Web.Notifications.AddErrorNotification(ResourceText.SharedPages.CouldNotProcessRelogin);
                return RedirectToAction("index", "client");

            }


            Company.InsurancePolicyServiceClient svc = null;
            InsurancePolicyReponse rsp = null;

            try
            {
                var token = UserAuthorization.CurrentUser;
                svc = new Company.InsurancePolicyServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                rsp = svc.Details(id.Value);
                GetLists(int.Parse(rsp.Policy.Client.UniqueId));

            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Controllers.InsurancePolicy.Update", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {
                if (svc != null) svc.Dispose();
            }


            SoundPower.Web.Notifications.AddResponseNotifications(rsp);



            return View(rsp.Policy);
        }

        [HttpPost]
        public ActionResult RequestEligibility(int id)
        {
          

            Company.InsurancePolicyServiceClient svc = null;
            ResponseBase rsp = null;

            try
            {
                var token = UserAuthorization.CurrentUser;
                svc = new Company.InsurancePolicyServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                rsp = svc.RequestEligibility(id);
                if (!rsp.IsFailure)
                {
                    SoundPower.Web.Notifications.AddSuccessMessages("Eligibility request was successfully made.");
                }
            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Controllers.InsurancePolicy.RequestEligibility", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {
                if (svc != null) svc.Dispose();
            }


            SoundPower.Web.Notifications.AddResponseNotifications(rsp);



            return RedirectToAction("update",new { id = id });
        }

        [HttpGet]
        [OutputCache(NoStore = true, Duration = 0)]
        public ActionResult Eligibility(int id)
        {


            Company.InsurancePolicyServiceClient svc = null;
            EligibilityResponse rsp = null;
            byte[] renderedBytes = null;
            string mimeType = null;
            string extension = null;

            try
            {
                var token = UserAuthorization.CurrentUser;
                svc = new Company.InsurancePolicyServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                rsp = svc.Eligibility(id);
            SoundPower.Web.Notifications.AddResponseNotifications(rsp);
                if (rsp.IsFailure)
                {
                    SoundPower.Web.Notifications.AddSuccessMessages("Eligibility request was successfully made.");
              return RedirectToAction("index", "client");
              }
                Company.Exports.EligibilityExport exporter = null;
                var companies = StaticData.InsuranceCompanies(token.ClearingHouse.GetValueOrDefault(1));
                var company = companies.Find(i => i.CompanyId.ToString() == rsp.Policy.Company.UniqueId);
                rsp.Policy.Company.Name = company.Name;
                if (rsp.Response.Contains("ST*271*") && rsp.Response.Contains("ISA*"))
                {
                    exporter = new Company.Exports.EligibilityExport(rsp.Response, rsp.AsOf, rsp.Policy,StaticData.InsuranceTypes,StaticData.InsuranceServiceTypes);

                }
                else
                {
                    var t = Newtonsoft.Json.JsonConvert.DeserializeObject<Company.Exports.Pokitdok.EligibilityResponse>(rsp.Response );
                    exporter = new Company.Exports.EligibilityExport(t, rsp.AsOf, rsp.Policy);
                }
                



                
                renderedBytes = Company.Exports.ExportUtilities.ToPDF(exporter.GenerateReport(), out mimeType, out extension);

            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Controllers.InsurancePolicy.Eligibility", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {
                if (svc != null) svc.Dispose();
            }


            Response.AddHeader("Content-Disposition", string.Format("attachment; filename=EligibilityReport{0}.{1}",id,  extension));

            //Step 7 : Return file content result
            return new FileContentResult(renderedBytes, mimeType);


        }

    

        [HttpPost]
        [AntiForgeryHandleError]
        public ActionResult Delete(int id)
        {



            Company.InsurancePolicyServiceClient svc = null;
            InsurancePolicyReponse rsp = null;
            ResponseBase rspAction = null;
            try
            {
                var token = UserAuthorization.CurrentUser;
                svc = new Company.InsurancePolicyServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                rsp = svc.Details(id);

                rspAction = svc.Remove(id);
            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Controllers.InsurancePolicy.Delete", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {
                if (svc != null) svc.Dispose();
            }


            SoundPower.Web.Notifications.AddResponseNotifications(rspAction);
            if (rspAction.IsFailure)
            {
                return RedirectToAction("update", new { id = id });
            }
            else
            {
                SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.ClientPages.PolicyRemoved);
                return RedirectToAction("details", "client", new { id = rsp.Policy.Client.UniqueId });
            }


        }

        [HttpGet]
        public ActionResult Delete()
        {
            SoundPower.Web.Notifications.AddErrorNotification(ResourceText.SharedPages.CouldNotProcessRelogin);
            return RedirectToAction("index", "client");
        }

        [HttpPost]
        [AntiForgeryHandleError]
        public ActionResult DeleteWaiver(int id,int waiverId)
        {



            Company.InsurancePolicyServiceClient svc = null;
            ResponseBase rspAction = null;
            try
            {
                var token = UserAuthorization.CurrentUser;
                svc = new Company.InsurancePolicyServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);

                rspAction = svc.RemoveWaiver(waiverId,id);
            }
            catch (Exception ex)
            {
                Dictionary<string, string> ctx = new Dictionary<string, string>();
                ctx.Add("Policy", id.ToString());
                ctx.Add("Waiver", waiverId.ToString());
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Controllers.InsurancePolicy.DeleteWaiver", ex,ctx);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {
                if (svc != null) svc.Dispose();
            }


            SoundPower.Web.Notifications.AddResponseNotifications(rspAction);
            if (!rspAction.IsFailure)
            {
       
                SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.ClientPages.WaiverRemoved);
            }
            return RedirectToAction("update", new { id = id });

        }

        [HttpGet]
        public ActionResult DeleteWaiver()
        {
            SoundPower.Web.Notifications.AddErrorNotification(ResourceText.SharedPages.CouldNotProcessRelogin);
            return RedirectToAction("index", "client");
        }

        [HttpPost]
        [AntiForgeryHandleError]
        public ActionResult AddWaiver(int id, int service)
        {



            Company.InsurancePolicyServiceClient svc = null;
            ResponseBase rspAction = null;
            try
            {
                var token = UserAuthorization.CurrentUser;
                svc = new Company.InsurancePolicyServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);

                rspAction = svc.CreateWaiver(new WaiverInfo() { PolicyId = id, Version = "NEW", WaiverId = -1, Service = new GenericEntity(service.ToString(), "ClientService", null) });
            }
            catch (Exception ex)
            {
                Dictionary<string, string> ctx = new Dictionary<string, string>();
                ctx.Add("Policy", id.ToString());
                ctx.Add("Service", service.ToString());
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Controllers.InsurancePolicy.AddWaiver", ex, ctx);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {
                if (svc != null) svc.Dispose();
            }


            SoundPower.Web.Notifications.AddResponseNotifications(rspAction);
            if (rspAction.IsFailure)
            {
                if (rspAction.FieldIssues!=null && rspAction.FieldIssues.Count>0)
                {
                    foreach(var i in rspAction.FieldIssues)
                    {
                        foreach(var f in i.Fields)
                        {
                            SoundPower.Web.Notifications.AddErrorNotification(string.Format("{0} {1}", f, i));
                        }
                    }
                }
            }
                    else

            {

                SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.ClientPages.WaiverAdded);
            }
            return RedirectToAction("update", new { id = id });

        }

        [HttpPost]
        [AntiForgeryHandleError]
        public ActionResult ConfirmWaiver(int id, DateTime waiverStart, DateTime waiverEnd, int wId)
        {



            Company.InsurancePolicyServiceClient svc = null;
            ResponseBase rspAction = null;
            try
            {
                var token = UserAuthorization.CurrentUser;
                svc = new Company.InsurancePolicyServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);

                rspAction = svc.ConfirmWaiver(new WaiverInfo() { End = waiverEnd, Start = waiverStart, PolicyId = id, Version = "NEW", WaiverId = wId, Service = new GenericEntity("??", "ClientService", null) });
            }
            catch (Exception ex)
            {
                Dictionary<string, string> ctx = new Dictionary<string, string>();
                ctx.Add("Policy", id.ToString());
                ctx.Add("Waiver", wId.ToString());
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Controllers.InsurancePolicy.ConfirmWaiver", ex, ctx);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {
                if (svc != null) svc.Dispose();
            }


            SoundPower.Web.Notifications.AddResponseNotifications(rspAction);
            if (rspAction.IsFailure)
            {
                if (rspAction.FieldIssues != null && rspAction.FieldIssues.Count > 0)
                {
                    foreach (var i in rspAction.FieldIssues)
                    {
                        foreach (var f in i.Fields)
                        {
                            SoundPower.Web.Notifications.AddErrorNotification(string.Format("{0} {1}", f, i));
                        }
                    }
                }
            }
            else

            {

                SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.ClientPages.WaiverConfirmed);
            }
            return RedirectToAction("update", new { id = id });

        }


        [HttpGet]
        public ActionResult AddWaiver()
        {
            SoundPower.Web.Notifications.AddErrorNotification(ResourceText.SharedPages.CouldNotProcessRelogin);
            return RedirectToAction("index", "client");
        }

        [HttpGet]
        public ActionResult ConfirmWaiver()
        {
            SoundPower.Web.Notifications.AddErrorNotification(ResourceText.SharedPages.CouldNotProcessRelogin);
            return RedirectToAction("index", "client");
        }

        private PersonField GetLists(int id)
        {
            Company.ClientServiceClient svcClient = null;
            Company.InsuranceCompanyServiceClient svc = null;
            ClientInfoResponse rspClient = null;
            PersonField result = null;
            try
            {
                var token = UserAuthorization.CurrentUser;


                svcClient = new Company.ClientServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                rspClient = svcClient.Details(id);
               
                result = rspClient.Client.ToPerson();
                ViewBag.HasGovt = rspClient.Client.GovtProgram != null;
                ViewBag.Services = rspClient.Client.Services;
                    svc = new Company.InsuranceCompanyServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                    var rsp = svc.List(true);
                    if(rsp.Companies!=null && rsp.Companies.Count>0)
                    {
                        var list = StaticData.AllInsuranceCompanies;
                        foreach(var c in rsp.Companies)
                        {
                            var d = list.Find(i => i.CompanyId == c.CompanyId);
                        if (d != null)
                        {
                            c.Name = d.Name;
                            c.Version = "";
                            if(rspClient.Client.GovtProgram !=null)
                            {
                               var cprog= d.GovernmentPrograms.Find(p => p.UniqueId == rspClient.Client.GovtProgram.UniqueId);
                              if (cprog!=null)  c.Version = cprog.AlternateId;
                            }
                        }
                        }
                        rsp.Companies.Sort((a, b) => a.Name.CompareTo(b.Name));
                    }
                    ViewBag.Companies = rsp.Companies;
                

            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.InsurancePolicy.GetLists", ex);
                throw bex;
            }
            finally
            {
                if (svcClient != null) svcClient.Dispose();

            }
            return result;

        }
    }
}