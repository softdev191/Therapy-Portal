using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using www.soundpower.biz.common;
using www.therapycorner.com.account;

namespace TherapyCorner.Portal
{
    public static class StaticData
    {

        public static GenericEntityList CredentialTypes
        {
            get
            {
                var cachedData = System.Web.HttpContext.Current.Cache["CredentialTypes"] as GenericEntityList;
                if (cachedData==null)
                {
                    Account.ReferenceClientService svc = null;
                    GenericEntityListResponse rsp = null;

                    try
                    {
                        var token = UserAuthorization.CurrentUser;
                        svc = new Account.ReferenceClientService(SiteUtilities.AccountService,  SiteUtilities.CurrentCulture);
                        rsp = svc.CredentialTypes();

                    }
                    catch (Exception ex)
                    {
                        throw new SoundPower.ErrorTracking.BaseException("Reference List Processing Failure", "TherapyCorner.Portal.StaticData.CredentialTypes", ex);
                    }
                    finally
                    {
                        if (svc != null) svc.Dispose();
                    }


                    if (rsp.IsFailure)
                    {

                        throw new SoundPower.ErrorTracking.BaseException("Reference List Failure Received", "TherapyCorner.Portal.StaticData.CredentialTypes");

                    }
                    System.Web.HttpContext.Current.Cache.Add("CredentialTypes", rsp.EntityList, null, System.Web.Caching.Cache.NoAbsoluteExpiration, new TimeSpan(0, 30, 0), System.Web.Caching.CacheItemPriority.Normal, null);
                    cachedData = rsp.EntityList;
                }
                return cachedData;
            }
        }

        public static GenericEntityList InsuranceTypes
        {
            get
            {
                var cachedData = System.Web.HttpContext.Current.Cache["InsuranceTypes"] as GenericEntityList;
                if (cachedData == null)
                {
                    Account.ReferenceClientService svc = null;
                    GenericEntityListResponse rsp = null;

                    try
                    {
                        var token = UserAuthorization.CurrentUser;
                        svc = new Account.ReferenceClientService(SiteUtilities.AccountService, SiteUtilities.CurrentCulture);
                        rsp = svc.InsuranceTypes();

                    }
                    catch (Exception ex)
                    {
                        throw new SoundPower.ErrorTracking.BaseException("Reference List Processing Failure", "TherapyCorner.Portal.StaticData.InsuranceTypes", ex);
                    }
                    finally
                    {
                        if (svc != null) svc.Dispose();
                    }


                    if (rsp.IsFailure)
                    {

                        throw new SoundPower.ErrorTracking.BaseException("Reference List Failure Received", "TherapyCorner.Portal.StaticData.InsuranceTypes");

                    }
                    System.Web.HttpContext.Current.Cache.Add("InsuranceTypes", rsp.EntityList, null, System.Web.Caching.Cache.NoAbsoluteExpiration, new TimeSpan(0, 30, 0), System.Web.Caching.CacheItemPriority.Normal, null);
                    cachedData = rsp.EntityList;
                }
                return cachedData;
            }
        }

