using Intuit.Ipp.OAuth2PlatformClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using www.therapycorner.com.account;
using www.therapycorner.com.account.MessageContracts;

namespace TherapyCorner.Portal.Controllers
{
    [RequireUser]
    [RequireHttps]
    [CompanyFilter]
    [RequireAdmin]
    public class SettingsController : Controller
    {
        // GET: Settings
        [HttpGet]
        public ActionResult Integrations(int? tab)
        {
            Account.IntegrationServiceClient svc = null;
            Account.CompanyServiceClient  svcComp = null;
            IntegrationChoiceListResponse rsp = null;
            ViewBag.Tab = tab;
            try
            {
                var token = UserAuthorization.CurrentUser;
                svc = new Account.IntegrationServiceClient(SiteUtilities.AccountService, token, SiteUtilities.CurrentCulture);
                rsp = svc.List();
                svcComp = new Account.CompanyServiceClient(SiteUtilities.AccountService, token, SiteUtilities.CurrentCulture);
                var rspComp = svcComp.Details(token.CurrentCompany);
                ViewBag.Company = rspComp.Company;

            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.Settings.Integrations", ex);
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
            if (rsp.Choices == null) rsp.Choices = new www.therapycorner.com.account.IntegrationChoiceList();
            return View(rsp.Choices);
        }

        public ActionResult InitiateQBLink()
        {
            string scopeVal = OidcScopes.Accounting.GetStringValue();
            string url = "";
            try
            {
                var token = UserAuthorization.CurrentUser;
                DiscoveryResponse doc = GetQBDiscovery();
                url = string.Format("{0}?client_id={1}&response_type=code&scope={2}&redirect_uri={3}&state={4}",
                       doc.AuthorizeEndpoint,
                       SiteUtilities.QBClientId,
                       scopeVal,
                       System.Uri.EscapeDataString(SiteUtilities.QBRedirect),
                     string.Format("{0}-{1}", token.UserId,token.CurrentCompany));
            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.Settings.InitiateQBLink", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            return Redirect(url);
        }

        private static DiscoveryResponse GetQBDiscovery()
        {
            var discoveryClient = new DiscoveryClient(SiteUtilities.QBDiscovery);
            var doc = discoveryClient.GetAsync().Result;
            if (doc.StatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new Exception("Failed to connect to QuickBooks Discovery");
            }
            if (doc.IsError )
            {
                throw new Exception(doc.Error);
            }
            return doc;
        }

        public ActionResult QBLink()
        {
            if (Request.QueryString.Count > 0)
            {
                var response = new AuthorizeResponse(Request.QueryString.ToString());
                if (response.IsError )
                {
                    SoundPower.Web.Notifications.AddErrorNotification(string.Format(ResourceText.SettingsPages.QBLinkError, response.Error));
                }
                else
                {
                    var token = UserAuthorization.CurrentUser;
                    var claims = response.State.Split('-');
                    if (claims[0] != token.User.UniqueId )
                    {
                        SoundPower.Web.Notifications.AddErrorNotification(string.Format(ResourceText.SettingsPages.QBLinkError, "Invalid Therapy Corner User"));

                    }
                    else
                    {
                        Account.SessionServiceClient svcSession = null;
                        Account.IntegrationServiceClient svc = null;
                        IntegrationChoiceListResponse rspChoices = null;
                        www.soundpower.biz.common.ResponseBase rspSave = null;
                        try
                        {
                            if (claims[1]!= token.CurrentCompany)
                            {
                                svcSession = new Account.SessionServiceClient(SiteUtilities.AccountService, UserAuthorization.CurrentUser, SiteUtilities.CurrentCulture);
                                var rsp = svcSession.ChangeCompany(claims[1]);
                                UserAuthorization.ResetCachedToken();
                                UserAuthorization.SetMessageCount();

                                token = UserAuthorization.CurrentUser;
                                if (claims[1] != token.CurrentCompany)
                                {
                                    throw new Exception("Could not switch to linked company");
                                }
                               }
                            DiscoveryResponse doc = GetQBDiscovery();

                            var tokenClient = new TokenClient(doc.TokenEndpoint, SiteUtilities.QBClientId, SiteUtilities.QBClientSecret);
                            TokenResponse accesstokenCallResponse =  tokenClient.RequestTokenFromCodeAsync(response.Code, SiteUtilities.QBRedirect).Result;
                            if (accesstokenCallResponse.HttpStatusCode != System.Net.HttpStatusCode.OK)
                            {
                                throw new Exception("Failed to connect to QB Code Swap");
                            }
                            svc = new Account.IntegrationServiceClient(SiteUtilities.AccountService, UserAuthorization.CurrentUser, SiteUtilities.CurrentCulture);
                            rspChoices = svc.List();
                            if (rspChoices.IsFailure)
                            {
                                SoundPower.Web.Notifications.AddResponseNotifications(rspChoices);
                            }
                            else
                            {
                                var choice = rspChoices.Choices.Find(c => c.Area.UniqueId == "1");
                                if (choice ==null || choice.IntegrationId!=1)
                                {
                                    var t = StaticData.Integrations.Find(i => i.IntegrationId == 1);
                                    choice = t.Clone();
                                    choice.Values = new www.therapycorner.com.account.IntegrationValueList();
                                    choice.Values.AddRange(from a in choice.Attributes
                                                           select new IntegrationValue() { AttributeId = a.AttributeId, Label=a.Label });
                                }

                                var att = choice.Values.Find(a => a.AttributeId == 1);
                                att.Value = response.RealmId;
                                att = choice.Values.Find(a => a.AttributeId == 2);
                                att.Value = accesstokenCallResponse.RefreshToken;
                                att = choice.Values.Find(a => a.AttributeId == 3);
                                DateTime expires = DateTime.UtcNow.AddDays(364);
                                att.Value = expires.ToString();
                                att = choice.Values.Find(a => a.AttributeId == 4);
                                expires = expires.AddDays(-14);
                                att.Value = expires.ToString();
                                rspSave = svc.Update(choice);
                                SoundPower.Web.Notifications.AddResponseNotifications(rspSave);
                                if (!rspSave.IsFailure)
                                {
                                    SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.SettingsPages.QBLinkSuccess);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            var bex = new SoundPower.ErrorTracking.BaseException("QB Link Storage Processing Failure", "TherapyCorner.Portal.Controllers.Settings.QBLink", ex);
                            SoundPower.Web.Utilities.ReportError(bex);
                            throw bex;
                        }
                        finally
                        {
                            if (svcSession != null) svcSession.Dispose();
                            if (svc != null) svc.Dispose();


                        }
                    }
                }
            }
            
                return RedirectToAction("integrations");
        }

        [HttpPost]
        [AntiForgeryHandleError]
        public ActionResult SetAPI(string version)
        {
     
                Account.CompanyServiceClient svc = null;
                www.soundpower.biz.common.ResponseBase rspSave = null;
                try
                {

                    svc = new Account.CompanyServiceClient(SiteUtilities.AccountService, UserAuthorization.CurrentUser, SiteUtilities.CurrentCulture);


                    rspSave = svc.SetAPI(version);
                    SoundPower.Web.Notifications.AddResponseNotifications(rspSave);
                    if (!rspSave.IsFailure)
                    {
                        SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.SettingsPages.APIEnabled);
                    }

                }
                catch (Exception ex)
                {
                    var bex = new SoundPower.ErrorTracking.BaseException("Save Processing Failure", "TherapyCorner.Portal.Controllers.Settings.SetAPI", ex);
                    SoundPower.Web.Utilities.ReportError(bex);
                    throw bex;
                }
                finally
                {
                    if (svc != null) svc.Dispose();


                }
            



            return RedirectToAction("integrations",new { tab = 2 });
        }

        [HttpPost]
        [AntiForgeryHandleError]
        public ActionResult ResetAPI(string version)
        {

            Account.CompanyServiceClient svc = null;
            www.soundpower.biz.common.ResponseBase rspSave = null;
            try
            {

                svc = new Account.CompanyServiceClient(SiteUtilities.AccountService, UserAuthorization.CurrentUser, SiteUtilities.CurrentCulture);


                rspSave = svc.SetAPI(version);
                SoundPower.Web.Notifications.AddResponseNotifications(rspSave);
                if (!rspSave.IsFailure)
                {
                    SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.SettingsPages.APIReset);
                }

            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Save Processing Failure", "TherapyCorner.Portal.Controllers.Settings.ResetAPI", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {
                if (svc != null) svc.Dispose();


            }




            return RedirectToAction("integrations", new { tab = 2 });
        }

        [HttpPost]
        [AntiForgeryHandleError]
        public ActionResult ClearAPI(string version)
        {

            Account.CompanyServiceClient svc = null;
            www.soundpower.biz.common.ResponseBase rspSave = null;
            try
            {

                svc = new Account.CompanyServiceClient(SiteUtilities.AccountService, UserAuthorization.CurrentUser, SiteUtilities.CurrentCulture);


                rspSave = svc.ClearAPI(version);
                SoundPower.Web.Notifications.AddResponseNotifications(rspSave);
                if (!rspSave.IsFailure)
                {
                    SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.SettingsPages.APIDisabled);
                }

            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Save Processing Failure", "TherapyCorner.Portal.Controllers.Settings.ClearAPI", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {
                if (svc != null) svc.Dispose();


            }




            return RedirectToAction("integrations", new { tab = 2 });
        }

