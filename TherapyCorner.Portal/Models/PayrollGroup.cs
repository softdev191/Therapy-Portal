using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using www.soundpower.biz.common;

namespace TherapyCorner.Portal.Models
{
    public class PayrollGroup
    {
        public DateTime FromDT { get; set; }
        public DateTime ToDT { get; set; }
        public string Title { get; set; }

        public decimal Count { get; set; }

        public decimal Amount { get; set; }

        public List<PayrollGroup> Children { get; set; }

        public string Ids { get; set; }

        public GenericEntity StaffMember { get; set; }

        public bool CanPay { get; set; }

        public bool Approved { get; set; }
    }
}