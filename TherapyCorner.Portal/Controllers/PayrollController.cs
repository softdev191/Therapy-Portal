using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace TherapyCorner.Portal.Controllers
{
    [RequireHttps]
    [RequireUser]
    [CompanyFilter]
    public class PayrollController : Controller
    {
        // GET: Payroll
        [HttpGet]
        public ActionResult Index()
        {

            return Index(new www.therapycorner.com.company.MessageContracts.PayrollSearchRequest() { Status = www.therapycorner.com.company.MessageContracts.PayrollSearchRequest.STATUS_PENDING });
        }
        [HttpPost]
        public ActionResult Index(www.therapycorner.com.company.MessageContracts.PayrollSearchRequest request)
        {
            Company.PayrollServiceClient svcPay = null;

            List<Models.PayrollGroup> pays = new List<Models.PayrollGroup>();
            ViewBag.Request = request;
            try
            {
                var token = UserAuthorization.CurrentUser;



                svcPay = new Company.PayrollServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                var rPay = svcPay.StaffList(request);
                SoundPower.Web.Notifications.AddResponseNotifications(rPay);

                if (rPay.Entries != null && rPay.Entries.Count > 0)
                {
                    var grps = from e in rPay.Entries
                               let grp = string.Format("{0}-{1}-{2}", e.SrcType.Context, e.SrcType.UniqueId, e.ForApproval)
                               group e by grp into g
                               select new { GroupKey = g.Key, Entries = g.ToList() };

                    foreach (var g in grps)
                    {
                        var i = new Models.PayrollGroup()
                        {
                            Title = string.Format("{0} {1}", g.Entries[0].SrcType.Name, g.Entries[0].ForApproval ? "(Review)" : ""),
                            Amount = g.Entries.Sum(e => e.Amount * e.Rate),
                            Children = new List<Models.PayrollGroup>()
                        };

                        var childgroups = from e in g.Entries
                                          let gs = string.Format("{0}-{1}", e.DoneOn.ToShortDateString(), e.ApprovedAt.HasValue.ToString())
                                          group e by gs into c
                                          select new { AD = c.Key, Entries = c.ToList() };
                        foreach (var c in childgroups)
                        {
                            var cg = new Models.PayrollGroup()
                            {
                                FromDT = c.Entries[0].DoneOn.Date,
                                ToDT = c.Entries[0].DoneOn.Date,
                                Count = c.Entries.Sum(e => e.Amount),
                                Amount = c.Entries.Sum(e => e.Amount * e.Rate),
                                Approved = c.Entries[0].ApprovedAt.HasValue
                            };
                            cg.Children = new List<Models.PayrollGroup>();
                            cg.Children.AddRange(from pc in c.Entries
                                                 select new Models.PayrollGroup()
                                                 {
                                                     FromDT = pc.DoneOn,
                                                     ToDT = pc.DoneOn,
                                                     Title = pc.Client.Name
                                                 });
                            i.Children.Add(cg);

                        }

                        pays.Add(i);
                    }
                }
            }
            catch (Exception ex)
            {
                var cntx = new Dictionary<string, string>();
                cntx.Add("Request", www.soundpower.biz.common.Utilities.SerializeDataContractToXML(request));
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Controllers.Payroll.Index", ex, cntx);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {
                if (svcPay != null) svcPay.Dispose();


            }
            return View(pays);
        }

        [RequireAdmin]
        public ActionResult Approval(DateTime? toDT)
        {
            Company.PayrollServiceClient svcPay = null;

            List<Models.PayrollGroup> pays = new List<Models.PayrollGroup>();
            var mstTimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Mountain Standard Time");
            var hourOffset = mstTimeZoneInfo.BaseUtcOffset.Hours;
            ViewBag.ToDT = toDT;
            try
            {
                var token = UserAuthorization.CurrentUser;



                svcPay = new Company.PayrollServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                var rPay = svcPay.PendingList();
                if (toDT.HasValue)
                {
                    var mx = toDT.Value.AddHours(24 + (hourOffset * -1)).AddMinutes(-1); // make it 6:59 AM of Next day. Ref TC #167
                    rPay.Entries.RemoveAll(e => e.DoneOn >= mx);
                }
                if (rPay.Entries != null && rPay.Entries.Count > 0)
                {
                    var stff = from e in rPay.Entries
                               group e by e.Staff.UniqueId into g
                               select new { GroupKey = g.Key, Entries = g.ToList() };
                    foreach (var s in stff)
                    {
                        var grps = from e in s.Entries
                                   let grp = string.Format("{0}-{1}-{2}-{3}", e.SrcType.Context, e.SrcType.UniqueId, e.ForApproval, e.DoneOn.ToShortDateString())
                                   group e by grp into g
                                   select new { GroupKey = g.Key, Entries = g.ToList() };

                        var si = new Models.PayrollGroup()
                        {
                            FromDT = s.Entries[0].DoneOn.AddHours(hourOffset).Date, // Converted to AZ time zone Ref: TC #167 
                            Title = "",
                            StaffMember = s.Entries[0].Staff,
                            Count = s.Entries.Sum(e => e.Amount),
                            Amount = s.Entries.Sum(e => e.Amount * e.Rate),
                            CanPay = s.Entries[0].CanPay.GetValueOrDefault(false),
                            Children = new List<Models.PayrollGroup>()
                        };

                        foreach (var g in grps)
                        {
                            var i = new Models.PayrollGroup()
                            {
                                FromDT = g.Entries[0].DoneOn.AddHours(hourOffset).Date, // Converted to AZ time zone Ref: TC #167 
                                Title = string.Format("{0} {1}", g.Entries[0].SrcType.Name, g.Entries[0].ForApproval ? "(Review)" : ""),
                                Count = g.Entries.Sum(e => e.Amount),
                                Amount = g.Entries.Sum(e => e.Amount * e.Rate),
                                Ids = string.Join(",", from e in g.Entries select e.EntryId),
                                CanPay = !g.Entries.Exists(e => !e.CanPay.GetValueOrDefault(false))
                            };



                            si.Children.Add(i);
                        }
                        pays.Add(si);
                    }

                }
            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Controllers.Payroll.Approval", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {
                if (svcPay != null) svcPay.Dispose();


            }
            return View(pays);
        }


        public ActionResult Approve(string[] ids)
        {
            if (ids == null || ids.Length == 0)
            {
                SoundPower.Web.Notifications.AddErrorNotification(ResourceText.PayrollPages.NoneSelected);
            }
            else
            {
                Company.PayrollServiceClient svcPay = null;



                try
                {
                    var token = UserAuthorization.CurrentUser;



                    svcPay = new Company.PayrollServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                    var rPay = svcPay.Approve(new www.soundpower.biz.common.ObjectIdRequestBase(string.Join(",", ids)));
                    SoundPower.Web.Notifications.AddResponseNotifications(rPay);


                }
                catch (Exception ex)
                {
                    var bex = new SoundPower.ErrorTracking.BaseException("Save Processing Failure", "TherapyCorner.Controllers.Payroll.Approve", ex);
                    SoundPower.Web.Utilities.ReportError(bex);
                    throw bex;
                }
                finally
                {
                    if (svcPay != null) svcPay.Dispose();


                }
            }
            return RedirectToAction("approval");
        }
    }
}