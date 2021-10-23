using TherapyCorner.Portal.ResourceText;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TherapyCorner.Portal.Models
{
    public class LoginViewModel
    {
        [Display(ResourceType = typeof(Dictionary), Name = "Login")]
        [Required(ErrorMessageResourceType = typeof(ProfilePages), ErrorMessageResourceName ="LoginIdRequired")]
        [StringLength(20,ErrorMessageResourceType = typeof(ProfilePages), ErrorMessageResourceName = "LoginIdRequired")]
        public string Login { get; set; }

        [Display(ResourceType = typeof(Dictionary),Name ="Password")]
        [Required(ErrorMessageResourceType = typeof(ProfilePages), ErrorMessageResourceName = "PasswordRequired")]
        [DataType(DataType.Password)]
        [System.Web.Mvc.AllowHtml]
        public string Password { get; set; }

        public string ReturnURL { get; set; }
   
       public string InitialCompany { get; set; }
    }



 
}