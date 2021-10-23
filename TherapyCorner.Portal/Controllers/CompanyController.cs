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
    [CompanyFilter]
    [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]

    public class CompanyController : Controller
    {
        // GET: Company
        [RequireAdmin]
        public ActionResult Index()
        {
            Account.CompanyServiceClient svc = null;
            CompanyInfoResponse rsp = null;
            Company.GovernmentProgramServiceClient svcComp = null;
            try
            {
                var token = UserAuthorization.CurrentUser;
                svc = new Account.CompanyServiceClient(SiteUtilities.AccountService, token, SiteUtilities.CurrentCulture);
                rsp = svc.Details(token.CurrentCompany);
                svcComp = new Company.GovernmentProgramServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                var rspGovt = svcComp.List(true);
                SoundPower.Web.Notifications.AddResponseNotifications(rspGovt);
                if(!rspGovt.IsFailure )
                {
                    if (rspGovt.Programs!=null && rspGovt.Programs.Count>0)
                    {
                        var programs = StaticData.GovernmentPrograms;
                        foreach(var p in rspGovt.Programs)
                        {
                            var d = programs.Find(info => info.ProgramId == p.ProgramId);
                            if (d != null) p.Name = d.Name;
                        }
                    }
                    ViewBag.Programs = rspGovt.Programs;
                }

            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.Company.Index", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {
                if (svc != null) svc.Dispose();
                if (svcComp != null) svcComp.Dispose();
            }


            SoundPower.Web.Notifications.AddResponseNotifications(rsp);
            if (rsp.IsFailure)
            {
                return RedirectToAction("index", "home");

            }
          
            return View(rsp.Company);
        }

        [RequireAdmin]
        [HttpGet]
        public ActionResult Edit()
        {
            Account.CompanyServiceClient svc = null;
            CompanyInfoResponse rsp = null;
            try
            {
                var token = UserAuthorization.CurrentUser;
                svc = new Account.CompanyServiceClient(SiteUtilities.AccountService, token, SiteUtilities.CurrentCulture);
                rsp = svc.Details(token.CurrentCompany);


            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.Company.Edit", ex);
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

            return View(rsp.Company);
        }

        [RequireAdmin]
        [HttpPost]
        public ActionResult Edit(CompanyInfo model)
        {
            if (ModelState.IsValid)
            {
                Account.CompanyServiceClient svc = null;
                ResponseBase rsp = null;
                try
                {
                    var token = UserAuthorization.CurrentUser;
                    svc = new Account.CompanyServiceClient(SiteUtilities.AccountService, token, SiteUtilities.CurrentCulture);
                    rsp = svc.Update(model);


                }
                catch (Exception ex)
                {
                    var bex = new SoundPower.ErrorTracking.BaseException("Save Failure", "TherapyCorner.Portal.Controllers.Company.Edit", ex);
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

                    SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.CompanyPages.UpdateSuccess);
                    return RedirectToAction("index");
                }
                else
                {
                    SiteUtilities.ApplyFieldIssues(rsp, ModelState);

                }
            }
            return View(model);
        }

        [RequireBiller]
        public ActionResult GovernmentProgram()
        {
            www.therapycorner.com.company.MessageContracts.ReferencedGovernmentProgramListResponse rspGovt = null;
            Company.GovernmentProgramServiceClient svcComp = null;
            try
            {
                var token = UserAuthorization.CurrentUser;

                svcComp = new Company.GovernmentProgramServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                 rspGovt = svcComp.List(false);
                SoundPower.Web.Notifications.AddResponseNotifications(rspGovt);
                if (rspGovt.Programs == null) rspGovt.Programs = new www.therapycorner.com.company.ReferencedGovernmentProgramList();
                var programs = StaticData.GovernmentPrograms;
                foreach(var d in programs)
                {
                    var existing = rspGovt.Programs.Find(i => i.ProgramId == d.ProgramId);
                    if (existing!=null)
                    {
                        existing.Name = d.Name;
                    }
                    else
                    {
                        rspGovt.Programs.Add(new www.therapycorner.com.company.ReferencedGovernmentProgram() { ProgramId = (int)d.ProgramId, IsActive = false, Name = d.Name, ClientCount = 0 });
                    }
                }
        

            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.Company.GovernmentProgram", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {
                if (svcComp != null) svcComp.Dispose();
            }


            if (rspGovt.IsFailure)
            {
                return RedirectToAction("index", "home");

            }
            rspGovt.Programs.Sort((a, b) => a.Name.CompareTo(b.Name));
            return View(rspGovt.Programs);
        }


        [RequireBiller]
        [HttpGet]
        public ActionResult GovernmentProgramDetails(int id)
        {
            www.therapycorner.com.company.MessageContracts.ReferencedGovernmentProgramListResponse rspGovt = null;
            Company.GovernmentProgramServiceClient svcComp = null;
            ReferencedGovernmentProgram program = null;
            try
            {
                var token = UserAuthorization.CurrentUser;

                svcComp = new Company.GovernmentProgramServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                rspGovt = svcComp.List(false);
                SoundPower.Web.Notifications.AddResponseNotifications(rspGovt);
                if (rspGovt.Programs == null) RedirectToAction("governmentprogram");
                var programs = StaticData.GovernmentPrograms;

                program = rspGovt.Programs.Find(p => p.ProgramId == id);
                if (program == null) RedirectToAction("governmentprogram");
                var d = programs.Find(p => p.ProgramId == program.ProgramId);
                program.Name = d.Name;

            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.Company.GovernmentProgramDetails", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {
                if (svcComp != null) svcComp.Dispose();
            }



            return View(program);
        }


        [RequireBiller]
        [HttpPost]
        [AntiForgeryHandleError]
        public ActionResult GovernmentProgramDetails(ReferencedGovernmentProgram request)
        {
            ResponseBase rspGovt = null;
            Company.GovernmentProgramServiceClient svcComp = null;
            if (ModelState.IsValid)
            {
                try
                {
                    var token = UserAuthorization.CurrentUser;

                    svcComp = new Company.GovernmentProgramServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                    rspGovt = svcComp.Update(request);
                    SoundPower.Web.Notifications.AddResponseNotifications(rspGovt);
                    if (!rspGovt.IsFailure)
                    {
                        SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.CompanyPages.ProgramAdded);
                        return RedirectToAction("governmentprogram");

                    }


                }
                catch (Exception ex)
                {
                    var bex = new SoundPower.ErrorTracking.BaseException("Save Failure", "TherapyCorner.Portal.Controllers.Company.GovernmentProgramDetails", ex);
                    SoundPower.Web.Utilities.ReportError(bex);
                    throw bex;
                }
                finally
                {
                    if (svcComp != null) svcComp.Dispose();
                }

            }

            return View(request);
        }

        [RequireBiller]
        [HttpPost]
        [AntiForgeryHandleError]
        public ActionResult RemoveGovt(int programId)
        {
            ResponseBase rspGovt = null;
            Company.GovernmentProgramServiceClient svcComp = null;
            try
            {
                var token = UserAuthorization.CurrentUser;

                svcComp = new Company.GovernmentProgramServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                rspGovt = svcComp.Remove(programId);
                SoundPower.Web.Notifications.AddResponseNotifications(rspGovt);
              


            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.Company.RemoveGovt", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {
                if (svcComp != null) svcComp.Dispose();
            }


            if (!rspGovt.IsFailure)
            {
                SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.CompanyPages.ProgramRemoved);

            }
            return RedirectToAction("governmentprogram");
        }

        [HttpGet]
        public ActionResult RemoveGovt()
        {
            SoundPower.Web.Notifications.AddErrorNotification(ResourceText.SharedPages.CouldNotProcessRelogin);
            return RedirectToAction("governmentprogram");
        }

        [RequireBiller]
        [HttpPost]
        [AntiForgeryHandleError]
        public ActionResult AddGovernmentProgram(ReferencedGovernmentProgram request)
        {
            if (ModelState.IsValid)
            {
                ResponseBase rspGovt = null;
                Company.GovernmentProgramServiceClient svcComp = null;
                try
                {
                    var token = UserAuthorization.CurrentUser;

                    svcComp = new Company.GovernmentProgramServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                    request.Name = "unknown";
                    rspGovt = svcComp.Add(request);
                    SoundPower.Web.Notifications.AddResponseNotifications(rspGovt);
                if (!rspGovt.IsFailure)
                {
                    SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.CompanyPages.ProgramAdded);
                    return RedirectToAction("governmentprogram");

                }
                else
                    {
                        SetGovtProgramsToAdd(svcComp);
                    }


                }
                catch (Exception ex)
                {
                    var bex = new SoundPower.ErrorTracking.BaseException("Save Failure", "TherapyCorner.Portal.Controllers.Company.AddGovernmentProgram", ex);
                    SoundPower.Web.Utilities.ReportError(bex);
                    throw bex;
                }
                finally
                {
                    if (svcComp != null) svcComp.Dispose();
                }



            }
            return View(request);
        }

        [HttpGet]
        public ActionResult AddGovernmentProgram()
        {
            www.therapycorner.com.company.MessageContracts.ReferencedGovernmentProgramListResponse rspGovt = null;
            Company.GovernmentProgramServiceClient svcComp = null;
            try
            {
                var token = UserAuthorization.CurrentUser;

                svcComp = new Company.GovernmentProgramServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                rspGovt = SetGovtProgramsToAdd(svcComp);

            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.Company.AddGovernmentProgram", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {
                if (svcComp != null) svcComp.Dispose();
            }


            if (rspGovt.IsFailure)
            {
                return RedirectToAction("index", "home");

            }


            return View(new ReferencedGovernmentProgram() { IsActive = true, Version="New" });
        }

        private ReferencedGovernmentProgramListResponse SetGovtProgramsToAdd(Company.GovernmentProgramServiceClient svcComp)
        {
            ReferencedGovernmentProgramListResponse rspGovt = svcComp.List(false);
            SoundPower.Web.Notifications.AddResponseNotifications(rspGovt);
            if (rspGovt.Programs == null) rspGovt.Programs = new www.therapycorner.com.company.ReferencedGovernmentProgramList();
            var programs = StaticData.GovernmentPrograms;
            foreach (var d in programs)
            {
                var existing = rspGovt.Programs.Find(i => i.ProgramId == d.ProgramId);
                if (existing != null)
                {
                    existing.Name = d.Name;
                }
                else
                {
                    rspGovt.Programs.Add(new www.therapycorner.com.company.ReferencedGovernmentProgram() { ProgramId = (int)d.ProgramId, IsActive = false, Name = d.Name, ClientCount = 0 });
                }
            }
            rspGovt.Programs.Sort((a, b) => a.Name.CompareTo(b.Name));
            rspGovt.Programs.RemoveAll(a => a.IsActive);
            ViewBag.Programs = rspGovt.Programs;
            return rspGovt;
        }

        [RequireBiller]
        public ActionResult Insurance()
        {
            www.therapycorner.com.company.MessageContracts.ReferencedInsuranceCompanyListResponse rspGovt = null;
            Company.InsuranceCompanyServiceClient svcComp = null;
            try
            {
                var token = UserAuthorization.CurrentUser;

                svcComp = new Company.InsuranceCompanyServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                rspGovt = svcComp.List(false);
                SoundPower.Web.Notifications.AddResponseNotifications(rspGovt);
                if (rspGovt.Companies == null) rspGovt.Companies = new www.therapycorner.com.company.ReferencedInsuranceCompanyList();
                var programs = StaticData.InsuranceCompanies(token.ClearingHouse.GetValueOrDefault(1));
                foreach (var d in programs)
                {
                    var existing = rspGovt.Companies.Find(i => i.CompanyId == d.CompanyId);
                    if (existing != null)
                    {
                        existing.Name = d.Name;
                    }

                }


            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.Company.Insurance", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {
                if (svcComp != null) svcComp.Dispose();
            }


            if (rspGovt.IsFailure)
            {
                return RedirectToAction("index", "home");

            }
            rspGovt.Companies.RemoveAll(c => !c.IsActive);
            rspGovt.Companies.Sort((a, b) => a.Name.CompareTo(b.Name));
            return View(rspGovt.Companies);
        }

        [RequireBiller]
        [HttpPost]
        [AntiForgeryHandleError]
        public ActionResult InsuranceDetails(ReferencedInsuranceCompany request)
        {
            ResponseBase rspGovt = null;
            Company.InsuranceCompanyServiceClient svcComp = null;
                       var token = UserAuthorization.CurrentUser;
         if (ModelState.IsValid)
            {
                try
                {
                 
                    svcComp = new Company.InsuranceCompanyServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                    rspGovt = svcComp.Update(request);
                    SoundPower.Web.Notifications.AddResponseNotifications(rspGovt);
                    if (!rspGovt.IsFailure)
                    {
                        SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.CompanyPages.ProgramAdded);
                        return RedirectToAction("insurance");

                    }
                    

                }
                catch (Exception ex)
                {
                    var bex = new SoundPower.ErrorTracking.BaseException("Save Failure", "TherapyCorner.Portal.Controllers.Company.InsuranceDetails", ex);
                    SoundPower.Web.Utilities.ReportError(bex);
                    throw bex;
                }
                finally
                {
                    if (svcComp != null) svcComp.Dispose();
                }

            }
            var programs = StaticData.InsuranceCompanies(token.ClearingHouse.GetValueOrDefault(1));
          
            var def = programs.Find(c => c.CompanyId == request.CompanyId);
            ViewBag.Company = def;
            return View(request);
        }

        [RequireBiller]
        public ActionResult InsuranceDetails(int id)
        {
            www.therapycorner.com.company.MessageContracts.ReferencedInsuranceCompanyListResponse rspGovt = null;
            Company.InsuranceCompanyServiceClient svcComp = null;
            ReferencedInsuranceCompany company = null;
            try
            {
                var token = UserAuthorization.CurrentUser;

                svcComp = new Company.InsuranceCompanyServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                rspGovt = svcComp.List(false);
                SoundPower.Web.Notifications.AddResponseNotifications(rspGovt);
                if (rspGovt.Companies == null) return RedirectToAction("insurance");
                var programs = StaticData.InsuranceCompanies(token.ClearingHouse.GetValueOrDefault(1));
                company = rspGovt.Companies.Find(c => c.CompanyId == id);
                if (company==null || !company.IsActive )
                {
                    return RedirectToAction("insurance");
                }
                var def = programs.Find(c => c.CompanyId == id);
                ViewBag.Company = def;

            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.Company.InsuranceDetails", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {
                if (svcComp != null) svcComp.Dispose();
            }


          

            return View(company);
        }

        [RequireBiller]
        public JsonResult  InsuranceOptions(string name)
        {
            JsonResult result = new JsonResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            GenericEntityListResponse rsp = new GenericEntityListResponse() { EntityList = new GenericEntityList() };
            www.therapycorner.com.company.MessageContracts.ReferencedInsuranceCompanyListResponse rspGovt = null;
            Company.InsuranceCompanyServiceClient svcComp = null;
            name = name.ToUpper();
            try
            {
                var token = UserAuthorization.CurrentUser;

                svcComp = new Company.InsuranceCompanyServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                rspGovt = svcComp.List(false);
                SoundPower.Web.Notifications.AddResponseNotifications(rspGovt);
                if (rspGovt.Companies == null) rspGovt.Companies = new www.therapycorner.com.company.ReferencedInsuranceCompanyList();
                var programs = StaticData.InsuranceCompanies(token.ClearingHouse.GetValueOrDefault(1));
                foreach (var d in programs)
                {
                    if (!d.Name.ToUpper().Contains(name) ) continue;
                    var existing = rspGovt.Companies.Find(i => i.CompanyId == d.CompanyId);
                    if (existing != null && existing.IsActive) continue;

                    rsp.EntityList.Add(new GenericEntity(d.CompanyId.ToString(),"InsuranceCompany",d.Name));
                }


            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.Company.InsuranceOptions", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                rsp.IsFailure = true;
                rsp.ErrorMessages.Add(bex.Message);
            }
            finally
            {
                if (svcComp != null) svcComp.Dispose();
            }

            rsp.EntityList.Sort((a, b) => a.Name.CompareTo(b.Name));

            if (rsp.EntityList.Count > 30) rsp.EntityList.RemoveRange(30, rsp.EntityList.Count - 30);
           
            result.Data = rsp;
            return result;
        }
        [RequireBiller]
        [HttpPost]
        [AntiForgeryHandleError]
        public ActionResult RemoveInsurance(string programId)
        {
            ResponseBase rspGovt = null;
            Company.InsuranceCompanyServiceClient svcComp = null;
            try
            {
                var token = UserAuthorization.CurrentUser;

                svcComp = new Company.InsuranceCompanyServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                rspGovt = svcComp.Remove(programId);
                SoundPower.Web.Notifications.AddResponseNotifications(rspGovt);



            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.Company.RemoveInsurance", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {
                if (svcComp != null) svcComp.Dispose();
            }


            if (!rspGovt.IsFailure)
            {
                SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.CompanyPages.InsuranceRemoved);

            }
            return RedirectToAction("insurance");
        }

        [HttpGet]
        public ActionResult RemoveInsurance()
        {
            SoundPower.Web.Notifications.AddErrorNotification(ResourceText.SharedPages.CouldNotProcessRelogin);
            return RedirectToAction("insurance");
        }

        [HttpGet]
        public ActionResult AddInsurance()
        {
            return View();
        }

        [RequireBiller]
        [HttpPost]
        [AntiForgeryHandleError]

        public ActionResult AddInsurance(string id)
        {
            ResponseBase rspGovt = null;
            Company.InsuranceCompanyServiceClient svcComp = null;
            try
            {
                var token = UserAuthorization.CurrentUser;

                svcComp = new Company.InsuranceCompanyServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                rspGovt = svcComp.Add(id);
                SoundPower.Web.Notifications.AddResponseNotifications(rspGovt);



            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.Company.AddInsurance", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {
                if (svcComp != null) svcComp.Dispose();
            }


            if (!rspGovt.IsFailure)
            {
                SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.CompanyPages.InsuranceAdded);
                return RedirectToAction("insurancedetails", new { id = id });

            }
            return View();
        }
    }
}