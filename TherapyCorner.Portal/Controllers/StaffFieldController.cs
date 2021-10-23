using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using www.soundpower.biz.common;
using www.therapycorner.com.account;
using www.therapycorner.com.account.MessageContracts;

namespace TherapyCorner.Portal.Controllers
{
    [RequireHttps]
    [RequireUser]
    [CompanyFilter]
    [RequireAdmin]
    public class StaffFieldController : Controller
    {
        public ActionResult Index()
        {

            Company.StaffServiceClient svcStaff = null;
            FieldTypeListResponse rspStaff = null;
            try
            {
                var token = UserAuthorization.CurrentUser;


                svcStaff = new Company.StaffServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                rspStaff = svcStaff.FieldList();

            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.StaffField.Index", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {
                if (svcStaff != null) svcStaff.Dispose();

            }

            SoundPower.Web.Notifications.AddResponseNotifications(rspStaff);
            if (rspStaff.IsFailure) return RedirectToAction("index", "home");
            return View(rspStaff.Fields);
        }

        [HttpGet]
        public ActionResult Create()
        {
            return View(new FieldType() { IsActive = true, FieldId = -1, Version = "New" });
        }

        [HttpPost]
        [AntiForgeryHandleError]
        public ActionResult Create(FieldType model)
        {
           if (ModelState.IsValid)
            {
                Company.StaffServiceClient svcStaff = null;
                ResponseBase rspStaff = null;
                try
                {
                    var token = UserAuthorization.CurrentUser;


                    svcStaff = new Company.StaffServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                    rspStaff = svcStaff.CreateField(model);

                }
                catch (Exception ex)
                {
                    var bex = new SoundPower.ErrorTracking.BaseException("Save Processing Failure", "TherapyCorner.Portal.Controllers.StaffField.Create", ex);
                    SoundPower.Web.Utilities.ReportError(bex);
                    throw bex;
                }
                finally
                {
                    if (svcStaff != null) svcStaff.Dispose();

                }

                SoundPower.Web.Notifications.AddResponseNotifications(rspStaff);
                if (rspStaff.IsFailure)
                {
                    SiteUtilities.ApplyFieldIssues(rspStaff, ModelState);
                }
                else
                {
                    SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.StaffFieldPages.Created);
                    return RedirectToAction("index");
                }
            }
            return View(model);
        }

        [HttpGet]
        public ActionResult Edit(int? id)
        {
            if (!id.HasValue)
            {

                SoundPower.Web.Notifications.AddErrorNotification(ResourceText.SharedPages.CouldNotProcessRelogin);
                return RedirectToAction("index");

            }

            Company.StaffServiceClient svcStaff = null;
            FieldTypeListResponse rspStaff = null;
            try
            {
                var token = UserAuthorization.CurrentUser;


                svcStaff = new Company.StaffServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                rspStaff = svcStaff.FieldList();

            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.StaffField.Edit", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {
                if (svcStaff != null) svcStaff.Dispose();

            }

            SoundPower.Web.Notifications.AddResponseNotifications(rspStaff);
            if (rspStaff.IsFailure) return RedirectToAction("index", "home");
            return View(rspStaff.Fields.Find(f=>f.FieldId==id));
        }

        [HttpPost]
        [AntiForgeryHandleError]
        public ActionResult Edit(FieldType model)
        {
            if (ModelState.IsValid)
            {
                Company.StaffServiceClient svcStaff = null;
                ResponseBase rspStaff = null;
                try
                {
                    var token = UserAuthorization.CurrentUser;

                  
                    
                    svcStaff = new Company.StaffServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                    rspStaff = svcStaff.UpdateField(model);

                }
                catch (Exception ex)
                {
                    var bex = new SoundPower.ErrorTracking.BaseException("Save Processing Failure", "TherapyCorner.Portal.Controllers.StaffField.Edit", ex);
                    SoundPower.Web.Utilities.ReportError(bex);
                    throw bex;
                }
                finally
                {
                    if (svcStaff != null) svcStaff.Dispose();

                }

                SoundPower.Web.Notifications.AddResponseNotifications(rspStaff);
                if (rspStaff.IsFailure)
                {
                    SiteUtilities.ApplyFieldIssues(rspStaff, ModelState);
                }
                else
                {
                    SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.StaffFieldPages.Updated);
                    return RedirectToAction("index");
                }
            }
            return View(model);
        }

  

    
    }
}