        public static GenericEntityList InsuranceServiceTypes
        {
            get
            {
                var cachedData = System.Web.HttpContext.Current.Cache["InsuranceServiceTypes"] as GenericEntityList;
                if (cachedData == null)
                {
                    Account.ReferenceClientService svc = null;
                    GenericEntityListResponse rsp = null;

                    try
                    {
                        var token = UserAuthorization.CurrentUser;
                        svc = new Account.ReferenceClientService(SiteUtilities.AccountService, SiteUtilities.CurrentCulture);
                        rsp = svc.InsuranceServiceTypes();

                    }
                    catch (Exception ex)
                    {
                        throw new SoundPower.ErrorTracking.BaseException("Reference List Processing Failure", "TherapyCorner.Portal.StaticData.InsuranceServiceTypes", ex);
                    }
                    finally
                    {
                        if (svc != null) svc.Dispose();
                    }


                    if (rsp.IsFailure)
                    {

                        throw new SoundPower.ErrorTracking.BaseException("Reference List Failure Received", "TherapyCorner.Portal.StaticData.InsuranceServiceTypes");

                    }
                    System.Web.HttpContext.Current.Cache.Add("InsuranceServiceTypes", rsp.EntityList, null, System.Web.Caching.Cache.NoAbsoluteExpiration, new TimeSpan(0, 30, 0), System.Web.Caching.CacheItemPriority.Normal, null);
                    cachedData = rsp.EntityList;
                }
                return cachedData;
            }
        }
        public static ClearingHouseInfoList ClearingHouses
        {
            get
            {
                var cachedData = System.Web.HttpContext.Current.Cache["ClearingHouses"] as ClearingHouseInfoList;
                if (cachedData == null)
                {
                    Account.ReferenceClientService svc = null;
                    www.therapycorner.com.account.MessageContracts.ClearingHouseListResponse rsp = null;

                    try
                    {
                        var token = UserAuthorization.CurrentUser;
                        svc = new Account.ReferenceClientService(SiteUtilities.AccountService, SiteUtilities.CurrentCulture);
                        rsp = svc.ClearingHouses();

                    }
                    catch (Exception ex)
                    {
                        throw new SoundPower.ErrorTracking.BaseException("Reference List Processing Failure", "TherapyCorner.Portal.StaticData.ClearingHouses", ex);
                    }
                    finally
                    {
                        if (svc != null) svc.Dispose();
                    }


                    if (rsp.IsFailure)
                    {

                        throw new SoundPower.ErrorTracking.BaseException("Reference List Failure Received", "TherapyCorner.Portal.StaticData.ClearingHouses");

                    }
                    System.Web.HttpContext.Current.Cache.Add("ClearingHouses", rsp.ClearingHouses, null, System.Web.Caching.Cache.NoAbsoluteExpiration, new TimeSpan(0, 30, 0), System.Web.Caching.CacheItemPriority.Normal, null);
                    cachedData = rsp.ClearingHouses;
                }
                return cachedData;
            }
        }


        public static GovernmentProgramList GovernmentPrograms
        {
            get
            {
                var cachedData = System.Web.HttpContext.Current.Cache["GovernmentPrograms"] as GovernmentProgramList;
                if (cachedData == null)
                {
                    Account.ReferenceClientService svc = null;
                    www.therapycorner.com.account.MessageContracts.GovernmentProgramListResponse rsp = null;

                    try
                    {
                        var token = UserAuthorization.CurrentUser;
                        svc = new Account.ReferenceClientService(SiteUtilities.AccountService, SiteUtilities.CurrentCulture);
                        rsp = svc.GovernmentPrograms() ;

                    }
                    catch (Exception ex)
                    {
                        throw new SoundPower.ErrorTracking.BaseException("Reference List Processing Failure", "TherapyCorner.Portal.StaticData.GovernmentPrograms", ex);
                    }
                    finally
                    {
                        if (svc != null) svc.Dispose();
                    }


                    if (rsp.IsFailure)
                    {

                        throw new SoundPower.ErrorTracking.BaseException("Reference List Failure Received", "TherapyCorner.Portal.StaticData.GovernmentPrograms");

                    }
                    System.Web.HttpContext.Current.Cache.Add("GovernmentPrograms", rsp.Programs, null, System.Web.Caching.Cache.NoAbsoluteExpiration, new TimeSpan(0, 30, 0), System.Web.Caching.CacheItemPriority.Normal, null);
                    cachedData = rsp.Programs;
                }
                return cachedData;
            }
        }

        public static InsuranceCompanyList InsuranceCompanies(int clearingHouse)
        {
            string cid = string.Format("InsuranceCompanies_", clearingHouse);
                var cachedData = System.Web.HttpContext.Current.Cache[cid] as InsuranceCompanyList;
                if (cachedData == null)
                {
                    Account.ReferenceClientService svc = null;
                    www.therapycorner.com.account.MessageContracts.InsuranceCompanyListResponse rsp = null;

                    try
                    {
                        var token = UserAuthorization.CurrentUser;
                        svc = new Account.ReferenceClientService(SiteUtilities.AccountService, SiteUtilities.CurrentCulture);
                        rsp = svc.InsuranceCompanies(clearingHouse.ToString());

                    }
                    catch (Exception ex)
                    {
                        throw new SoundPower.ErrorTracking.BaseException("Reference List Processing Failure", "TherapyCorner.Portal.StaticData.InsuranceCompanies", ex);
                    }
                    finally
                    {
                        if (svc != null) svc.Dispose();
                    }


                    if (rsp.IsFailure)
                    {

                        throw new SoundPower.ErrorTracking.BaseException("Reference List Failure Received", "TherapyCorner.Portal.StaticData.InsuranceCompanies");

                    }
                    System.Web.HttpContext.Current.Cache.Add(cid, rsp.Companies, null, System.Web.Caching.Cache.NoAbsoluteExpiration, new TimeSpan(0, 30, 0), System.Web.Caching.CacheItemPriority.Normal, null);
                    cachedData = rsp.Companies;
                }
                return cachedData;
            
        }


