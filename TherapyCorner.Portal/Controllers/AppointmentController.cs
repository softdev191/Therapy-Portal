using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using www.therapycorner.com.account.MessageContracts;
using www.therapycorner.com.account;
using www.soundpower.biz.common;
using www.therapycorner.com.company.MessageContracts;

namespace TherapyCorner.Portal.Controllers
{
    [RequireHttps]
    [RequireUser]
    [CompanyFilter]
    [OutputCache(NoStore =true,Duration =0, VaryByParam ="*")]
    public class AppointmentController : Controller
    {
        private const string colorCancelled = "#8d9093";
        private const string colorUnapproved = "#ff0000";
        private const string colorOther = "#ff66ff";
        private const string colorNoShow = "#e600e6";

        // GET: Appointment
        public ActionResult Index()
        {
            var token = UserAuthorization.CurrentUser;
            if (!token.IsProvider && !token.IsSupervisor) return RedirectToAction("index", "home");

            return View();
        }

        [HttpGet]
        public ActionResult ScheduleUnavailable()
        {
            var token = UserAuthorization.CurrentUser;
            if (!token.IsProvider && !token.IsSupervisor) return RedirectToAction("index", "home");

            return View(new MeetingInfo() { Start = DateTime.Now, End = DateTime.Now.AddDays(1), MeetingId = 0 });
        }

        [HttpPost]
        public ActionResult ScheduleUnavailable(MeetingInfo model)
        {
            var token = UserAuthorization.CurrentUser;
            if (!token.IsProvider && !token.IsSupervisor) return RedirectToAction("index", "home");
            if (ModelState.IsValid )
            {
                Account.ScheduleServiceClient svcComp = null;
                ResponseBase rsp = null;
                try
                {

                    svcComp = new Account.ScheduleServiceClient(SiteUtilities.AccountService, token, SiteUtilities.CurrentCulture);
                    rsp = svcComp.AddUnavailability(model);
                }
                catch (Exception ex)
                {
                    var bex = new SoundPower.ErrorTracking.BaseException("Save Processing Failure", "TherapyCorner.Portal.Controllers.Appointment.ScheduleUnavailable", ex);
                    throw bex;
                }
                finally
                {
                    if (svcComp != null) svcComp.Dispose();

                }

                SoundPower.Web.Notifications.AddResponseNotifications(rsp);
                if (!rsp.IsFailure)
                {
                    SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.CalendarPages.UnavailabilityCreated);
                    return RedirectToAction("index");
                }
                else
                {
                    SiteUtilities.ApplyFieldIssues(rsp, ModelState);
                }
            }
            return View(model);
        }

        public JsonResult UserMeetings(int id ,DateTime  start, DateTime end, bool? includeCancelled)
        {
    
            JsonResult result = new JsonResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        

            www.therapycorner.com.account.MessageContracts.ScheduleResponse rsp = null;
            Account.ScheduleServiceClient svcComp = null;
                 var token = UserAuthorization.CurrentUser;
           
            try
            {

                svcComp = new Account.ScheduleServiceClient(SiteUtilities.AccountService, token, SiteUtilities.CurrentCulture);
                rsp = svcComp.Schedule(id,start,end);
            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.Appointment.UserMeetings", ex);
               rsp = new ScheduleResponse() { IsFailure = true };
                rsp.ErrorMessages.Add(bex.Message);
                result.Data = rsp;
                return result ;
            }
            finally
            {
                if (svcComp != null) svcComp.Dispose();
            }

            Models.CalendarEventList data = new Models.CalendarEventList();
            
            if (rsp.Meetings !=null && rsp.Meetings.Count>0)
            {
                if (!includeCancelled.GetValueOrDefault(false))
                {
                    rsp.Meetings.RemoveAll(m => m.Type == www.therapycorner.com.company.AppointmentStatusEnum.Cancelled.ToString() || m.Type == www.therapycorner.com.company.AppointmentStatusEnum.NoShow.ToString());
                }
                bool isCancelled = false;
                foreach(var m in rsp.Meetings)
                {
                    var meet = new Models.CalendarEvent()
                    {
                        start = DateTime.SpecifyKind(m.Start, DateTimeKind.Utc),
                        end = DateTime.SpecifyKind(m.End, DateTimeKind.Utc),
                        id=m.MeetingId.ToString() 
                    };
                    isCancelled = m.Type == www.therapycorner.com.company.AppointmentStatusEnum.Cancelled.ToString() || m.Type == www.therapycorner.com.company.AppointmentStatusEnum.NoShow.ToString();
                    if (token.UserId != id)
                    {
                        if (m.Type != token.CurrentCompany && m.Type != "Appointment" && !isCancelled)
                        {
                            if (isCancelled) continue;
                            meet.title = ResourceText.Dictionary.Unavailable;
                            meet.backgroundColor = colorOther;
                        }
                        else
                        {
                            meet.title = m.Title;
                            meet.url = Url.Action("details", new { id = m.MeetingId });
                            if (!isCancelled && m.Start > DateTime.Now.AddDays(-2) && (token.IsAdmin || token.IsWorker))
                    {
                                meet.startEditable =true;
                            }
                            meet.isAppointment = true;
                            if (isCancelled )
                            {
                                meet.backgroundColor = m.Type == www.therapycorner.com.company.AppointmentStatusEnum.Cancelled.ToString() ? colorCancelled : colorNoShow;
                            }
                            else if (!m.Approved.GetValueOrDefault(false))
                            {
                                meet.backgroundColor = colorUnapproved;
                            }

                        }
                    }
                    else if (m.Type != "N/A" && m.Type != token.CurrentCompany && m.Type != "Appointment" && !isCancelled)
                    {
                        var c = token.Companies.Find(t => t.UniqueId == m.Type);
                        meet.title = c.Name;
                        meet.backgroundColor = colorOther;

                    }
                    else if (m.Type == "N/A")
                    {
                        meet.title = m.Title;
                        meet.backgroundColor = colorOther;
                        if (m.Start > DateTime.Now.AddDays(-2))
                        {
                            meet.startEditable = true;
                            meet.url = Url.Action("editunavailable", new { id = m.MeetingId, dy = m.Start });
                        }

                    }
                    else if (m.Type == token.CurrentCompany || m.Type == "Appointment" || isCancelled)
                    {
                        meet.title = m.Title;
                        meet.isAppointment = true;
                        meet.url = Url.Action("details", new { id = m.MeetingId });
                        if (m.Start > DateTime.Now.AddDays(-2))
                        {
                            meet.startEditable = true;
                        }
                        meet.isAppointment = true;
                        if (isCancelled)
                        {
                            meet.backgroundColor = m.Type == www.therapycorner.com.company.AppointmentStatusEnum.Cancelled.ToString() ? colorCancelled : colorNoShow;
                        }
                        else if (!m.Approved.GetValueOrDefault(false))
                        {
                            meet.backgroundColor = colorUnapproved;
                        }

                    }
                    data.Add(meet);

                }
            }
            MakeAllUTC(data);

            result.Data = data;
            
            return result;
        }

