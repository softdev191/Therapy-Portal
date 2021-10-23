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

    public class ClientServiceController : Controller
    {
        [HttpPost]
        public JsonResult MakeupCount()
        {
            Company.ClientServiceServiceClient svcClient = null;
            var rsp = new ObjectIdResponseBase(); ;
            JsonResult result = new JsonResult();

            try
            {
                var token = UserAuthorization.CurrentUser;


                svcClient = new Company.ClientServiceServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                var rspSrch = svcClient.MakeupCounts();
                rspSrch.Services.RemoveAll(x => x.End < DateTime.Now);
                rsp.ErrorMessages = rspSrch.ErrorMessages;
                rsp.WarningMessages = rspSrch.WarningMessages;
                rsp.IsFailure = rspSrch.IsFailure;
                rsp.ObjectId = rsp.IsFailure ? "-" : rspSrch.Services.Sum(s=>s.MakeupCount.GetValueOrDefault(0)).ToString();
            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Makeup Counter Processing Failure", "TherapyCorner.Portal.Controllers.ClientServiceController.MakeupCount", ex);
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
        public JsonResult NeedRxCount()
        {
            Company.ClientServiceServiceClient svcClient = null;
            var rsp = new ObjectIdResponseBase(); ;
            JsonResult result = new JsonResult();

            try
            {
                var token = UserAuthorization.CurrentUser;


                svcClient = new Company.ClientServiceServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                var rspSrch = svcClient.NeedRx();
                rsp.ErrorMessages = rspSrch.ErrorMessages;
                rsp.WarningMessages = rspSrch.WarningMessages;
                rsp.IsFailure = rspSrch.IsFailure;
                rsp.ObjectId = rsp.IsFailure ? "-" : rspSrch.Services.Count.ToString();
            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Need Prescription Counter Processing Failure", "TherapyCorner.Portal.Controllers.ClientServiceController.NeeedRxCount", ex);
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
        public JsonResult PendingCount()
        {
            Company.ClientServiceServiceClient svcClient = null;
            var rsp = new ObjectIdResponseBase(); ;
            JsonResult result = new JsonResult();

            try
            {
                var token = UserAuthorization.CurrentUser;


                svcClient = new Company.ClientServiceServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                var rspSrch = svcClient.Pending();
                rsp.ErrorMessages = rspSrch.ErrorMessages;
                rsp.WarningMessages = rspSrch.WarningMessages;
                rsp.IsFailure = rspSrch.IsFailure;
                rsp.ObjectId = rsp.IsFailure ? "-" : rspSrch.Services.Count.ToString();
            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Pending Service Counter Processing Failure", "TherapyCorner.Portal.Controllers.ClientServiceController.PendingCount", ex);
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
        public JsonResult SchedulingCount()
        {
            Company.ClientServiceServiceClient svcClient = null;
            var rsp = new ObjectIdResponseBase(); ;
            JsonResult result = new JsonResult();

            try
            {
                var token = UserAuthorization.CurrentUser;


                svcClient = new Company.ClientServiceServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                var rspSrch = svcClient.NeedScheduling();
                rsp.ErrorMessages = rspSrch.ErrorMessages;
                rsp.WarningMessages = rspSrch.WarningMessages;
                rsp.IsFailure = rspSrch.IsFailure;
                rsp.ObjectId = rsp.IsFailure ? "-" : rspSrch.Services.Count.ToString();
            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Need Scheduling Counter Processing Failure", "TherapyCorner.Portal.Controllers.ClientServiceController.SchedulingCount", ex);
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

        // GET: ClientService
        [RequireWorkerFilter]
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
               client= GetLists(id.Value);
            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.ClientService.Create", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            return View(new ClientService()
            {
                Client = client,
                Duration = new GenericEntity("?", "Duration", null),
                Id = -1,
                Provider = new GenericEntity("?", "Staff", null),
                Rate = new GenericEntity("?", "Rate", null),
                Service = new GenericEntity("?", "Service", null),
                Start = DateTime.Today,
                Version = "NEW"
            });

        }

        [RequireWorkerFilter]
        public JsonResult NeedsScheduling(int id, string staffId)
        {
            JsonResult result = new JsonResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet };

            Company.ClientServiceClient svcClient = null;
            Company.FreqDurServiceClient svcDur = null;
            ClientInfoResponse rspClient = null;
            Models.ClientServiceScheduleInfoResponse data = new Models.ClientServiceScheduleInfoResponse();

            try
            {
                var token = UserAuthorization.CurrentUser;

                svcClient = new Company.ClientServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);

                svcDur = new Company.FreqDurServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                rspClient = svcClient.Details(id);

                if (rspClient.IsFailure )
                {
                    throw new Exception(string.Join("/n", rspClient.ErrorMessages));
                }

                data.Services.AddRange(from s in rspClient.Client.Services
                              where !s.End.HasValue
                              && (string.IsNullOrWhiteSpace(staffId) || staffId == s.Provider.UniqueId)
                              && s.AllowedCount.GetValueOrDefault(0) > s.ScheduledCount.GetValueOrDefault(0)
                              select new Models.ClientServiceScheduleInfo(s));

                var rspDurs = svcDur.List(false);
                foreach(var d in data.Services  )
                {
                    d.DurationTime = rspDurs.Durations.Find(f => f.FreqDurId.ToString() == d.Duration.UniqueId).Duration;
                }
            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.ClientService.NeedsScheduling", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                data.IsFailure = true;
                data.ErrorMessages.Add(bex.Message);
            }
            finally
            {
                if (svcClient != null) svcClient.Dispose();
                if (svcDur != null) svcDur.Dispose();

            }

            result.Data = data;

            return result;
        }

        [RequireWorkerFilter]
        public ActionResult Pending()
        {
            Company.ClientServiceServiceClient svc = null;
            ClientServiceListResponse rsp = null;

            try
            {
                var token = UserAuthorization.CurrentUser;
                svc = new Company.ClientServiceServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                rsp = svc.Pending();

            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Controllers.ClientService.Pending", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {
                if (svc != null) svc.Dispose();
            }


            SoundPower.Web.Notifications.AddResponseNotifications(rsp);



            return View(rsp.Services);
        }

        [RequireWorkerFilter]
        public ActionResult NeedRx()
        {
            Company.ClientServiceServiceClient svc = null;
            ClientServiceListResponse rsp = null;

            try
            {
                var token = UserAuthorization.CurrentUser;
                svc = new Company.ClientServiceServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                rsp = svc.NeedRx();

            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Controllers.ClientService.MissingRx", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {
                if (svc != null) svc.Dispose();
            }


            SoundPower.Web.Notifications.AddResponseNotifications(rsp);



            return View(rsp.Services);
        }

   

        [RequireWorkerFilter]
        public ActionResult NeedScheduling()
        {
            Company.ClientServiceServiceClient svc = null;
            ClientServiceListResponse rsp = null;

            try
            {
                var token = UserAuthorization.CurrentUser;
                svc = new Company.ClientServiceServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                rsp = svc.NeedScheduling();

            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Controllers.ClientService.NeedsScheduling", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {
                if (svc != null) svc.Dispose();
            }


            SoundPower.Web.Notifications.AddResponseNotifications(rsp);


            return View(rsp.Services);
        }
        [HttpPost]
        [AntiForgeryHandleError]
        [RequireWorkerFilter]
        public ActionResult Create(HttpPostedFileBase file, ClientService model)
        {
            bool fileGood = true;
            if (file != null && file.ContentLength > 3000000)
            {
                SoundPower.Web.Notifications.AddErrorNotification(ResourceText.CredentialPages.FileTooLarge);
                fileGood = false;
            }
            if (ModelState.IsValid && fileGood)
            {
                if (model.CPTs != null)
                {
                    foreach (var cpt in model.CPTs)
                    {
                        if (!cpt.Amount.HasValue) cpt.Amount = 0;
                    }
                }
                Company.ClientServiceServiceClient svc = null;
                ResponseBase rsp = null;
                ClientServiceRequest req = new ClientServiceRequest() {  Service = model };
                if (file != null && file.ContentLength > 0)
                {

                    req.FileData = new byte[file.InputStream.Length];
                    file.InputStream.Read(req.FileData, 0, req.FileData.Length);
                    req.Service.RxFileType = file.ContentType;
                    var parts = file.FileName.Split('.');
                    req.Service.RxFileExtension = parts[parts.Length - 1];
                }
                try
                {
                    var token = UserAuthorization.CurrentUser;
                    svc = new Company.ClientServiceServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                    rsp = svc.Create(req);

                }
                catch (Exception ex)
                {
                    var bex = new SoundPower.ErrorTracking.BaseException("Save Failure", "TherapyCorner.Controllers.ClientService.Create", ex);
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

                    SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.ClientPages.ServiceAdded);
                    return RedirectToAction("details", "client",new { id = model.Client.UniqueId });
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
                var bex = new SoundPower.ErrorTracking.BaseException("Return Processing Failure", "TherapyCorner.Portal.Controllers.ClientService.Create", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            return View(model);
        }

        [HttpPost]
        [AntiForgeryHandleError]
        [RequireBillerOrWorker]
        public ActionResult Update(HttpPostedFileBase file, ClientService model)
        {
            bool fileGood = true;
            if (file != null && file.ContentLength > 3000000)
            {
                SoundPower.Web.Notifications.AddErrorNotification(ResourceText.CredentialPages.FileTooLarge);
                fileGood = false;
            }
            if (ModelState.IsValid && fileGood)
            {

                if (model.CPTs!= null)
                {
                    foreach (var cpt in model.CPTs)
                    {
                        if (!cpt.Amount.HasValue) cpt.Amount = 0;
                    } 
                }
                Company.ClientServiceServiceClient svc = null;
                ResponseBase rsp = null;
                ClientServiceRequest req = new ClientServiceRequest() { Service = model };
                if (file != null && file.ContentLength > 0)
                {

                    req.FileData = new byte[file.InputStream.Length];
                    file.InputStream.Read(req.FileData, 0, req.FileData.Length);
                    req.Service.RxFileType = file.ContentType;
                    var parts = file.FileName.Split('.');
                    req.Service.RxFileExtension = parts[parts.Length - 1];
                }
                try
                {
                    var token = UserAuthorization.CurrentUser;
                    svc = new Company.ClientServiceServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                    rsp = svc.Update(req);

                }
                catch (Exception ex)
                {
                    var bex = new SoundPower.ErrorTracking.BaseException("Save Failure", "TherapyCorner.Controllers.ClientService.Update", ex);
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

                    SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.ClientPages.ServiceUpdated);
                    return RedirectToAction("details", "client", new { id = model.Client.UniqueId });
                }
                else
                {
                    SiteUtilities.ApplyFieldIssues(rsp, ModelState);
                    ViewBag.FieldIssues = rsp.FieldIssues;
                }

            }

            try
            {
                GetLists(int.Parse(model.Client.UniqueId),int.Parse(model.Service.UniqueId), model.Start,model.End,model.CPTs);
            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Return Processing Failure", "TherapyCorner.Portal.Controllers.ClientService.Update", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            return View(model);
        }


        [HttpGet]
        [RequireBillerOrWorker]
        public ActionResult Update(int? id)
        {
            if (!id.HasValue)
            {

                SoundPower.Web.Notifications.AddErrorNotification(ResourceText.SharedPages.CouldNotProcessRelogin);
                return RedirectToAction("index", "client");

            }


            Company.ClientServiceServiceClient svc = null;
                ClientServiceResponse rsp = null;

                try
                {
                    var token = UserAuthorization.CurrentUser;
                    svc = new Company.ClientServiceServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                    rsp = svc.Details(id.Value);
                GetLists(int.Parse(rsp.Service.Client.UniqueId),int.Parse(rsp.Service.Service.UniqueId),rsp.Service.Start, rsp.Service.End,rsp.Service.CPTs );
                 
                }
                catch (Exception ex)
                {
                    var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Controllers.ClientService.Update", ex);
                    SoundPower.Web.Utilities.ReportError(bex);
                    throw bex;
                }
                finally
                {
                    if (svc != null) svc.Dispose();
                }


                SoundPower.Web.Notifications.AddResponseNotifications(rsp);
               if (rsp.Service.ApprovedFrom.HasValue && rsp.Service.Start< rsp.Service.ApprovedFrom.Value )
            {
                SoundPower.Web.Notifications.AddWarningMessage(ResourceText.ClientPages.StartsBeforeAuth);
            }

          
            return View(rsp.Service);
        }

        [HttpGet]
        [RequireBillerOrWorker]
        public ActionResult Image(int id)
        {



            Company.ClientServiceServiceClient svc = null;
            www.therapycorner.com.account.MessageContracts.FileResponse rsp = null;

            try
            {
                var token = UserAuthorization.CurrentUser;
                svc = new Company.ClientServiceServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                rsp = svc.Image(id);

            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Controllers.ClientService.Update", ex);
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

        [HttpPost]
        [AntiForgeryHandleError]
        [RequireWorkerFilter]
        public ActionResult Discontinue(int id)
        {



            Company.ClientServiceServiceClient svc = null;
            ClientServiceResponse rsp = null;
            ResponseBase rspAction = null;
            try
            {
                var token = UserAuthorization.CurrentUser;
                svc = new Company.ClientServiceServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                rsp = svc.Details(id);

                rsp.Service.End = DateTime.Today;
                rspAction= svc.Update(new ClientServiceRequest() { Service = rsp.Service });
            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Controllers.ClientService.Discontinue", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {
                if (svc != null) svc.Dispose();
            }


            SoundPower.Web.Notifications.AddResponseNotifications(rspAction);
            if(rspAction.IsFailure)
            {
                return RedirectToAction("update", new { id = id });
            }
            else
            {
                SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.ClientPages.ServiceEnded);
                return RedirectToAction("details", "client", new { id = rsp.Service.Client.UniqueId });
            }


        }

        [HttpPost]
        [AntiForgeryHandleError]
        [RequireWorkerFilter]
        public ActionResult Delete(int id)
        {



            Company.ClientServiceServiceClient svc = null;
            ClientServiceResponse rsp = null;
            ResponseBase rspAction = null;
            try
            {
                var token = UserAuthorization.CurrentUser;
                svc = new Company.ClientServiceServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                rsp = svc.Details(id);

                rspAction = svc.Remove(id);
            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Controllers.ClientService.Delete", ex);
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
                SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.ClientPages.ServiceDeleted);
                return RedirectToAction("details", "client", new { id = rsp.Service.Client.UniqueId });
            }


        }

        [HttpGet]
        public ActionResult Delete()
        {
            SoundPower.Web.Notifications.AddErrorNotification(ResourceText.SharedPages.CouldNotProcessRelogin);
            return RedirectToAction("index", "client");
        }


        [HttpGet]

        public ActionResult Discontinue()
        {
            SoundPower.Web.Notifications.AddErrorNotification(ResourceText.SharedPages.CouldNotProcessRelogin);
            return RedirectToAction("index","client");
        }
        private PersonField GetLists(int id, int? serviceId = null, DateTime? fromDate=null, DateTime? toDate =null, CPTRateList cptRates=null)
        {
            Company.ClientServiceClient svcClient = null;
            Company.ServiceInfoServiceClient svc = null;
            ClientInfoResponse rspClient = null;
            PersonField result = null;
            try
            {
                var token = UserAuthorization.CurrentUser;

                if (cptRates !=null)
                {
                    var cpts = StaticData.CPTCodes;
                    foreach(var rate in cptRates )
                    {
                        var cpt = cpts.Find(d => d.UniqueId == rate.CPT.UniqueId);
                        rate.CPT = cpt;
                    }
                }

                svcClient = new Company.ClientServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                rspClient = svcClient.Details(id);
               
                result = rspClient.Client.ToPerson();
                svc = new Company.ServiceInfoServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                if (serviceId.HasValue)
                {
                    var rsp = svc.Details(serviceId.Value);
                    ViewBag.Service = rsp.Service;
                }
                else
                {
                    var rsp = svc.List(true);
                    ViewBag.Services = rsp.Services;
                }

            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.ClientService.GetLists", ex);
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