        public static InsuranceCompanyList AllInsuranceCompanies
        {
            get
            {
                var cachedData = System.Web.HttpContext.Current.Cache["InsuranceCompanies"] as InsuranceCompanyList;
                if (cachedData == null)
                {
                    Account.ReferenceClientService svc = null;
                    www.therapycorner.com.account.MessageContracts.InsuranceCompanyListResponse rsp = null;

                    try
                    {
                        var token = UserAuthorization.CurrentUser;
                        svc = new Account.ReferenceClientService(SiteUtilities.AccountService, SiteUtilities.CurrentCulture);
                        rsp = svc.InsuranceCompanies("ALL");

                    }
                    catch (Exception ex)
                    {
                        throw new SoundPower.ErrorTracking.BaseException("Reference List Processing Failure", "TherapyCorner.Portal.StaticData.InsuranceCompanies", ex);
                    }
                    finally
                    {
                        if (svc != null) svc.Dispose();
                    }


                    if (rsp.IsFailure)
                    {

                        throw new SoundPower.ErrorTracking.BaseException("Reference List Failure Received", "TherapyCorner.Portal.StaticData.InsuranceCompanies");

                    }
                    System.Web.HttpContext.Current.Cache.Add("InsuranceCompanies", rsp.Companies, null, System.Web.Caching.Cache.NoAbsoluteExpiration, new TimeSpan(0, 30, 0), System.Web.Caching.CacheItemPriority.Normal, null);
                    cachedData = rsp.Companies;
                }
                return cachedData;
            }
        }

        public static GenericEntityList CPTCodes
        {
            get
            {
                var cachedData = System.Web.HttpContext.Current.Cache["CPTCodes"] as GenericEntityList;
                if (cachedData == null)
                {
                    Account.ReferenceClientService svc = null;
                    GenericEntityListResponse rsp = null;

                    try
                    {
                        var token = UserAuthorization.CurrentUser;
                        svc = new Account.ReferenceClientService(SiteUtilities.AccountService, SiteUtilities.CurrentCulture);
                        rsp = svc.CPTCodes();

                    }
                    catch (Exception ex)
                    {
                        throw new SoundPower.ErrorTracking.BaseException("Reference List Processing Failure", "TherapyCorner.Portal.StaticData.CPTCodes", ex);
                    }
                    finally
                    {
                        if (svc != null) svc.Dispose();
                    }


                    if (rsp.IsFailure)
                    {

                        throw new SoundPower.ErrorTracking.BaseException("Reference List Failure Received", "TherapyCorner.Portal.StaticData.CPTCodes");

                    }
                    System.Web.HttpContext.Current.Cache.Add("CPTCodes", rsp.EntityList, null, System.Web.Caching.Cache.NoAbsoluteExpiration, new TimeSpan(0, 30, 0), System.Web.Caching.CacheItemPriority.Normal, null);
                    cachedData = rsp.EntityList;
                }
                return cachedData;
            }
        }

        public static GenericEntityList Diagnosis
        {
            get
            {
                var cachedData = System.Web.HttpContext.Current.Cache["Diagnosis"] as GenericEntityList;
                if (cachedData == null)
                {
                    Account.ReferenceClientService svc = null;
                    GenericEntityListResponse rsp = null;

                    try
                    {
                        var token = UserAuthorization.CurrentUser;
                        svc = new Account.ReferenceClientService(SiteUtilities.AccountService, SiteUtilities.CurrentCulture);
                        rsp = svc.Diagnosis();

                    }
                    catch (Exception ex)
                    {
                        throw new SoundPower.ErrorTracking.BaseException("Reference List Processing Failure", "TherapyCorner.Portal.StaticData.Diagnosis", ex);
                    }
                    finally
                    {
                        if (svc != null) svc.Dispose();
                    }


                    if (rsp.IsFailure)
                    {

                        throw new SoundPower.ErrorTracking.BaseException("Reference List Failure Received", "TherapyCorner.Portal.StaticData.Diagnosis");

                    }
                    System.Web.HttpContext.Current.Cache.Add("Diagnosis", rsp.EntityList, null, System.Web.Caching.Cache.NoAbsoluteExpiration, new TimeSpan(0, 30, 0), System.Web.Caching.CacheItemPriority.Normal, null);
                    cachedData = rsp.EntityList;
                }
                return cachedData;
            }
        }