        public JsonResult Availability(int id,int staff, DateTime start, int count)
        {

            JsonResult result = new JsonResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            string partialAvail = "#dddde3";
            string noAvail = "#c2c2c3";

            DateTime init = start.AddDays(-1 * (int)start.DayOfWeek).Date;

            www.therapycorner.com.account.MessageContracts.ScheduleResponse rsp = null;
            Account.ScheduleServiceClient svcComp = null;
            Company.AppointmentServiceClient svcClient = null;
            Company.StaffServiceClient svcStaff = null;
            var token = UserAuthorization.CurrentUser;
            www.therapycorner.com.company.MessageContracts.AppointmentInfoListResponse rspClient = null;
            try
            {
                svcClient = new Company.AppointmentServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                rspClient = svcClient.ClientSchedule(id, init, init.AddDays(count * 7));
                rspClient.Appointments.RemoveAll(a => a.Status == www.therapycorner.com.company.AppointmentStatusEnum.Cancelled || a.Status == www.therapycorner.com.company.AppointmentStatusEnum.NoShow);
                svcStaff = new Company.StaffServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                var rspStaff = svcStaff.Details(staff);

                if (rspStaff.Staff.User!=null)
                {
                    svcComp = new Account.ScheduleServiceClient(SiteUtilities.AccountService, token, SiteUtilities.CurrentCulture);
                    rsp = svcComp.Schedule(int.Parse(rspStaff.Staff.User.UniqueId), init, init.AddDays(count * 7));

                    //break up meetings longer than 1 week
                    var longMeets = rsp.Meetings.FindAll(m => m.End.Subtract(m.Start).TotalDays > 7);
                    if (longMeets.Count > 0)
                    {
                        while (longMeets.Count > 0)
                        {

                            foreach (var m in longMeets)
                            {
                                rsp.Meetings.Add(new MeetingInfo()
                                {
                                    End = m.End,
                                    MeetingId = m.MeetingId,
                                    Start = m.Start.AddDays(7),
                                    Title = m.Title,
                                    Type = m.Type
                                });
                                m.End = m.Start.AddDays(7);
                            }

                            longMeets = rsp.Meetings.FindAll(m => m.End.Subtract(m.Start).TotalDays > 7);
                        }
                    }

                    //Break up multi-week spanning meetings
                    var multiWeeks = rsp.Meetings.FindAll(m => (int)m.End.DayOfWeek < (int)m.Start.DayOfWeek);
                    if (multiWeeks.Count > 0)
                    {
 
                            foreach (var m in multiWeeks)
                            {
                            var midpoint = m.Start.AddDays(7 - (int)m.Start.DayOfWeek);
                                rsp.Meetings.Add(new MeetingInfo()
                                {
                                    End = m.End,
                                    MeetingId = m.MeetingId,
                                    Start = midpoint,
                                    Title = m.Title,
                                    Type = m.Type
                                });
                                m.End = midpoint;
                            }

                    }
                }
                else
                {
                    rsp = new ScheduleResponse() { Availabilities = new AvailabilityList(), Meetings = new MeetingInfoList() };
                    for (int i=1;i<6;i++)
                    {
                        rsp.Availabilities.Add(new www.therapycorner.com.account.Availability() { AvailabilityId = i, CompanyId = token.CurrentCompany, Day = i, StartTime = new TimeSpan(7, 00, 0), EndTime = new TimeSpan(20, 00, 0) });
                    }
                    var rspAppt = svcClient.Staff(staff, init, init.AddDays(count * 7));
                    rsp.Meetings.AddRange(from a in rspAppt.Appointments select a.ToMeeting(false, null));
            
                    }

            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.Appointment.Availability", ex);
                rsp = new ScheduleResponse() { IsFailure = true };
                rsp.ErrorMessages.Add(bex.Message);
                result.Data = rsp;
                return result;
            }
            finally
            {
                if (svcComp != null) svcComp.Dispose();
                if (svcClient != null) svcClient.Dispose();
                if (svcStaff != null) svcStaff.Dispose();
            }

            Models.CalendarEventList data = new Models.CalendarEventList();
            var aStart = init;
            for (int i=0;i<7;i++)
            {
                var baseDT = init.AddDays(i);
                var avails = rsp.Availabilities.FindAll(a => (int)a.DayName == i);
                if (rsp.Availabilities==null || avails.Count==0)
                {
                    data.Add(new Models.CalendarEvent()
                    {
                        backgroundColor = noAvail,
                        start=baseDT,
                        end=baseDT.AddDays(1),
                        startEditable=false,
                        id=string.Format("UA{0}",i),
                        title=""
                    });
                    continue;
                }

                avails.Sort((a, b) => a.StartTicks.CompareTo(b.StartTicks));

                aStart = baseDT;
                foreach(var a in avails)
                {
                    data.Add(new Models.CalendarEvent()
                    {
                        backgroundColor = noAvail,
                        start = aStart,
                        end = baseDT.Add(a.StartTime),
                        startEditable = false,
                        id = string.Format("UA{0}-{1}", i,a.StartTicks),
                        title = ""
                    });
                    var openStart = baseDT.Add(a.StartTime);
                    var openEnd = baseDT.Add(a.EndTime);

                    //Get staff conflicts
                    var curTime = openStart;
                    while (curTime<openEnd)
                    {
                        var conflicts = rsp.Meetings.FindAll(m => IsConflict(curTime, m));
                        if(conflicts.Count==0)
                        {
                            curTime = curTime.AddMinutes(15);

                        }
                        else
                        {
                            var openCount = count - conflicts.Count;
                            if (openCount < 0) openCount = 0;
                            var tm = new TimeSpan(openStart.Hour, openStart.Minute, 0);
                            var minTime = conflicts.Min(m => TimeOfDay(m.Start, curTime.DayOfWeek,tm,false));
                            tm = new TimeSpan(openEnd.Hour, openEnd.Minute, 0);
                            var maxTime = conflicts.Max(m => TimeOfDay(m.End, curTime.DayOfWeek, tm, true));

                            data.Add(new Models.CalendarEvent()
                            {
                                backgroundColor = openCount == 0 ? noAvail : partialAvail,
                                start = openStart.Date.Add(minTime),
                                end = openStart.Date.Add(maxTime),
                                startEditable = false,
                                id = string.Format("SC{0}-{1}", i,curTime.ToString()),
                                title = string.Format("P({0})",openCount)
                            });
                            curTime = openStart.Date.Add(maxTime);
                        }
                    }
                    //Get client conflicts
                    curTime = openStart;
                    while (curTime < openEnd)
                    {
                        var conflicts = rspClient.Appointments.FindAll(m => IsConflict(curTime, m.ToMeeting(false,null)));
                        if (conflicts.Count == 0)
                        {
                            curTime = curTime.AddMinutes(15);

                        }
                        else
                        {
                            var openCount = count - conflicts.Count;
                            if (openCount < 0) openCount = 0;
                            var tm = new TimeSpan(openStart.Hour, openStart.Minute, 0);
                            var minTime = conflicts.Min(m => TimeOfDay(m.Start, curTime.DayOfWeek, tm, false));
                            tm = new TimeSpan(openEnd.Hour, openEnd.Minute, 0);
                            var maxTime = conflicts.Max(m => TimeOfDay(m.End, curTime.DayOfWeek, tm, true));

                            data.Add(new Models.CalendarEvent()
                            {
                                backgroundColor = openCount ==0 ? noAvail:partialAvail,
                                start = openStart.Date.Add(minTime),
                                end = openStart.Date.Add(maxTime),
                                startEditable = false,
                                id = string.Format("CC{0}-{1}", i, curTime.ToString()),
                                title = string.Format("C({0})", openCount)
                            });
                            curTime = openStart.Date.Add(maxTime);
                        }
                    }
                    aStart = openEnd;
                }

                if (aStart<baseDT.AddDays(1) )
                {
                    data.Add(new Models.CalendarEvent()
                    {
                        backgroundColor = noAvail,
                        start = aStart,
                        end = baseDT.AddDays(1),
                        startEditable = false,
                        id = string.Format("UA{0}-{1}", i,"End"),
                        title = ""
                    });
                }
            }

            MakeAllUTC(data);
            result.Data = data;

            return result;
        }

