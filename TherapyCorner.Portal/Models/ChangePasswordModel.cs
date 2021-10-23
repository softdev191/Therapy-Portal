using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using TherapyCorner.Portal.ResourceText;

namespace TherapyCorner.Portal.Models
{
    public class ChangePasswordModel
    {
        [Display(ResourceType = typeof(ProfilePages), Name = "NewPassword")]
        [Required(ErrorMessageResourceType = typeof(www.therapycorner.com.account.ResStrings.ValidationText), ErrorMessageResourceName = "Required")]
        [System.Web.Mvc.AllowHtml]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(ResourceType = typeof(ProfilePages), Name = "ConfirmPassword")]
        [Required(ErrorMessageResourceType = typeof(www.therapycorner.com.account.ResStrings.ValidationText), ErrorMessageResourceName = "Required")]
        [Compare("Password",ErrorMessageResourceType  = typeof(ProfilePages), ErrorMessageResourceName  = "DoesNotMatch")]
        [System.Web.Mvc.AllowHtml]
        [DataType(DataType.Password)]
        public string RptPassword { get; set; }

        [Display(ResourceType = typeof(ProfilePages), Name = "CurrentPassword")]
        [System.Web.Mvc.AllowHtml]
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; }

        public string ReturnURL { get; set; }
    }
}