        [HttpPost]
        public ActionResult Integrations(IntegrationChoice choice)
        {
            if (ModelState.IsValid)
            {
            
                        Account.IntegrationServiceClient svc = null;
                        www.soundpower.biz.common.ResponseBase rspSave = null;
                        try
                        {
                  
                            svc = new Account.IntegrationServiceClient(SiteUtilities.AccountService, UserAuthorization.CurrentUser, SiteUtilities.CurrentCulture);
                          
                               
                                rspSave = svc.Update(choice);
                                SoundPower.Web.Notifications.AddResponseNotifications(rspSave);
                                if (!rspSave.IsFailure)
                                {
                                    SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.SettingsPages.IntegrationsUpdated);
                                }
                            
                        }
                        catch (Exception ex)
                        {
                            var bex = new SoundPower.ErrorTracking.BaseException("Save Processing Failure", "TherapyCorner.Portal.Controllers.Settings.Integrations", ex);
                            SoundPower.Web.Utilities.ReportError(bex);
                            throw bex;
                        }
                        finally
                        {
                            if (svc != null) svc.Dispose();


                        }
                    }
                
            

            return RedirectToAction("integrations");
        }

        [HttpPost]
        public ActionResult UpdateClearingHouse(CompanyInfo company)
        {
            if (ModelState.IsValid)
            {

                Account.CompanyServiceClient svc = null;
                www.soundpower.biz.common.ResponseBase rspSave = null;
                try
                {

                    svc = new Account.CompanyServiceClient(SiteUtilities.AccountService, UserAuthorization.CurrentUser, SiteUtilities.CurrentCulture);


                    rspSave = svc.UpdateClearingHouse(company);
                    SoundPower.Web.Notifications.AddResponseNotifications(rspSave);
                    if (!rspSave.IsFailure)
                    {
                        UserAuthorization.ResetCachedToken();
                        SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.SettingsPages.ClearingHouseUpdated);
                    }

                }
                catch (Exception ex)
                {
                    var bex = new SoundPower.ErrorTracking.BaseException("Save Processing Failure", "TherapyCorner.Portal.Controllers.Settings.UpdateClearingHouse", ex);
                    SoundPower.Web.Utilities.ReportError(bex);
                    throw bex;
                }
                finally
                {
                    if (svc != null) svc.Dispose();


                }
            }



