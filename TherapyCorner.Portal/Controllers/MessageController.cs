using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using www.therapycorner.com.company.MessageContracts;
using www.therapycorner.com.company;
using www.soundpower.biz.common;

namespace TherapyCorner.Portal.Controllers
{
    [RequireHttps]
    [RequireUser]
    [CompanyFilter]
    public class MessageController : Controller
    {
        private readonly int hoursOffset = 0;
        public MessageController()
        {
            var mstTimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Mountain Standard Time");
            hoursOffset = mstTimeZoneInfo.BaseUtcOffset.Hours;
        }

        // GET: Message
        public ActionResult Index()
        {
            Company.MessageClientService svcMsg= null;

            MessageInfoListResponse rsp = null;
            try
            {
                var token = UserAuthorization.CurrentUser;


                svcMsg = new Company.MessageClientService(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                rsp = svcMsg.List(false);
                if (rsp.Messages.Count > 0) rsp.Messages.Sort((a, b) => b.SentAt.CompareTo(a.SentAt));

            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.Message.Index", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {
                if (svcMsg != null) svcMsg.Dispose();

            }

            SoundPower.Web.Notifications.AddResponseNotifications(rsp);
            if (rsp.IsFailure) return RedirectToAction("index", "home");

            rsp.Messages.ForEach(x =>
            {
                x.SentAt = x.SentAt.AddHours(hoursOffset);
            });

            return View(rsp.Messages);
        }

        public ActionResult Details(long id)
        {
            Company.MessageClientService svcMsg = null;

            MessageInfoResponse rsp = null;
            try
            {
                var token = UserAuthorization.CurrentUser;


                svcMsg = new Company.MessageClientService(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                rsp = svcMsg.Details(id);
                UserAuthorization.SetMessageCount();

            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.Message.Details", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {
                if (svcMsg != null) svcMsg.Dispose();

            }

            SoundPower.Web.Notifications.AddResponseNotifications(rsp);
            if (rsp.IsFailure) return RedirectToAction("index");

            rsp.Message.SentAt =  rsp.Message.SentAt.AddHours(hoursOffset);

            return View(rsp.Message);
        }


        public ActionResult SentDetails(long id)
        {
            Company.MessageClientService svcMsg = null;

            MessageInfoResponse rsp = null;
            try
            {
                var token = UserAuthorization.CurrentUser;


                svcMsg = new Company.MessageClientService(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                rsp = svcMsg.Details(id);

            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.Message.Details", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {
                if (svcMsg != null) svcMsg.Dispose();

            }

            SoundPower.Web.Notifications.AddResponseNotifications(rsp);
            if (rsp.IsFailure) return RedirectToAction("index");

            rsp.Message.SentAt = rsp.Message.SentAt.AddHours(hoursOffset);

            return View(rsp.Message);
        }

        public ActionResult Sent()
        {
            Company.MessageClientService svcMsg = null;

            MessageInfoListResponse rsp = null;
            try
            {
                var token = UserAuthorization.CurrentUser;


                svcMsg = new Company.MessageClientService(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                rsp = svcMsg.List(true);
                if (rsp.Messages.Count > 0) rsp.Messages.Sort((a, b) => b.SentAt.CompareTo(a.SentAt));
            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.Message.Sent", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {
                if (svcMsg != null) svcMsg.Dispose();

            }

            SoundPower.Web.Notifications.AddResponseNotifications(rsp);
            if (rsp.IsFailure) return RedirectToAction("index", "home");

            rsp.Messages.ForEach(x =>
            {
                x.SentAt = x.SentAt.AddHours(hoursOffset);
            });

            return View(rsp.Messages);
        }

        [HttpGet]
        public ActionResult Create(long? forwardId, long? replyId, bool? includeAll)
        {
            string staffId;
            staffId = PrepNewMessage();
            MessageInfo original = null;
            string subject = "";
            string body = "";
                        List<string> recips = new List<string>();
            if (forwardId.HasValue || replyId.HasValue)
            {
                Company.MessageClientService svcMsg = null;
                long id = forwardId.HasValue ? forwardId.Value : replyId.Value;
              
                MessageInfoResponse rsp = null;
                try
                {
                    var token = UserAuthorization.CurrentUser;


                    svcMsg = new Company.MessageClientService(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                    rsp = svcMsg.Details(id);
                    original = rsp.Message;
                    subject = string.Format("{0}: {1}", forwardId.HasValue ? "FWD" : "RE", original.Subject);
                    
                    body = string.Format("<p>&nbsp;</p><hr><p align=\"center\"><strong><em>Original Message</em></strong></p>{0}", original.Contents);
                    if (replyId.HasValue)
                    {
                      if (body.Length > 2550) body = body.Substring(0, 2550);
                      recips.Add(original.SentBy.UniqueId);
                        if(includeAll.GetValueOrDefault(false))
                        {
                            recips.AddRange(from r in original.Recipients
                                            where r.UniqueId.ToString() != staffId
                                            select r.UniqueId.ToString());
                        }
                    }
                }
                catch (Exception ex)
                {
                    var bex = new SoundPower.ErrorTracking.BaseException("Original Message Processing Failure", "TherapyCorner.Portal.Controllers.Message.Create", ex);
                    SoundPower.Web.Utilities.ReportError(bex);
                    throw bex;
                }
                finally
                {
                    if (svcMsg != null) svcMsg.Dispose();

                }
            }

            return View(new Models.CreateMessageModel()
            {
                Subject =subject,
                Contents=body,
                MessageId = -1,
                SentAt = DateTime.UtcNow,
                SentBy = new GenericEntity(staffId, "Staff", null),
                IsReply = (replyId.HasValue ? true : forwardId.HasValue ? (bool?)false:null),
                OriginalMessage =original,
                RecipientIds = string.Join(",", recips)
            });
        }

        [HttpPost]
        [AntiForgeryHandleError]
        public ActionResult Create(Models.CreateMessageModel model)
        {
            if(string.IsNullOrWhiteSpace(model.RecipientIds))
            {
                SoundPower.Web.Notifications.AddErrorNotification(ResourceText.MessagePages.MustHaveRecipient);
            }
            else if(ModelState.IsValid)
            {
                                Company.MessageClientService svcMsg = null;
                model.Contents = model.MessageBody;
                ResponseBase rsp = null;
                try
                {
                    var token = UserAuthorization.CurrentUser;

                    svcMsg = new Company.MessageClientService(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                    rsp = svcMsg.Create(model.ToBase());

                }
                catch (Exception ex)
                {
                    var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.Message.Remove", ex);
                    SoundPower.Web.Utilities.ReportError(bex);
                    throw bex;
                }
                finally
                {
                    if (svcMsg != null) svcMsg.Dispose();

                }

                if (rsp.IsFailure)
                {
                    SiteUtilities.ApplyFieldIssues(rsp, ModelState);
                }
                else
                {
                    return RedirectToAction("sent");
                }
            }

             PrepNewMessage();
            return View(model);
        }
        private string PrepNewMessage()
        {
            string staffId;
            Company.StaffServiceClient svcMsg = null;
            try
            {
                var token = UserAuthorization.CurrentUser;


                svcMsg = new Company.StaffServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                var rsp = svcMsg.Search(new StaffSearchRequest() { Status = StaffSearchRequest.StatusEnum.NotInactiveOnly });
                staffId = rsp.Staff.Find(s => s.User != null && s.User.UniqueId == token.User.UniqueId).StaffId.ToString();
                rsp.Staff.RemoveAll(s => s.User != null && s.User.UniqueId == token.User.UniqueId);
                rsp.Staff.Sort((a, b) => a.DisplayName.CompareTo(b.DisplayName));
                if (rsp.Staff.Count == 0)
                {
                    SoundPower.Web.Notifications.AddErrorNotification(ResourceText.MessagePages.CannotSendMessage);
                }
                ViewBag.Staff = rsp.Staff;
            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.Message.PrepNewMessage", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {
                if (svcMsg != null) svcMsg.Dispose();

            }

            return staffId;
        }

        [HttpPost]
        [AntiForgeryHandleError]
        public ActionResult Remove(string deleteids)
        {
            if (string.IsNullOrWhiteSpace(deleteids)) return RedirectToAction("index");

            Company.MessageClientService svcMsg = null;

            try
            {
                var token = UserAuthorization.CurrentUser;
                long[] ids = (from i in deleteids.Split(',') select long.Parse(i)).ToArray();

                svcMsg = new Company.MessageClientService(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                foreach (var id in ids)
                {
                    var rsp = svcMsg.Remove(id);
                    SoundPower.Web.Notifications.AddResponseNotifications(rsp);
                }
                UserAuthorization.SetMessageCount();

            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.Message.Remove", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {
                if (svcMsg != null) svcMsg.Dispose();

            }

          

   
            return RedirectToAction("index");
        }

        [HttpPost]
        [AntiForgeryHandleError]
        public ActionResult RemoveSent(string deleteids)
        {
            if (string.IsNullOrWhiteSpace(deleteids)) return RedirectToAction("sent");
            Company.MessageClientService svcMsg = null;

            try
            {
                var token = UserAuthorization.CurrentUser;
                long[] ids = (from i in deleteids.Split(',') select long.Parse(i)).ToArray();

                svcMsg = new Company.MessageClientService(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                foreach (var id in ids)
                {
                    var rsp = svcMsg.RemoveSent(id);
                    SoundPower.Web.Notifications.AddResponseNotifications(rsp);
                }

            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.Message.RemoveSent", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {
                if (svcMsg != null) svcMsg.Dispose();

            }




            return RedirectToAction("sent");
        }
    }
}