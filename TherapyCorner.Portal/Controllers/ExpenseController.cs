using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using www.therapycorner.com.company.MessageContracts;
using www.therapycorner.com.company;
using www.soundpower.biz.common;
using www.therapycorner.com.account.MessageContracts;

namespace TherapyCorner.Portal.Controllers
{
    [RequireHttps]
    [RequireUser]
    [CompanyFilter]
    public class ExpenseController : Controller
    {
        [HttpGet]
        public ActionResult Image(int id)
        {


           Company.ExpenseClientService svc = null;
            FileResponse rsp = null;

            try
            {
                var token = UserAuthorization.CurrentUser;
                svc = new Company.ExpenseClientService(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                rsp = svc.Image(id);

            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.Expense.Image", ex);
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
            return RedirectToAction("index", "expense");


        }

        [HttpGet]
        public ActionResult Index()
        {
            return Index(new ExpenseSearchRequest()
            {
                StaffId=-1,
                Status="0,1"
            }
            );
        }
        [RequireAdmin]
        [HttpPost]
        public ActionResult Reject(int rejectId, string reason)
        {
            if (ModelState.IsValid)
            {
                Company.ExpenseClientService svcExp = null;
             

                try
                {
                    var token = UserAuthorization.CurrentUser;

                    svcExp = new Company.ExpenseClientService(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                    var rExp = svcExp.Reject(new RejectionRequest() { ObjectId = rejectId.ToString(), Reason = reason });
                    SoundPower.Web.Notifications.AddResponseNotifications(rExp);
                    if (!rExp.IsFailure)
                    {
                        SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.ExpensePages.ExpenseRejected);
                    }
                }
                catch (Exception ex)
                {
                    var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Controllers.Expense.Reject", ex);
                    SoundPower.Web.Utilities.ReportError(bex);
                    throw bex;
                }
                finally
                {
                    if (svcExp != null) svcExp.Dispose();


                }
            }
            
            return RedirectToAction("approvals");
        }
        [HttpGet]
        public ActionResult Reject()
        {
            SoundPower.Web.Notifications.AddErrorNotification(ResourceText.SharedPages.CouldNotProcessRelogin);
            return RedirectToAction("Approvals");
        }

        [RequireAdmin]
        [HttpPost]
        public ActionResult MoreInfo(int moreInfoId, string reason)
        {
            if (ModelState.IsValid)
            {
                Company.ExpenseClientService svcExp = null;


                try
                {
                    var token = UserAuthorization.CurrentUser;

                    svcExp = new Company.ExpenseClientService(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                    var rExp = svcExp.MoreInfo(new RejectionRequest() { ObjectId = moreInfoId.ToString(), Reason = reason });
                    SoundPower.Web.Notifications.AddResponseNotifications(rExp);
                    if (!rExp.IsFailure)
                    {
                        SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.ExpensePages.ExpenseMO);
                    }
                }
                catch (Exception ex)
                {
                    var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Controllers.Expense.MoreInfo", ex);
                    SoundPower.Web.Utilities.ReportError(bex);
                    throw bex;
                }
                finally
                {
                    if (svcExp != null) svcExp.Dispose();


                }
            }

            return RedirectToAction("approvals");
        }
        [HttpGet]
        public ActionResult MoreInfo()
        {
            SoundPower.Web.Notifications.AddErrorNotification(ResourceText.SharedPages.CouldNotProcessRelogin);
            return RedirectToAction("Approvals");
        }
        [RequireAdmin]
        [HttpPost]
        public ActionResult Approve(int approveId)
        {
            if (ModelState.IsValid)
            {
                Company.ExpenseClientService svcExp = null;


                try
                {
                    var token = UserAuthorization.CurrentUser;

                    svcExp = new Company.ExpenseClientService(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                    var rExp = svcExp.Approve(approveId);
                    SoundPower.Web.Notifications.AddResponseNotifications(rExp);
                    if (!rExp.IsFailure)
                    {
                        SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.ExpensePages.ExpenseApproved);
                    }
                }
                catch (Exception ex)
                {
                    var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Controllers.Expense.Approve", ex);
                    SoundPower.Web.Utilities.ReportError(bex);
                    throw bex;
                }
                finally
                {
                    if (svcExp != null) svcExp.Dispose();


                }
            }

            return RedirectToAction("approvals");
        }
        [HttpGet]
        public ActionResult Approve()
        {
            SoundPower.Web.Notifications.AddErrorNotification(ResourceText.SharedPages.CouldNotProcessRelogin);
            return RedirectToAction("Approvals");
        }

        [HttpPost]
        public ActionResult Index(ExpenseSearchRequest request)
        {
            if (ModelState.IsValid)
            {
                Company.ExpenseClientService svcExp = null;


                try
                {
                    var token = UserAuthorization.CurrentUser;

                    svcExp = new Company.ExpenseClientService(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                    var rExp = svcExp.List(request);
                    SoundPower.Web.Notifications.AddResponseNotifications(rExp);

                    if (rExp.Expenses != null && rExp.Expenses.Count > 0) rExp.Expenses.Sort((a, b) => a.MadeOn.CompareTo(b.MadeOn));
                    ViewBag.Expenses = rExp.Expenses;
                }
                catch (Exception ex)
                {
                    var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Controllers.Expense.Index", ex);
                    SoundPower.Web.Utilities.ReportError(bex);
                    throw bex;
                }
                finally
                {
                    if (svcExp != null) svcExp.Dispose();


                }
            }
            else
            {
                ViewBag.Expenses = new ExpenseInfoList();
            }
            return View(request);
        }

        [RequireAdmin]
        [HttpGet]
        public ActionResult Approvals()
        {
            try
            {

                SetStaffList(null);
            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Staff ListProcessing Failure", "TherapyCorner.Controllers.Expense.Approvals", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            return Index(new ExpenseSearchRequest()
            {
                Status = "0"
            }
            );
        }

        [RequireAdmin]
        [HttpPost]
        public ActionResult Approvals(ExpenseSearchRequest request)
        {
            if (ModelState.IsValid)
            {
                Company.ExpenseClientService svcExp = null;


                try
                {
                    var token = UserAuthorization.CurrentUser;

                    svcExp = new Company.ExpenseClientService(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                    var rExp = svcExp.List(request);
                    SoundPower.Web.Notifications.AddResponseNotifications(rExp);

                    if (rExp.Expenses != null && rExp.Expenses.Count > 0) rExp.Expenses.Sort((a, b) => a.MadeOn.CompareTo(b.MadeOn));
                    ViewBag.Expenses = rExp.Expenses;
                }
                catch (Exception ex)
                {
                    var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Controllers.Expense.Approvals", ex);
                    SoundPower.Web.Utilities.ReportError(bex);
                    throw bex;
                }
                finally
                {
                    if (svcExp != null) svcExp.Dispose();


                }
            }
            else
            {
                ViewBag.Expenses = new ExpenseInfoList();
            }
            try
            {

                SetStaffList(request.StaffId);
            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Staff ListProcessing Failure", "TherapyCorner.Controllers.Expense.Approvals", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }

            return View(request);
        }


        private void SetStaffList(int? staffId)
        {
            List<SelectListItem> results = new List<SelectListItem>();
            results.Add(new SelectListItem() { Text = ResourceText.Dictionary.All, Value = "" });


           
            Company.StaffServiceClient svcStaff = null;
         
            StaffInfoListResponse rspStaff = null;
            try
            {
                var token = UserAuthorization.CurrentUser;

              
                svcStaff = new Company.StaffServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                rspStaff = svcStaff.Search(new StaffSearchRequest()
                {
                    Status= StaffSearchRequest.StatusEnum.NotInactiveOnly
                });

            }
            catch (Exception ex)
            {
                throw new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.Staff.SetStaffList", ex);
           
            }
            finally
            {
                if (svcStaff != null) svcStaff.Dispose();

            }
            rspStaff.Staff.Sort((a, b) => a.DisplayName.CompareTo(b.DisplayName));

            foreach (var s in rspStaff.Staff) results.Add(new SelectListItem() { Text = s.DisplayName, Value = s.StaffId.ToString(), Selected = s.StaffId == staffId.GetValueOrDefault(0) });
            ViewBag.Staff = results;
        }
        [HttpGet]
        public ActionResult Create()
        {
            return View(new  ExpenseInfo()
                {
                    MadeOn=DateTime.Now.Date,
                    MadeBy= new GenericEntity("0","Staff",null),
                    Amount=0,
                    ExpenseId=-1,
                    Status= ExpenseStatusEnum.Pending
                
            });
        }

        [HttpPost]
        public ActionResult Create(ExpenseInfo model, HttpPostedFileBase file)
        {
            bool fileGood = true;
            if (file != null && file.ContentLength > 3000000)
            {
                SoundPower.Web.Notifications.AddErrorNotification(ResourceText.CredentialPages.FileTooLarge);
                fileGood = false;
            }
            if (ModelState.IsValid && fileGood)
            {
                Company.ExpenseClientService svcExp = null;
                ExpenseInfoRequest req = new ExpenseInfoRequest()
                {
                    Expense = model
                };
                if (file != null && file.ContentLength > 0)
                {

                    req.FileData = new byte[file.InputStream.Length];
                    file.InputStream.Read(req.FileData, 0, req.FileData.Length);
                    req.Expense.RcptType = file.ContentType;
                    var parts = file.FileName.Split('.');
                    req.Expense.RcptExtension = parts[parts.Length - 1];
                }
                ResponseBase rsp = null;
                try
                {
                    var token = UserAuthorization.CurrentUser;

                    svcExp = new Company.ExpenseClientService(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                    rsp = svcExp.Create(req);

                    
                }
                catch (Exception ex)
                {
                    var bex = new SoundPower.ErrorTracking.BaseException("Save Processing Failure", "TherapyCorner.Controllers.Expense.Create", ex);
                    SoundPower.Web.Utilities.ReportError(bex);
                    throw bex;
                }
                finally
                {
                    if (svcExp != null) svcExp.Dispose();


                }

                SoundPower.Web.Notifications.AddResponseNotifications(rsp);
                if (!rsp.IsFailure)
                {

                    SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.ExpensePages.ReportCreated);
                    return RedirectToAction("index", "expense");
                }
                else
                {
                    SiteUtilities.ApplyFieldIssues(rsp, ModelState);

                }
            }
            
            return View(model);
        }

        [HttpGet]
        public ActionResult Update(int? id)
        {
            if (!id.HasValue)
            {

                SoundPower.Web.Notifications.AddErrorNotification(ResourceText.SharedPages.CouldNotProcessRelogin);
                return RedirectToAction("index");

            }
            Company.ExpenseClientService svcExp = null;

            ExpenseInfoResponse rsp = null;
            try
            {
                var token = UserAuthorization.CurrentUser;

                svcExp = new Company.ExpenseClientService(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                rsp = svcExp.Details(id.Value);
             
            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Controllers.Expense.Update", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {
                if (svcExp != null) svcExp.Dispose();


            }
            SoundPower.Web.Notifications.AddResponseNotifications(rsp);
            if (rsp.IsFailure)
            {

                return RedirectToAction("index", "expense");
            }
            return View(rsp.Expense);
        }

        [HttpPost]
        public ActionResult Update(ExpenseInfo model, HttpPostedFileBase file)
        {
            bool fileGood = true;
            if (file != null && file.ContentLength > 3000000)
            {
                SoundPower.Web.Notifications.AddErrorNotification(ResourceText.CredentialPages.FileTooLarge);
                fileGood = false;
            }
            if (ModelState.IsValid && fileGood)
            {
                Company.ExpenseClientService svcExp = null;
                ExpenseInfoRequest req = new ExpenseInfoRequest()
                {
                    Expense = model
                };
                if (file != null && file.ContentLength > 0)
                {

                    req.FileData = new byte[file.InputStream.Length];
                    file.InputStream.Read(req.FileData, 0, req.FileData.Length);
                    req.Expense.RcptType = file.ContentType;
                    var parts = file.FileName.Split('.');
                    req.Expense.RcptExtension = parts[parts.Length - 1];
                }
                ResponseBase rsp = null;
                try
                {
                    var token = UserAuthorization.CurrentUser;

                    svcExp = new Company.ExpenseClientService(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                    rsp = svcExp.Update(req);


                }
                catch (Exception ex)
                {
                    var bex = new SoundPower.ErrorTracking.BaseException("Save Processing Failure", "TherapyCorner.Controllers.Expense.Update", ex);
                    SoundPower.Web.Utilities.ReportError(bex);
                    throw bex;
                }
                finally
                {
                    if (svcExp != null) svcExp.Dispose();


                }

                SoundPower.Web.Notifications.AddResponseNotifications(rsp);
                if (!rsp.IsFailure)
                {

                    SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.ExpensePages.ReportCreated);
                    return RedirectToAction("index", "expense");
                }
                else
                {
                    SiteUtilities.ApplyFieldIssues(rsp, ModelState);

                }
            }

            return View(model);
        }
    }
}
