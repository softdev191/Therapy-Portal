using System;
using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using www.therapycorner.com.account;

namespace TherapyCorner.Portal.Models
{
    public class ApplicationUser : IUser
    {
        public DateTime CreateDate { get; set; }
        private string _id;
        public ApplicationUser()
        {
            CreateDate = DateTime.Now;
            UserName = "NotSet";
            _id = "GUID";
        }

        public ApplicationUser(Session token)
        {
            UserName = token.SessionId;
            _id = token.SessionId;
        }
        public string Id
        {
            get
            {
                return "GUID";
            }
        }

        public string UserName
        {
            get; set;
        }


    }
}