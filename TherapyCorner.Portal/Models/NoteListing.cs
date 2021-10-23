using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TherapyCorner.Portal.Models
{
    public class NoteListing
    {
        public DateTime NoteDate { get; set; }
        public string Type { get; set; }
        public string DetailsUrl { get; set; }
        public string Source { get; set; }
        public www.therapycorner.com.company.FilingStatusEnum Status { get; set; }
        public string Contributors { get; set; }
    }
}