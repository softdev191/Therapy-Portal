using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TherapyCorner.Portal.Models
{
    public class CalendarEvent
    {
        public string id { get; set; }
        public string title { get; set; }
        public DateTime start { get; set; }
        public DateTime end { get; set; }
        public string url { get; set; }
        public bool startEditable { get; set; }
        public string backgroundColor { get; set; }
        public bool isAppointment { get; set; }
    }

      public class CalendarEventList : List<CalendarEvent>
    {

    }
}