        private static bool IsConflict(DateTime timePoint, MeetingInfo meeting)
        {
            if (timePoint.DayOfWeek < meeting.Start.DayOfWeek || timePoint.DayOfWeek > meeting.End.DayOfWeek) return false;
            if (timePoint.Hour>meeting.Start.Hour || (timePoint.Hour==meeting.Start.Hour && timePoint.Minute>=meeting.Start.Minute ))
            {
                if (timePoint.Hour < meeting.End.Hour || (timePoint.Hour == meeting.End.Hour && timePoint.Minute < meeting.End.Minute))
                {
                    return true;
                }
            }

            return false;
        }

        private static void MakeAllUTC(Models.CalendarEventList events)
        {
            foreach(var e in events)
            {
                e.start = DateTime.SpecifyKind(e.start, DateTimeKind.Utc);
                e.end = DateTime.SpecifyKind(e.end, DateTimeKind.Utc);

            }
        }
        private static TimeSpan TimeOfDay(DateTime dt, DayOfWeek day, TimeSpan defaultTime, bool forMax)
        {
            if (dt.DayOfWeek != day) return defaultTime;

            var thisTime= new TimeSpan(dt.Hour, dt.Minute, 0);
            if (forMax && thisTime > defaultTime) return defaultTime;
            if (!forMax && thisTime < defaultTime) return defaultTime;
            return thisTime;
        }
        public JsonResult StaffMeetings(int id, DateTime start, DateTime end, bool? includeCancelled)
        {

            JsonResult result = new JsonResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet };


            www.therapycorner.com.company.MessageContracts.AppointmentInfoListResponse rsp = null;
            Company.AppointmentServiceClient  svcComp = null;
            var token = UserAuthorization.CurrentUser;

            try
            {

                svcComp = new Company.AppointmentServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                rsp = svcComp.Staff(id, start, end);
            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.Appointment.StaffMeetings", ex);
                rsp = new www.therapycorner.com.company.MessageContracts.AppointmentInfoListResponse() { IsFailure = true };
                rsp.ErrorMessages.Add(bex.Message);
                result.Data = rsp;
                return result;
            }
            finally
            {
                if (svcComp != null) svcComp.Dispose();
            }

