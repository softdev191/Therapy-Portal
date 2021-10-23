using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using www.soundpower.biz.common;
using www.therapycorner.com.account;
using www.therapycorner.com.company;
using www.therapycorner.com.company.MessageContracts;

namespace TherapyCorner.Portal.Controllers
{
    [RequireHttps]
    [RequireUser]
    [RequireAdmin]
    [CompanyFilter]
    public class StaffMatrixController : Controller
    {
        // GET: StaffMatrix
        public ActionResult Index()
        {
            MatrixFieldListResponse rsp = null;
            Company.MatrixServiceClient svcComp = null;
            try
            {
                var token = UserAuthorization.CurrentUser;

                svcComp = new Company.MatrixServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                rsp = svcComp.List();
                SoundPower.Web.Notifications.AddResponseNotifications(rsp);
            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.StaffMatrix.Index", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {
                if (svcComp != null) svcComp.Dispose();
            }


            if (rsp.IsFailure)
            {
                return RedirectToAction("index", "home");

            }
            rsp.Fields.Sort((a, b) => a.OrderNumber.CompareTo(b.OrderNumber));
            return View(rsp.Fields);
        }


        public ActionResult Generate()
        {
            MatrixRowListResponse rsp = null;
            Company.MatrixServiceClient svcComp = null;
            try
            {
                var token = UserAuthorization.CurrentUser;

                svcComp = new Company.MatrixServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                var frsp = svcComp.List();
                ViewBag.Fields = frsp.Fields;
                rsp = svcComp.Generate();
                SoundPower.Web.Notifications.AddResponseNotifications(rsp);
            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.StaffMatrix.Generate", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {
                if (svcComp != null) svcComp.Dispose();
            }


        
            return View(rsp.Rows);
        }

        public ActionResult Export()
        {
            MatrixRowListResponse rsp = null;
            Company.MatrixServiceClient svcComp = null;
            MatrixFieldList fields = null;
            try
            {
                var token = UserAuthorization.CurrentUser;

                svcComp = new Company.MatrixServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                var frsp = svcComp.List();
                fields = frsp.Fields;
                rsp = svcComp.Generate();
                SoundPower.Web.Notifications.AddResponseNotifications(rsp);
            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.StaffMatrix.Export", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {
                if (svcComp != null) svcComp.Dispose();
            }


            if (rsp.IsFailure)
            {
                return RedirectToAction("generate");

            }
            fields.Sort((a, b) => a.OrderNumber.CompareTo(b.OrderNumber));

            List<string> rows = new List<string>();
            List<string> cols = (from f in fields select f.Header).ToList();
            rows.Add(string.Join(",", cols));
            string value = null;
            FieldValue rv = null;
            foreach(var p in rsp.Rows)
            {
                cols = new List<string>();
                foreach(var f in fields)
                {
                    rv = p.Values.Find(c => c.FieldId == f.OrderNumber);
                    value = rv == null ? "" : rv.Value ?? "";
                    cols.Add(value.Replace(',',' '));
                }
                rows.Add(string.Join(",", cols));

            }
            string csv = string.Join("\n", rows);
            return File(new System.Text.UTF8Encoding().GetBytes(csv), "text/csv", "staffmatrix.csv");
        }

        [AntiForgeryHandleError]
        [HttpPost]
        public ActionResult Remove(int id)
        {
            ResponseBase rsp = null;
            Company.MatrixServiceClient svcComp = null;
            try
            {
                var token = UserAuthorization.CurrentUser;

                svcComp = new Company.MatrixServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                rsp = svcComp.Remove(id);
                SoundPower.Web.Notifications.AddResponseNotifications(rsp);
            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.StaffMatrix.Remove", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {
                if (svcComp != null) svcComp.Dispose();
            }


            if (!rsp.IsFailure)
            {
                SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.StaffMatrixPages.RemoveSuccess);

            }
            return RedirectToAction("index");
        }


        [HttpGet]
        public ActionResult Remove()
        {
            SoundPower.Web.Notifications.AddErrorNotification(ResourceText.SharedPages.CouldNotProcessRelogin);
            return RedirectToAction("index");
        }

        [HttpGet]
        public ActionResult Create()
        {
            SetSources();
            return View(new MatrixField() { OrderNumber=int.MaxValue, Source=new GenericEntity("0","MatrixSource",null),FieldId=new GenericEntity("0","MatrixField",null)});
        }

        [HttpPost]
        [AntiForgeryHandleError]
        public ActionResult Create(MatrixField model)
        {
            if (ModelState.IsValid)
            {
                ResponseBase rsp = null;
                Company.MatrixServiceClient svcComp = null;
                try
                {
                    var token = UserAuthorization.CurrentUser;

                    svcComp = new Company.MatrixServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                    rsp = svcComp.Create(model);
                    SoundPower.Web.Notifications.AddResponseNotifications(rsp);
                }
                catch (Exception ex)
                {
                    var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.StaffMatrix.Create", ex);
                    SoundPower.Web.Utilities.ReportError(bex);
                    throw bex;
                }
                finally
                {
                    if (svcComp != null) svcComp.Dispose();
                }


                if (!rsp.IsFailure)
                {
                    SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.StaffMatrixPages.CreateSuccess);
                    return RedirectToAction("index");
                }
                else
                {
                    SiteUtilities.ApplyFieldIssues(rsp, ModelState);
                }
            }

            SetSources();
            return View(model);
        }

        private void SetSources()
        {
            Company.MatrixServiceClient svcComp = null;
            try
            {
                var token = UserAuthorization.CurrentUser;

                svcComp = new Company.MatrixServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                ViewBag.Personal = svcComp.SourceFields("0").EntityList;
                ViewBag.Staff = svcComp.SourceFields("1").EntityList;
                ViewBag.Credential = svcComp.SourceFields("2").EntityList;
                ViewBag.Custom = svcComp.SourceFields("3").EntityList;


            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Field Processing Failure", "TherapyCorner.Portal.Controllers.StaffMatrix.SetSources", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {
                if (svcComp != null) svcComp.Dispose();
            }

        }
        [HttpPost]
        [AntiForgeryHandleError]
        public ActionResult Update(MatrixField model)
        {
            if (ModelState.IsValid)
            {
                ResponseBase rsp = null;
                Company.MatrixServiceClient svcComp = null;
                try
                {
                    var token = UserAuthorization.CurrentUser;

                    svcComp = new Company.MatrixServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                    rsp = svcComp.Update(model);
                    SoundPower.Web.Notifications.AddResponseNotifications(rsp);
                }
                catch (Exception ex)
                {
                    var bex = new SoundPower.ErrorTracking.BaseException("Save Processing Failure", "TherapyCorner.Portal.Controllers.StaffMatrix.Update", ex);
                    SoundPower.Web.Utilities.ReportError(bex);
                    throw bex;
                }
                finally
                {
                    if (svcComp != null) svcComp.Dispose();
                }


                if (!rsp.IsFailure)
                {
                    SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.StaffMatrixPages.UpdateSuccess);
                    return RedirectToAction("index");

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

            MatrixFieldResponse rsp = null;
            Company.MatrixServiceClient svcComp = null;
            try
            {
                var token = UserAuthorization.CurrentUser;

                svcComp = new Company.MatrixServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                rsp = svcComp.Details(id.Value);
                SoundPower.Web.Notifications.AddResponseNotifications(rsp);
            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.StaffMatrix.Update", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {
                if (svcComp != null) svcComp.Dispose();
            }


            if (rsp.IsFailure || rsp.Field == null)
            {
                return RedirectToAction("index");

            }
            return View(rsp.Field);
        }
    }
}