            return RedirectToAction("integrations");
        }

        public ActionResult SyncQB()
        {
            Account.IntegrationServiceClient svc = null;
            IntegrationChoiceListResponse rsp = null;
            Company.StaffServiceClient svcComp = null;
            try
            {
                var token = UserAuthorization.CurrentUser;
                svc = new Account.IntegrationServiceClient(SiteUtilities.AccountService, token, SiteUtilities.CurrentCulture);
                rsp = svc.List();

                if (rsp.Choices == null)
                {
                        SoundPower.Web.Notifications.AddErrorNotification(ResourceText.SettingsPages.NotLinkedToQB);
             
                   
                }
                else
                {
                    var accountingChoice = rsp.Choices.Find(c => c.Area.UniqueId == "1");
                    if (accountingChoice == null || accountingChoice.IntegrationId==-1)
                    {
                        SoundPower.Web.Notifications.AddErrorNotification(ResourceText.SettingsPages.NotLinkedToQB);


                    }
                    else
                    {
                        svcComp = new Company.StaffServiceClient(SiteUtilities.CompanyService, token, SiteUtilities.CurrentCulture);
                        accountingChoice.Name = token.CurrentCompany;
                       var rspSync=  svcComp.SyncQB(accountingChoice);
                        SoundPower.Web.Notifications.AddResponseNotifications(rspSync);
                        if (!rsp.IsFailure) SoundPower.Web.Notifications.AddSuccessMessages(ResourceText.SettingsPages.QBSyncd);
                    }
                }

            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.Settings.SyncQB", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                throw bex;
            }
            finally
            {
                if (svc != null) svc.Dispose();
                if (svcComp != null) svcComp.Dispose();
            }





            return RedirectToAction("integrations");
        }
    }
}