            Models.CalendarEventList data = new Models.CalendarEventList();
            if (!includeCancelled.GetValueOrDefault(false) && rsp.Appointments != null) rsp.Appointments.RemoveAll(a => a.Status == www.therapycorner.com.company.AppointmentStatusEnum.Cancelled || a.Status == www.therapycorner.com.company.AppointmentStatusEnum.NoShow);

            if (rsp.Appointments  != null && rsp.Appointments.Count > 0)
            {
                rsp.Appointments.RemoveAll(m => m.Status == www.therapycorner.com.company.AppointmentStatusEnum.Cancelled || m.Status == www.therapycorner.com.company.AppointmentStatusEnum.NoShow);
                foreach (var m in rsp.Appointments)
                {
                    
                    var meet = new Models.CalendarEvent()
                    {
                        start = DateTime.SpecifyKind(m.Start, DateTimeKind.Utc),
                        end = DateTime.SpecifyKind(m.End, DateTimeKind.Utc),
                        id = m.AppointmentId.ToString(),
                        isAppointment=true,
                        title = string.Format("{0} - {1}", m.Client.Name,m.Service.Name),
                        url = Url.Action("details", new { id = m.AppointmentId }),
                        startEditable = false
                    };
                    if (token.IsAdmin || token.IsWorker )
                    {
                        meet.startEditable = m.Status == www.therapycorner.com.company.AppointmentStatusEnum.Scheduled || m.Status == www.therapycorner.com.company.AppointmentStatusEnum.PendingNotes;
                    }
                    if (m.Status == www.therapycorner.com.company.AppointmentStatusEnum.Cancelled)
                    {
                        meet.backgroundColor = colorCancelled;
                    }
                    else if (m.Status == www.therapycorner.com.company.AppointmentStatusEnum.NoShow)
                    {
                        meet.backgroundColor = colorNoShow;

                    }
                    else if (!m.Approved.GetValueOrDefault(false))
                    {
                        meet.backgroundColor = colorUnapproved;
                    }
                    data.Add(meet);
                   
                }
            }
            MakeAllUTC(data);

            result.Data = data;

