using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using www.soundpower.biz.common;

namespace TherapyCorner.Portal.Controllers
{
    public class ReferenceController : Controller
    {
        // GET: Reference
        public JsonResult Diagnosis(string name)
        {
            JsonResult result = new JsonResult();
            result.JsonRequestBehavior = JsonRequestBehavior.AllowGet;

            GenericEntityListResponse rsp = new GenericEntityListResponse() { EntityList = new GenericEntityList() };
            try
            {
                var data = StaticData.Diagnosis;

                rsp.EntityList.AddRange(from d in data
                                        where d.UniqueId.Contains(name.ToUpper())
                                        || d.Name.ToLower().Contains(name.ToLower())
                                        orderby d.Name
                                        select d);
                if (rsp.EntityList.Count>30)
                {
                    rsp.EntityList.RemoveRange(29, rsp.EntityList.Count - 30);
                }
            }
            catch (Exception ex)
            {
                var bex = new SoundPower.ErrorTracking.BaseException("Processing Failure", "TherapyCorner.Portal.Controllers.Reference.Diagnosis", ex);
                SoundPower.Web.Utilities.ReportError(bex);
                rsp.IsFailure = true;
                rsp.ErrorMessages.Add(bex.Message);
            }
         

            result.Data = rsp;
            return result;
        }
    }
}