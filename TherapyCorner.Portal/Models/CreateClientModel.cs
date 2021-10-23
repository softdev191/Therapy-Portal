using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using www.therapycorner.com.company.ResStrings;
using System.Web.Mvc;

namespace TherapyCorner.Portal.Models
{
    public class CreateClientModel: www.therapycorner.com.company.ClientInfo
    {
        public www.therapycorner.com.company.RelationshipEnum? GuardianRelationship { get; set; }

        [Display(ResourceType = typeof(www.therapycorner.com.account.ResStrings.Dictionary), Name = "FirstName")]
        [StringLength(maximumLength: 50, MinimumLength = 1, ErrorMessageResourceType = typeof(www.therapycorner.com.account.ResStrings.ValidationText), ErrorMessageResourceName = "PersonNameLength")]
        public string GuardianFirstName { get; set; }

        [Display(ResourceType = typeof(www.therapycorner.com.account.ResStrings.Dictionary), Name = "LastName")]
        [StringLength(maximumLength: 50, MinimumLength = 1, ErrorMessageResourceType = typeof(www.therapycorner.com.account.ResStrings.ValidationText), ErrorMessageResourceName = "PersonNameLength")]
        public string GuardianLastName { get; set; }

        [Display(ResourceType = typeof(www.therapycorner.com.account.ResStrings.Dictionary), Name = "MiddleName")]
        [StringLength(maximumLength: 50, MinimumLength = 1, ErrorMessageResourceType = typeof(www.therapycorner.com.account.ResStrings.ValidationText), ErrorMessageResourceName = "PersonNameLength")]
        public string GuardianMiddleName { get; set; }

        [Display(ResourceType = typeof(www.therapycorner.com.account.ResStrings.Dictionary), Name = "Suffix")]
        [StringLength(maximumLength: 20, MinimumLength = 1, ErrorMessageResourceType = typeof(www.therapycorner.com.account.ResStrings.ValidationText), ErrorMessageResourceName = "SuffixLength")]
        public string GuardianSuffix { get; set; }

        [Display(ResourceType = typeof(Dictionary), Name = "Phone")]
        [StringLength(maximumLength: 15, MinimumLength = 10, ErrorMessageResourceType = typeof(www.therapycorner.com.account.ResStrings.ValidationText), ErrorMessageResourceName = "PhoneLength")]
        [RegularExpression("\\(?(?<AreaCode>\\d{3})\\)?-?\\s*(?<Number>\\d{3}(?:-|\\s*)\\d{4})", ErrorMessageResourceType = typeof(ValidationText), ErrorMessageResourceName = "Invalid")]
        public string GuardianPhone { get; set; }

        [Display(ResourceType = typeof(www.therapycorner.com.account.ResStrings.Dictionary), Name = "MobilePhone")]
        [StringLength(maximumLength: 15, MinimumLength = 10, ErrorMessageResourceType = typeof(www.therapycorner.com.account.ResStrings.ValidationText), ErrorMessageResourceName = "PhoneLength")]
        [RegularExpression("\\(?(?<AreaCode>\\d{3})\\)?-?\\s*(?<Number>\\d{3}(?:-|\\s*)\\d{4})", ErrorMessageResourceType = typeof(ValidationText), ErrorMessageResourceName = "Invalid")]
        public string GuardianMobile { get; set; }

        [Display(ResourceType = typeof(www.therapycorner.com.account.ResStrings.Dictionary), Name = "Email")]
        [StringLength(maximumLength: 50, MinimumLength = 6, ErrorMessageResourceType = typeof(www.therapycorner.com.account.ResStrings.ValidationText), ErrorMessageResourceName = "PersonNameLength")]
        [RegularExpression("([a-zA-Z0-9_\\-\\.]+)@((\\[[0-9]{1,3}\\.[0-9]{1,3}\\.[0-9]{1,3}\\.)|(([a-zA-Z0-9\\-]+\\.)+))([a-zA-Z]{2,5}|[0-9]{1,3})", ErrorMessageResourceType = typeof(ValidationText), ErrorMessageResourceName = "Invalid")]
        public string GuardianEmail { get; set; }

        [Display(ResourceType = typeof(www.therapycorner.com.account.ResStrings.Dictionary), Name = "Line1")]
        [StringLength(maximumLength: 50, MinimumLength = 3, ErrorMessageResourceType = typeof(www.therapycorner.com.account.ResStrings.ValidationText), ErrorMessageResourceName = "AddressLineLength")]
        public string GuardianLine1 { get; set; }

        [Display(ResourceType = typeof(www.therapycorner.com.account.ResStrings.Dictionary), Name = "Line2")]
        [StringLength(maximumLength: 50, MinimumLength = 3, ErrorMessageResourceType = typeof(www.therapycorner.com.account.ResStrings.ValidationText), ErrorMessageResourceName = "AddressLineLength")]
        public string GuardianLine2 { get; set; }

