using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TherapyCorner.Portal.Models
{
    public class MissingCredentialCounts
    {
        public string TypeId { get; set; }
        public string Name { get; set; }
        public string Blocking { get; set; }

        public int Required { get; set; }

        public int Uploaded { get; set; }

        public int Verified { get; set; }
    }
}