        public static IntegrationChoiceList Integrations
        {
            get
            {
                var cachedData = System.Web.HttpContext.Current.Cache["Integrations"] as IntegrationChoiceList;
                if (cachedData == null)
                {
                    Account.ReferenceClientService svc = null;
                    www.therapycorner.com.account.MessageContracts.IntegrationChoiceListResponse rsp = null;

                    try
                    {
                        var token = UserAuthorization.CurrentUser;
                        svc = new Account.ReferenceClientService(SiteUtilities.AccountService, SiteUtilities.CurrentCulture);
                        rsp = svc.Integrations();

                    }
                    catch (Exception ex)
                    {
                        throw new SoundPower.ErrorTracking.BaseException("Reference List Processing Failure", "TherapyCorner.Portal.StaticData.Integrations", ex);
                    }
                    finally
                    {
                        if (svc != null) svc.Dispose();
                    }


                    if (rsp.IsFailure)
                    {

                        throw new SoundPower.ErrorTracking.BaseException("Reference List Failure Received", "TherapyCorner.Portal.StaticData.Integrations");

                    }
                    System.Web.HttpContext.Current.Cache.Add("Diagnosis", rsp.Choices, null, System.Web.Caching.Cache.NoAbsoluteExpiration, new TimeSpan(0, 30, 0), System.Web.Caching.CacheItemPriority.Normal, null);
                    cachedData = rsp.Choices;
                }
                return cachedData;
            }
        }


        public static FieldTypeList GovernmentFields(string id)
        {
            string nme = string.Format("GovtFields_{0}", id);
            var cachedData = System.Web.HttpContext.Current.Cache[nme] as FieldTypeList;
            if (cachedData == null)
            {
                Account.ReferenceClientService svc = null;
                www.therapycorner.com.account.MessageContracts.FieldTypeListResponse rsp = null;

                try
                {
                    var token = UserAuthorization.CurrentUser;
                    svc = new Account.ReferenceClientService(SiteUtilities.AccountService, SiteUtilities.CurrentCulture);
                    rsp = svc.GovernmentProgramFields(id);

                }
                catch (Exception ex)
                {
                    throw new SoundPower.ErrorTracking.BaseException("Reference List Processing Failure", "TherapyCorner.Portal.StaticData.GovernmentFields", ex);
                }
                finally
                {
                    if (svc != null) svc.Dispose();
                }


                if (rsp.IsFailure)
                {

                    throw new SoundPower.ErrorTracking.BaseException("Reference List Failure Received", "TherapyCorner.Portal.StaticData.GovernmentFields");

                }
                System.Web.HttpContext.Current.Cache.Add(nme, rsp.Fields, null, System.Web.Caching.Cache.NoAbsoluteExpiration, new TimeSpan(0, 30, 0), System.Web.Caching.CacheItemPriority.Normal, null);
                cachedData = rsp.Fields;
            }
            return cachedData;
        }

        public static ProgramCategoryList  GovernmentCategories
        {
            get
            {
                var cachedData = System.Web.HttpContext.Current.Cache["GovtCategories"] as www.therapycorner.com.account.ProgramCategoryList;
                if (cachedData == null)
                {
                    Account.ReferenceClientService svc = null;
                    www.therapycorner.com.account.MessageContracts.ProgramCategoryListResponse rsp = null;

                    try
                    {
                        var token = UserAuthorization.CurrentUser;
                        svc = new Account.ReferenceClientService(SiteUtilities.AccountService, SiteUtilities.CurrentCulture);
                        rsp = svc.GovernmentProgramCategories();

                    }
                    catch (Exception ex)
                    {
                        throw new SoundPower.ErrorTracking.BaseException("Reference List Processing Failure", "TherapyCorner.Portal.StaticData.GovernmentPrograms", ex);
                    }
                    finally
                    {
                        if (svc != null) svc.Dispose();
                    }


                    if (rsp.IsFailure)
                    {

                        throw new SoundPower.ErrorTracking.BaseException("Reference List Failure Received", "TherapyCorner.Portal.StaticData.GovernmentCategories");

                    }
                    System.Web.HttpContext.Current.Cache.Add("GovtCategories", rsp.Categories, null, System.Web.Caching.Cache.NoAbsoluteExpiration, new TimeSpan(0, 30, 0), System.Web.Caching.CacheItemPriority.Normal, null);
                    cachedData = rsp.Categories;
                }
                return cachedData;
            }
        }
    }
}