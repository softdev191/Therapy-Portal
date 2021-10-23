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
    public class QuestionController : Controller
    {
        [CompanyFilter]
        public JsonResult List()
        {
            JsonResult result = new JsonResult();
            result.JsonRequestBehavior = JsonRequestBehavior.AllowGet;

            QuestionTypeListResponse  rsp = new QuestionTypeListResponse() ;
            Company.QuestionServiceClient svcComp = null;
            try
            {
                var token = UserAuthorization.CurrentUser;

                svcComp = new Company.QuestionServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                rsp = svcComp.List(true);
            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.Question.List", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                rsp.IsFailure = true;
                rsp.ErrorMessages.Add(bex.Message);
            }
            finally
            {
                if (svcComp != null) svcComp.Dispose();
            }


         
            rsp.Questions.Sort((a, b) => a.Label.CompareTo(b.Label));
           


            result.Data = rsp;
            return result;
        }

        [RequireAdmin]
        [CompanyFilter]
        public ActionResult Index()
        {
            QuestionTypeListResponse rsp = null;
            Company.QuestionServiceClient svcComp = null;
            try
            {
                var token = UserAuthorization.CurrentUser;

                svcComp = new Company.QuestionServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                rsp = svcComp.List(false);
                SoundPower.Web.Notifications.AddResponseNotifications(rsp);
            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.Question.Index", ex);
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
            rsp.Questions.Sort((a, b) => b.IsActive.GetValueOrDefault().CompareTo(a.IsActive.GetValueOrDefault()));
            return View(rsp.Questions);
        }

    

        [RequireAdmin]
        [HttpGet]
        public ActionResult Create()
        {
            return View(new QuestionType() {  IsActive=true, Type= ValueTypeEnum.String });
        }

        [RequireAdmin]
        [HttpPost]
        [AntiForgeryHandleError]
        public ActionResult Create(QuestionType  model)
        {

            if (ModelState.IsValid)
            {
                ResponseBase rsp = null;
                Company.QuestionServiceClient svcComp = null;
                try
                {
                    var token = UserAuthorization.CurrentUser;

                    svcComp = new Company.QuestionServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                    rsp = svcComp.Create(model);
                    SoundPower.Web.Notifications.AddResponseNotifications(rsp);
                }
                catch (Exception ex)
                {
                    var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.Question.Create", ex);
                    SoundPower.Web.Utilities.ReportError(bex);
                    throw bex;
                }
                finally
                {
                    if (svcComp != null) svcComp.Dispose();
                }


                if (!rsp.IsFailure)
                {
                    SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.QuestionPages.CreateSuccess);
                    return RedirectToAction("index");
                }
                else
                {
                    SiteUtilities.ApplyFieldIssues(rsp, ModelState);
                }
            }


            return View(model);
        }

        [RequireAdmin]
        [HttpPost]
        [AntiForgeryHandleError]
        public ActionResult Edit(QuestionType model)
        {

            if (ModelState.IsValid)
            {
                ResponseBase rsp = null;
                Company.QuestionServiceClient svcComp = null;
                try
                {
                    var token = UserAuthorization.CurrentUser;

                    svcComp = new Company.QuestionServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                    rsp = svcComp.Update(model);
                    SoundPower.Web.Notifications.AddResponseNotifications(rsp);
                }
                catch (Exception ex)
                {
                    var bex = new SoundPower.ErrorTracking.BaseException("Save Processing Failure", "TherapyCorner.Portal.Controllers.Question.Edit", ex);
                    SoundPower.Web.Utilities.ReportError(bex);
                    throw bex;
                }
                finally
                {
                    if (svcComp != null) svcComp.Dispose();
                }


                if (!rsp.IsFailure)
                {
                    SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.QuestionPages.UpdateSuccess);
                    return RedirectToAction("index");

                }
                else
                {
                    SiteUtilities.ApplyFieldIssues(rsp, ModelState);
                }
            }


            return View(model);
        }

        [RequireAdmin]
        [HttpGet]
        public ActionResult Edit(int? id)
        {
            if (!id.HasValue)
            {

                SoundPower.Web.Notifications.AddErrorNotification(ResourceText.SharedPages.CouldNotProcessRelogin);
                return RedirectToAction("index");

            }

            QuestionTypeResponse rsp = null;
            Company.QuestionServiceClient svcComp = null;
            try
            {
                var token = UserAuthorization.CurrentUser;

                svcComp = new Company.QuestionServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                rsp = svcComp.Details(id.Value);
                SoundPower.Web.Notifications.AddResponseNotifications(rsp);
            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.Question.Edit", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {
                if (svcComp != null) svcComp.Dispose();
            }


            if (rsp.IsFailure || rsp.Question==null )
            {
                return RedirectToAction("index");

            }
            return View(rsp.Question);
        }


    }
}