        [Display(ResourceType = typeof(www.therapycorner.com.account.ResStrings.Dictionary), Name = "City")]
        [StringLength(maximumLength: 50, MinimumLength = 1, ErrorMessageResourceType = typeof(www.therapycorner.com.account.ResStrings.ValidationText), ErrorMessageResourceName = "CityLength")]
        public string GuardianCity { get; set; }

        [Display(ResourceType = typeof(www.therapycorner.com.account.ResStrings.Dictionary), Name = "State")]
        public string GuardianState { get; set; }

        [Display(ResourceType = typeof(www.therapycorner.com.account.ResStrings.Dictionary), Name = "PostalCode")]
        [RegularExpression("(\\d{5})(-(\\d{4}))?", ErrorMessageResourceType = typeof(www.therapycorner.com.account.ResStrings.ValidationText), ErrorMessageResourceName = "Invalid")]
public string GuardianPostalCode { get; set; }

        internal void Validate(ModelStateDictionary ModelState)
        {
            if(GuardianRelationship.HasValue || !string.IsNullOrWhiteSpace(GuardianCity) || !string.IsNullOrWhiteSpace(GuardianEmail)
                || !string.IsNullOrWhiteSpace(GuardianFirstName) || !string.IsNullOrWhiteSpace(GuardianLastName) || !string.IsNullOrWhiteSpace(GuardianLine1)
                || !string.IsNullOrWhiteSpace(GuardianLine2) || !string.IsNullOrWhiteSpace(GuardianMiddleName) || !string.IsNullOrWhiteSpace(GuardianMobile) 
                || !string.IsNullOrWhiteSpace(GuardianPhone) || !string.IsNullOrWhiteSpace(GuardianPostalCode)
                || !string.IsNullOrWhiteSpace(GuardianSuffix))
            {
                if(!GuardianRelationship.HasValue)  ModelState.AddModelError("GuardianRelationship", ValidationText.Required);
                if(string.IsNullOrWhiteSpace(GuardianCity)) ModelState.AddModelError("GuardianCity", ValidationText.Required);
                if (string.IsNullOrWhiteSpace(GuardianFirstName)) ModelState.AddModelError("GuardianFirstName", ValidationText.Required);
                if (string.IsNullOrWhiteSpace(GuardianLastName)) ModelState.AddModelError("GuardianLastName", ValidationText.Required);
                if (string.IsNullOrWhiteSpace(GuardianLine1)) ModelState.AddModelError("GuardianLine1", ValidationText.Required);
                if (string.IsNullOrWhiteSpace(GuardianPostalCode)) ModelState.AddModelError("GuardianPostalCode", ValidationText.Required);
                if (string.IsNullOrWhiteSpace(GuardianMobile) && string.IsNullOrWhiteSpace(GuardianPhone) && string.IsNullOrWhiteSpace(GuardianEmail))
                {
                    ModelState.AddModelError("GuardianEmail", ValidationText.FormOfContactRequired);
                    ModelState.AddModelError("GuardianPhone", ValidationText.FormOfContactRequired);
                    ModelState.AddModelError("GuardianMobile", ValidationText.FormOfContactRequired);
                }

                this.Guardians = new www.therapycorner.com.company.GuardianInfoList();
                this.Guardians.Add(new www.therapycorner.com.company.GuardianInfo()
                {
                    Address = new www.therapycorner.com.account.AddressInfo()
                    {
                        Line1 = GuardianLine1,
                        Line2 = GuardianLine2,
                        City = GuardianCity,
                        State = GuardianState,
                        PostalCode = GuardianPostalCode
                    },
                    Email = GuardianEmail,
                    FirstName = GuardianFirstName,
                    MiddleName = GuardianMiddleName,
                    IsPrimary = true,
                    GuardianId = -1,
                    LastName = GuardianLastName,
                    Mobile = GuardianMobile,
                    Phone = GuardianPhone,
                    Relationship = GuardianRelationship.GetValueOrDefault(),
                    Suffix = GuardianSuffix,
                    Version = "NEW"

                });
            }
        }

        public www.therapycorner.com.company.ClientInfo ToBase()
        {
            var result = new www.therapycorner.com.company.ClientInfo()
            {
                Address = this.Address,
                ClientId = -1,
                Diagnosis = this.Diagnosis,
                DoB = this.DoB,
                DrEmail = this.DrEmail,
                DrName = this.DrName,
                DrPhone = this.DrPhone,
                Email = this.Email,
                FirstName = this.FirstName,
                Gender = this.Gender,
                LastName = this.LastName,
                MiddleName = this.MiddleName,
                Phone = this.Phone,
                Suffix = this.Suffix,
                Version = "NEW",
                Guardians=this.Guardians
            };


            return result;
        }
    }
}