            return result;
        }

        public JsonResult ClientMeetings(int id, DateTime start, DateTime end, bool? includeCancelled)
        {

            JsonResult result = new JsonResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet };


            www.therapycorner.com.company.MessageContracts.AppointmentInfoListResponse rsp = null;
            Company.AppointmentServiceClient svcComp = null;
            var token = UserAuthorization.CurrentUser;

            try
            {

                svcComp = new Company.AppointmentServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                rsp = svcComp.ClientSchedule(id, start, end);
            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.Appointment.ClientMeetings", ex);
                rsp = new www.therapycorner.com.company.MessageContracts.AppointmentInfoListResponse() { IsFailure = true };
                rsp.ErrorMessages.Add(bex.Message);
                result.Data = rsp;
                return result;
            }
            finally
            {
                if (svcComp != null) svcComp.Dispose();
            }

            Models.CalendarEventList data = new Models.CalendarEventList();
            if (!includeCancelled.GetValueOrDefault(false) && rsp.Appointments != null) rsp.Appointments.RemoveAll(a => a.Status == www.therapycorner.com.company.AppointmentStatusEnum.Cancelled || a.Status == www.therapycorner.com.company.AppointmentStatusEnum.NoShow); 
            if (rsp.Appointments != null && rsp.Appointments.Count > 0)
            {
                foreach (var m in rsp.Appointments)
                {

                    var meet = new Models.CalendarEvent()
                    {
                        start = DateTime.SpecifyKind(m.Start, DateTimeKind.Utc),
                        end = DateTime.SpecifyKind(m.End, DateTimeKind.Utc),
                        id = m.AppointmentId.ToString(),
                        isAppointment = true,
                        title = string.Format("{0} - {1}", m.Service.Name, m.Provider.Name),
                        url = Url.Action("details", new { id = m.AppointmentId }),

                        startEditable = m.Status== www.therapycorner.com.company.AppointmentStatusEnum.Scheduled || m.Status== www.therapycorner.com.company.AppointmentStatusEnum.PendingNotes
                         
                    };
                    meet.isAppointment = true;
                    if (m.Status== www.therapycorner.com.company.AppointmentStatusEnum.Cancelled )
                    {
                        meet.backgroundColor = colorCancelled;
                    }
                    else if(m.Status== www.therapycorner.com.company.AppointmentStatusEnum.NoShow)
                    {
                        meet.backgroundColor = colorNoShow;

                    }
                    else if (!m.Approved.GetValueOrDefault(false))
                    {
                        meet.backgroundColor = colorUnapproved;
                    }
                
                    data.Add(meet);

                }
            }
            MakeAllUTC(data);

            result.Data = data;

            return result;
        }

        public ActionResult Makeups()
        {
            Company.ClientServiceServiceClient svc = null;
            www.therapycorner.com.company.MessageContracts.ClientServiceListResponse rsp = null;

            try
            {
                var token = UserAuthorization.CurrentUser;
                svc = new Company.ClientServiceServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                rsp = svc.MakeupCounts();
                rsp.Services.RemoveAll(x => x.End < DateTime.Now);
            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Controllers.Appointment.Makeups", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {
                if (svc != null) svc.Dispose();
            }


            SoundPower.Web.Notifications.AddResponseNotifications(rsp);
            if (rsp.IsFailure) return (RedirectToAction("index"));

           
            return View(rsp.Services);
        }

        [HttpGet]
        public ActionResult CreateMakeup(int? id)
        {
            if (!id.HasValue )
            {
                SoundPower.Web.Notifications.AddErrorNotification(ResourceText.SharedPages.CouldNotProcessRelogin);
                return RedirectToAction("makeups");
            }
            var token = UserAuthorization.CurrentUser;
            if (!token.IsProvider && !token.IsSupervisor) return RedirectToAction("makeups");

            Company.ClientServiceServiceClient svc = null;
            ClientServiceResponse rsp = null;
            Company.FreqDurServiceClient svcDur = null;
            try
            {


                svc = new Company.ClientServiceServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                rsp = svc.Details(id.Value);

                ViewBag.Service = rsp.Service;
                svcDur = new Company.FreqDurServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                var rspDurs = svcDur.List(false);

                ViewBag.DurationTime = rspDurs.Durations.Find(f => f.FreqDurId.ToString() == rsp.Service.Duration.UniqueId).Duration;
            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.Appointment.CreateMakeup", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {
                if (svc != null) svc.Dispose();
                if (svcDur != null) svcDur.Dispose();

            }

            if (rsp.Service.Provider.AlternateId != token.User.UniqueId)
            {
                SoundPower.Web.Notifications.AddErrorNotification(ResourceText.Messages.AccessDenied);
                return RedirectToAction("makeups");

            }
            return View(new CreateAppointmentRequest() { ClientServiceId = id.Value , InitialDate = DateTime.Today.AddDays(1), MeetingCount = 1, StartTime = new TimeSpan(9, 0, 0) });
        }

        [HttpPost]
        [AntiForgeryHandleError]
        public ActionResult CreateMakeup(CreateAppointmentRequest request)
        {
                    var token = UserAuthorization.CurrentUser;
    
            if (ModelState.IsValid)
            {
                Company.AppointmentServiceClient svcStaff = null;
                ResponseBase rspStaff = null;
                try
                {


                    svcStaff = new Company.AppointmentServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                    rspStaff = svcStaff.CreateMakeup(request);

                }
                catch (Exception ex)
                {
                    var bex = new SoundPower.ErrorTracking.BaseException("Save Processing Failure", "TherapyCorner.Portal.Controllers.Appointment.CreateMakeup", ex);
                    SoundPower.Web.Utilities.ReportError(bex);
                    throw bex;
                }
                finally
                {
                    if (svcStaff != null) svcStaff.Dispose();

                }
                SoundPower.Web.Notifications.AddResponseNotifications(rspStaff);
                if (!rspStaff.IsFailure)
                {

                    SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.StaffPages.AppointmentsScheduled);
                    return RedirectToAction("makeups");
                }
                else
                {
                    SiteUtilities.ApplyFieldIssues(rspStaff, ModelState);
                }
            }
            Company.ClientServiceServiceClient svc = null;
            Company.FreqDurServiceClient svcDur = null;
            ClientServiceResponse rsp = null;
            try
            {
               

                svc = new Company.ClientServiceServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                rsp = svc.Details(request.ClientServiceId);
                svcDur = new Company.FreqDurServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);

                ViewBag.Service = rsp.Service;
                var rspDurs = svcDur.List(false);
           
                    ViewBag.DurationTime = rspDurs.Durations.Find(f => f.FreqDurId.ToString() == rsp.Service.Duration.UniqueId).Duration;
                
            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Validation Return Processing Failure", "TherapyCorner.Portal.Controllers.Appointment.CreateMakeup", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {
                if (svc != null) svc.Dispose();
                if (svcDur != null) svcDur.Dispose();

            }
            return View(request);
        }

        [HttpGet]
        public ActionResult EditUnavailable(long id, DateTime  dy, int? minutes)
        {
            var token = UserAuthorization.CurrentUser;
            if (!token.IsProvider && !token.IsSupervisor) return RedirectToAction("index", "home");

            Account.ScheduleServiceClient svcComp = null;
            www.therapycorner.com.account.MessageContracts.ScheduleResponse rsp = null;
            MeetingInfo model = null;
            try
            {

                svcComp = new Account.ScheduleServiceClient(SiteUtilities.AccountService, token, SiteUtilities.CurrentCulture);
                rsp = svcComp.Schedule(token.UserId, dy.AddDays(-1), dy.AddDays(1));
            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Save Processing Failure", "TherapyCorner.Portal.Controllers.Appointment.EditUnavailable", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {
                if (svcComp != null) svcComp.Dispose();

            }
            SoundPower.Web.Notifications.AddResponseNotifications(rsp);
            if (rsp.Meetings != null) model = rsp.Meetings.Find(m => m.Type == "N/A" && m.MeetingId == id);
if (rsp.IsFailure || model==null)
            {
                return RedirectToAction("index", new { id = token.UserId });
            }

if (minutes.HasValue)
            {
              model.Start=  model.Start.AddSeconds(minutes.Value);
              model.End=  model.End.AddSeconds(minutes.Value);
            }
            return View(model);
        }

        [HttpPost]
        [AntiForgeryHandleError]
        public ActionResult EditUnavailable(MeetingInfo model)
        {
            var token = UserAuthorization.CurrentUser;
            if (!token.IsProvider && !token.IsSupervisor) return RedirectToAction("index", "home");
            if (ModelState.IsValid)
            {
                Account.ScheduleServiceClient svcComp = null;
                ResponseBase rsp = null;
                try
                {

                    svcComp = new Account.ScheduleServiceClient(SiteUtilities.AccountService, token, SiteUtilities.CurrentCulture);
                    rsp = svcComp.UpdateUnavailability(model);
                }
                catch (Exception ex)
                {
                    var bex = new SoundPower.ErrorTracking.BaseException("Save Processing Failure", "TherapyCorner.Portal.Controllers.Appointment.EditUnavailable", ex);
                    SoundPower.Web.Utilities.ReportError(bex);
                    throw bex;
                }
                finally
                {
                    if (svcComp != null) svcComp.Dispose();

                }

                SoundPower.Web.Notifications.AddResponseNotifications(rsp);
                if (!rsp.IsFailure)
                {
                    SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.CalendarPages.UnavailabilityUpdated);
                    return RedirectToAction("index");
                }
                else
                {
                    SiteUtilities.ApplyFieldIssues(rsp, ModelState);
                }
            }
            return View(model);
        }

        [HttpPost]
        [AntiForgeryHandleError]
        public ActionResult RemoveUnavailable(long id)
        {
            var token = UserAuthorization.CurrentUser;
            if (!token.IsProvider && !token.IsSupervisor) return RedirectToAction("index", "home");

                Account.ScheduleServiceClient svcComp = null;
                ResponseBase rsp = null;
                try
                {

                    svcComp = new Account.ScheduleServiceClient(SiteUtilities.AccountService, token, SiteUtilities.CurrentCulture);
                    rsp = svcComp.RemoveUnAvailability(id);
                }
                catch (Exception ex)
                {
                    var bex = new SoundPower.ErrorTracking.BaseException("Save Processing Failure", "TherapyCorner.Portal.Controllers.Appointment.RemoveUnavailable", ex);
                    SoundPower.Web.Utilities.ReportError(bex);
                    throw bex;
                }
                finally
                {
                    if (svcComp != null) svcComp.Dispose();

                }

                SoundPower.Web.Notifications.AddResponseNotifications(rsp);
                if (!rsp.IsFailure)
                {
                    SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.CalendarPages.UnavailabilityRemoved);
                }

            return RedirectToAction("index");

        }

        [HttpGet]
        public ActionResult RemoveUnavailable()
        {
            SoundPower.Web.Notifications.AddErrorNotification(ResourceText.SharedPages.CouldNotProcessRelogin);
            return RedirectToAction("index");
        }

        [HttpGet]
        public ActionResult Details(long id)
        {
            var token = UserAuthorization.CurrentUser;

            Company.AppointmentServiceClient svcComp = null;
            Company.ClientServiceClient svcClient = null;
            Account.CompanyServiceClient svcAcct = null;
            Account.UserServiceClient svcUser = null;
            www.therapycorner.com.company.MessageContracts.AppointmentInfoResponse rsp = null;
            bool enableNotes = true;
            try
            {

                svcComp = new Company.AppointmentServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                rsp = svcComp.Details(id);
                if(rsp.Appointment!=null)
                {
                    switch(rsp.Appointment.Location.GetValueOrDefault())
                    {
                        case www.therapycorner.com.company.LocationEnum.ClientHome:
                            svcClient = new Company.ClientServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                            var rspClient = svcClient.Details(int.Parse(rsp.Appointment.Client.UniqueId));
                            ViewBag.Address = rspClient.Client.Address;
                            break;
                        case www.therapycorner.com.company.LocationEnum.Clinic:
                            svcAcct = new Account.CompanyServiceClient(SiteUtilities.AccountService, token, SiteUtilities.CurrentCulture);
                            var rspAccount = svcAcct.Details(token.CurrentCompany);
                            ViewBag.Address = rspAccount.Company.Address;
                            break;
                        case www.therapycorner.com.company.LocationEnum.ProviderHome:
                            if(string.IsNullOrWhiteSpace(rsp.Appointment.Provider.AlternateId))
                            {
                                ViewBag.Address = new www.therapycorner.com.account.AddressInfo();
                            }
                            else
                            {
                                svcUser = new Account.UserServiceClient(SiteUtilities.AccountService, token, SiteUtilities.CurrentCulture);
                                var rspUser = svcUser.Details(int.Parse(rsp.Appointment.Provider.AlternateId));
                                ViewBag.Address = rspUser.User.Address;
                            }
                           
                            break;
                    }
                    if (rsp.Appointment.Series!=null)
                    {
                        string bse = rsp.Appointment.Series.WeeksBetween <= 1 ? ResourceText.CalendarPages.PartOfWeeklySeries : ResourceText.CalendarPages.PartOfSeries;
                        SoundPower.Web.Notifications.AddInformationNotification(string.Format(bse, rsp.Appointment.Series.DayOfWeekName, rsp.Appointment.Series.StartTimeString, rsp.Appointment.Series.WeeksBetween));
                    }
                  
                }
            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.Appointment.Details", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {
                if (svcComp != null) svcComp.Dispose();
                if (svcClient != null) svcClient.Dispose();
                if (svcUser != null) svcUser.Dispose();
                if (svcComp != null) svcComp.Dispose();

            }
            SoundPower.Web.Notifications.AddResponseNotifications(rsp);

            if (rsp.IsFailure || rsp.Appointment == null)
            {
                return RedirectToAction("index", "home");
            }

            ViewBag.NotesEnabled = enableNotes;
            return View(rsp.Appointment );
        }


        [HttpGet]
        public ActionResult MoveOptSel(long? id, int? series, int? minutes)
        {
            if (!id.HasValue)
            {
                SoundPower.Web.Notifications.AddErrorNotification(ResourceText.SharedPages.CouldNotProcessRelogin);
                return RedirectToAction("index");
            }
            var token = UserAuthorization.CurrentUser;

            Company.AppointmentServiceClient svcComp = null;
    
            www.therapycorner.com.company.MessageContracts.AppointmentSeriesResponse rsp = null;
            try
            {
                ViewBag.AppointmentId = id.Value;
                svcComp = new Company.AppointmentServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                rsp = svcComp.Series(series.Value);
                ViewBag.Series = rsp.Series;
                ViewBag.Minutes = minutes;
                
            }
            catch (Exception ex)
            {
                Dictionary<string, string> ctx = new Dictionary<string, string>();
                ctx.Add("Series", series.GetValueOrDefault(0).ToString());
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.Appointment.MoveOptSel", ex,ctx);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {
                if (svcComp != null) svcComp.Dispose();


            }
            SoundPower.Web.Notifications.AddResponseNotifications(rsp);

            if (rsp.IsFailure || rsp.Series == null)
            {
                return RedirectToAction("index", "home");
            }

          
            return View();
        }


        [HttpPost]
        [AntiForgeryHandleError]
        public ActionResult MoveOptSel(long id, int series, bool singleOnly, string[] ids, int? minutes)
        {
            if (singleOnly) return RedirectToAction("move", new { id = id, minutes = minutes });

            //check for selected ids
            if (ids==null || ids.Length<=1)
            {
                SoundPower.Web.Notifications.AddErrorNotification(ResourceText.CalendarPages.MustSelectSomeInSeries);
                return RedirectToAction("MoveOptSel", new { id = id, minutes = minutes, series=series });
            }

            //Get Series
            var token = UserAuthorization.CurrentUser;
            
            Company.AppointmentServiceClient svcComp = null;

            www.therapycorner.com.company.MessageContracts.AppointmentSeriesResponse rsp = null;
            try
            {
                svcComp = new Company.AppointmentServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                rsp = svcComp.Series(series);


            }
            catch (Exception ex)
            {
                Dictionary<string, string> ctx = new Dictionary<string, string>();
                ctx.Add("Series", series.ToString());
                var bex = new SoundPower.ErrorTracking.BaseException("Start Move Processing Failure", "TherapyCorner.Portal.Controllers.Appointment.MoveOptSel", ex,ctx);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {
                if (svcComp != null) svcComp.Dispose();


            }
            //Begin request object - placing selected ids in place
            var req = new www.therapycorner.com.company.MessageContracts.MoveSeriesRequest();
            req.SeriesId = series;
            req.StartTime = rsp.Series.StartTime;
            req.DayOfWeek = rsp.Series.DayOfWeek;
            req.AppointmentIds = string.Join(",", ids);


            //adjust start time/day of week if there are minutes
            if (minutes.HasValue)
            {
                var newStart = rsp.Series.Appointments[0].Start.AddMinutes(minutes.Value);
                req.StartTime = new TimeSpan(newStart.Hour, newStart.Minute, 0);
                req.DayOfWeek = (int)newStart.DayOfWeek;
            }

            //Get date range to use for availability
            var appts = rsp.Series.Appointments.FindAll(a => ids.Contains(a.AppointmentId.ToString()));
            req.MinDate = appts.Min(a => a.Start).Date;
            req.MaxDate = appts.Max(a => a.End).Date;
            req.MinDate = req.MinDate.AddDays(-1 * (int)req.MinDate.DayOfWeek);
            req.MaxDate = req.MaxDate.AddDays(6-(int)req.MaxDate.DayOfWeek);
            //return view

            ViewBag.Duration = rsp.Series.Appointments[0].End.Subtract(rsp.Series.Appointments[0].Start).TotalMinutes;
            ViewBag.ClientName = rsp.Series.Client.Name;
            ViewBag.ClientId = rsp.Series.Client.UniqueId;
            ViewBag.ServiceName = rsp.Series.Service.Name;
            ViewBag.Staff = rsp.Series.Provider.UniqueId;

            var dts = from a in rsp.Series.Appointments
                      where ids.Contains(a.AppointmentId.ToString())
                      select a.Start.ToShortDateString();
            ViewBag.OrigDates = string.Join(", ", dts);
            return View("moveseries",req);
        }

        [HttpPost]
        [AntiForgeryHandleError]
        public ActionResult MoveSeries(MoveSeriesRequest request, string clientName, string clientId, string serviceName, int duration,string staff, string origDates)
        {
            ViewBag.ClientName = clientName;
            ViewBag.ClientId = clientId;
            ViewBag.ServiceName = serviceName;
            ViewBag.Staff = staff;
            ViewBag.OrigDates = origDates;
            ViewBag.Duration = duration;

            //Get Series
            var token = UserAuthorization.CurrentUser;

            Company.AppointmentServiceClient svcComp = null;
           
            ResponseBase rsp = null;
            try
            {
                svcComp = new Company.AppointmentServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                rsp = svcComp.MoveSeries(request);


            }
            catch (Exception ex)
            {
                Dictionary<string, string> ctx = new Dictionary<string, string>();
                ctx.Add("Request", Utilities.SerializeDataContractToXML(request));
                var bex = new SoundPower.ErrorTracking.BaseException("Save Processing Failure", "TherapyCorner.Portal.Controllers.Appointment.MoveSeries", ex,ctx);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {
                if (svcComp != null) svcComp.Dispose();


            }
            SoundPower.Web.Notifications.AddResponseNotifications(rsp);
            if (!rsp.IsFailure)
            {
                return RedirectToAction("details", new { id = request.AppointmentIds.Split(',')[0] });
            }




            return View( request);
        }

        [HttpGet]
        public ActionResult DragMove(long? id, int? minutes)
        {
            if (!id.HasValue)
            {

                SoundPower.Web.Notifications.AddErrorNotification(ResourceText.SharedPages.CouldNotProcessRelogin);
                return RedirectToAction("index");

            }
            var token = UserAuthorization.CurrentUser;

            Company.AppointmentServiceClient svcComp = null;

            www.therapycorner.com.company.MessageContracts.AppointmentInfoResponse rsp = null;
            www.therapycorner.com.company.AppointmentInfo model = null;
            try
            {

                svcComp = new Company.AppointmentServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                rsp = svcComp.Details(id.Value);

            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.Appointment.DragMove", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {
                if (svcComp != null) svcComp.Dispose();


            }
            SoundPower.Web.Notifications.AddResponseNotifications(rsp);
            if (rsp.Appointment != null  && !rsp.IsFailure)
            {
                if (!token.IsAdmin && !token.IsWorker && token.User.UniqueId != rsp.Appointment.Provider.AlternateId)
                {
                    rsp.IsFailure = true;
                    SoundPower.Web.Notifications.AddErrorNotification(ResourceText.Messages.AccessDenied);
                }
                else
                {
                    if (rsp.Appointment.Series==null)
                    {
                        model = rsp.Appointment;
                        ViewBag.Duration = model.End.Subtract(model.Start).TotalMinutes;
                        if (minutes.HasValue)
                        {
                            model.Start = model.Start.AddSeconds(minutes.Value);
                            model.End = model.End.AddSeconds(minutes.Value);
                        }
                        return View("move",model);
                    }
                    else
                    {
                        return RedirectToAction("MoveOptSel", new { id = id, series = rsp.Appointment.Series.SeriesId, minutes = minutes });
                    }
                }
            }

                return RedirectToAction("index", "home");
            



        }


        [HttpGet]
        public ActionResult Move(long? id,  int? minutes)
        {
            if (!id.HasValue)
            {

            SoundPower.Web.Notifications.AddErrorNotification(ResourceText.SharedPages.CouldNotProcessRelogin);
            return RedirectToAction("index");
        
    }
            var token = UserAuthorization.CurrentUser;

            Company.AppointmentServiceClient svcComp = null;

            www.therapycorner.com.company.MessageContracts.AppointmentInfoResponse rsp = null;
            www.therapycorner.com.company.AppointmentInfo  model = null;
            try
            {

                svcComp = new Company.AppointmentServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                rsp = svcComp.Details(id.Value);
              
            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.Appointment.Move", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {
                if (svcComp != null) svcComp.Dispose();


            }
            SoundPower.Web.Notifications.AddResponseNotifications(rsp);
            if (rsp.Appointment !=null)
            {
                if (!token.IsAdmin && !token.IsWorker && token.User.UniqueId != rsp.Appointment.Provider.AlternateId )
                {
                    rsp.IsFailure = true;
                    SoundPower.Web.Notifications.AddErrorNotification(ResourceText.Messages.AccessDenied);
                }
            }
            if (rsp.IsFailure || rsp.Appointment == null)
            {
                return RedirectToAction("index", "home");
            }


            model = rsp.Appointment;
            ViewBag.Duration = model.End.Subtract(model.Start).TotalMinutes;
            if (minutes.HasValue)
            {
                model.Start = model.Start.AddSeconds(minutes.Value);
                model.End = model.End.AddSeconds(minutes.Value);
            }
            return View(model);
        }

        [HttpPost]
        [AntiForgeryHandleError]
        public ActionResult Move(www.therapycorner.com.company.AppointmentInfo model,double duration)
        {
            ViewBag.Duration = duration;
            var token = UserAuthorization.CurrentUser;
            model.End = model.Start.AddMinutes(duration);
            if (ModelState.IsValid)
            {
                Company.AppointmentServiceClient svcComp = null;
                ResponseBase rsp = null;
                try
                {

                    svcComp = new Company.AppointmentServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                    rsp = svcComp.Update(model);
                }
                catch (Exception ex)
                {
                    var bex = new SoundPower.ErrorTracking.BaseException("Save Processing Failure", "TherapyCorner.Portal.Controllers.Appointment.Move", ex);
                    SoundPower.Web.Utilities.ReportError(bex);
                    throw bex;
                }
                finally
                {
                    if (svcComp != null) svcComp.Dispose();

                }

                SoundPower.Web.Notifications.AddResponseNotifications(rsp);
                if (!rsp.IsFailure)
                {
                    SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.CalendarPages.MoveSuccessful);
                    return RedirectToAction("details",new { id = model.AppointmentId });
                }
                else
                {
                    SiteUtilities.ApplyFieldIssues(rsp, ModelState);
                }
            }
            return View(model);
        }

        [HttpGet]
        public ActionResult Cancel(long? id)
        {
            if (!id.HasValue )
            {

            SoundPower.Web.Notifications.AddErrorNotification(ResourceText.SharedPages.CouldNotProcessRelogin);
            return RedirectToAction("index");
        
    }
            var token = UserAuthorization.CurrentUser;

            Company.AppointmentServiceClient svcComp = null;

            www.therapycorner.com.company.MessageContracts.AppointmentInfoResponse rsp = null;
            www.therapycorner.com.company.AppointmentInfo model = null;
            try
            {

                svcComp = new Company.AppointmentServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                rsp = svcComp.Details(id.Value );

            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.Appointment.Move", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {
                if (svcComp != null) svcComp.Dispose();


            }
            SoundPower.Web.Notifications.AddResponseNotifications(rsp);

            if (rsp.IsFailure || rsp.Appointment == null)
            {
                return RedirectToAction("index", "home");
            }


            model = rsp.Appointment;
         
            return View(model);
        }

        [HttpPost]
        [AntiForgeryHandleError]
        public ActionResult Cancel(www.therapycorner.com.company.AppointmentInfo model, bool? noShow)
        {
            model.Status = noShow.GetValueOrDefault(false) ? www.therapycorner.com.company.AppointmentStatusEnum.NoShow : www.therapycorner.com.company.AppointmentStatusEnum.Cancelled;
            var token = UserAuthorization.CurrentUser;
            if (ModelState.IsValid)
            {
                Company.AppointmentServiceClient svcComp = null;
                ResponseBase rsp = null;
                try
                {

                    svcComp = new Company.AppointmentServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                    rsp = svcComp.Cancel(model);
                }
                catch (Exception ex)
                {
                    var bex = new SoundPower.ErrorTracking.BaseException("Save Cancellation Processing Failure", "TherapyCorner.Portal.Controllers.Appointment.Cancel", ex);
                    SoundPower.Web.Utilities.ReportError(bex);
                    throw bex;
                }
                finally
                {
                    if (svcComp != null) svcComp.Dispose();

                }

                SoundPower.Web.Notifications.AddResponseNotifications(rsp);
                if (!rsp.IsFailure)
                {
                    SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.CalendarPages.CancelSuccessful);
                    return RedirectToAction("details", new { id = model.AppointmentId });
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