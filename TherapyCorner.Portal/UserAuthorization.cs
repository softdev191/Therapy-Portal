using SoundPower.ErrorTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using www.therapycorner.com.account;
using www.therapycorner.com.account.MessageContracts;

namespace TherapyCorner.Portal
{
    public class UserAuthorization
    {

        internal static Session SetToken(string userId, bool forceRefresh=false)
        {
            if (string.IsNullOrWhiteSpace(userId )) return null;
            var cacheKey = "TC_USER_" + userId.ToString();
            var cachedClaims = System.Web.HttpContext.Current.Cache[cacheKey] as Session;
            if (cachedClaims == null || forceRefresh)
            {
                Session tkn = GetToken(userId);
                if (tkn != null)
                {
                    if (forceRefresh) System.Web.HttpContext.Current.Cache.Remove(cacheKey);

                    System.Web.HttpContext.Current.Cache.Add(cacheKey, tkn, null, DateTime.Now.AddSeconds(30), System.Web.Caching.Cache.NoSlidingExpiration, System.Web.Caching.CacheItemPriority.Normal, null);

                    cachedClaims = tkn;
                }
            }
            try
            {
                System.Web.HttpContext.Current.Items["UserToken"] = cachedClaims;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return cachedClaims;
        }

        public static Session CurrentUser
        {
            get
            {
                Session r = (Session)System.Web.HttpContext.Current.Items["UserToken"];
                if (r == null )
                {
                    
                    r = SetToken(System.Web.HttpContext.Current.User.Identity.Name);
                }
                else if (!string.IsNullOrWhiteSpace(r.CurrentCompany) && !r.IsAdmin && !r.IsProvider && !r.IsSupervisor && !r.IsWorker & !r.IsBiller)
                    {
                    //Reset the cached token because something is off
                    ResetCachedToken();
                    r = SetToken(System.Web.HttpContext.Current.User.Identity.Name);

                }
                return r;
            }
        }

        internal static void ResetCachedToken()
        {
            System.Web.HttpContext.Current.Items.Clear();
            var cacheKey = "TC_USER_" + System.Web.HttpContext.Current.User.Identity.Name;
            System.Web.HttpContext.Current.Cache.Remove(cacheKey);
            SetToken(System.Web.HttpContext.Current.User.Identity.Name,true);
        }

        public static int MessageCount
        {
            get
            {
                int? r = null;
                if (r == null)
                {
                    var token = CurrentUser;
                    if (token == null) return 0;
                    var cacheKey = "TC_MSGCOUNT_" + token.SessionId;
                    r = System.Web.HttpContext.Current.Cache[cacheKey] as int?;
                    if (!r.HasValue)
                    {
                        r = SetMessageCount();
                    }

                }
                return r.GetValueOrDefault(0);
            }
        }

        internal static int SetMessageCount()
        {
            var token = CurrentUser;
            if (token==null) return 0;
            var cacheKey = "TC_MSGCOUNT_" + token.SessionId;
            int? cachedClaims =null;
            if (cachedClaims == null)
            {
                if (string.IsNullOrWhiteSpace(token.CurrentCompany))
                {
                    cachedClaims = 0;
                }
                else
                {
                    Company.MessageClientService svc = null;
                    Dictionary<string, string> contextVals = new Dictionary<string, string>();
                    contextVals.Add("session", token.SessionId);

                    try
                    {
                        svc = new Company.MessageClientService(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                        var rsp = svc.NewMessageCounts(token.UserId, token.CurrentCompany);
                        SoundPower.Web.Notifications.AddResponseNotifications(rsp);
                        if (!rsp.IsFailure)
                        {
                            cachedClaims = int.Parse(rsp.ObjectId);

                        }
                    }
                    catch (Exception ex)
                    {

                        BaseException iex = new BaseException("Logged In Request Retrieval Failure", "TherapyCorner.Portal.UserAuthorization.SetMessageCount", ex, contextVals);
                        throw iex;
                    }
                    finally
                    {
                        if (svc != null) svc.Dispose();
                    }
                }
                System.Web.HttpContext.Current.Cache.Remove(cacheKey);
                System.Web.HttpContext.Current.Cache.Add(cacheKey, cachedClaims, null, DateTime.Now.AddMinutes(1), System.Web.Caching.Cache.NoSlidingExpiration, System.Web.Caching.CacheItemPriority.Normal, null);

            }
          
            return cachedClaims.GetValueOrDefault(0);
        }

        private static Session GetToken(string id)
        {
            Account.SessionServiceClient svc = null;
            SessionResponse rsp = null;
            Dictionary<string, string> contextVals = new Dictionary<string, string>();
            contextVals.Add("id", id);
            if (!string.IsNullOrWhiteSpace(id))
            {
                try
                {
                    svc = new  Account.SessionServiceClient(SiteUtilities.AccountService,null,SiteUtilities.CurrentCulture);
                    rsp = svc.Details(id);

                }
                catch (Exception ex)
                {

                    BaseException iex = new BaseException("Logged In Request Retrieval Failure", "TherapyCorner.Portal.UserAuthorization.GetToken", ex,contextVals);
                    throw iex;
                }
                finally
                {
                    if (svc != null) svc.Dispose();
                }
            }
            if (rsp.IsFailure) return null;
            return rsp.Session;
        }
    }

  
}