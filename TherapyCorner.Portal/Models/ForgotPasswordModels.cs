using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using TherapyCorner.Portal.ResourceText;

namespace TherapyCorner.Portal.Models
{
    public class ForgotPasswordLogin
    {
        [Display(ResourceType = typeof(Dictionary), Name = "Login")]
        [Required(ErrorMessageResourceType = typeof(ProfilePages), ErrorMessageResourceName = "LoginIdRequired")]
        [StringLength(20, ErrorMessageResourceType = typeof(ProfilePages), ErrorMessageResourceName = "LoginIdRequired")]
        public string Login { get; set; }
    }

    public class ValidationCode
    {
        [Display(ResourceType = typeof(ProfilePages), Name = "CodeRequest")]
        [Required(ErrorMessageResourceType = typeof(www.therapycorner.com.account.ResStrings.ValidationText), ErrorMessageResourceName = "Required")]
        [System.Web.Mvc.AllowHtml]
        public string Code { get; set; }

        public int UserId { get; set; }

        public int ValidationId { get; set; }

        public string ReturnURL { get; set; }

        public bool HomeDevice { get; set; }

        public bool ByEmail { get; set; }
    }


}