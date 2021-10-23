using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TherapyCorner.Portal.Models
{
    public class ClientServiceScheduleInfo: www.therapycorner.com.company.ClientService 
    {
        public ClientServiceScheduleInfo()
        {

        }

        public ClientServiceScheduleInfo(www.therapycorner.com.company.ClientService bse)
        {
            this.ApprovedFrom = bse.ApprovedFrom;
            this.ApprovedTo = bse.ApprovedTo;
            this.Client = bse.Client;
            this.Duration = bse.Duration;
            this.Id = bse.Id;
            this.Location = bse.Location;
            this.Provider = bse.Provider;
            this.Rate = bse.Rate;
            this.Start = bse.Start;
            this.AllowedCount = bse.AllowedCount.GetValueOrDefault(0);
            this.ScheduledCount = bse.ScheduledCount.GetValueOrDefault(0);
            this.Service = bse.Service;
            this.LastAppointment = bse.LastAppointment;
        }

        public int DurationTime { get; set; }
    }

    public class ClientServiceScheduleInfoResponse : www.soundpower.biz.common.ResponseBase
    {
        public List<ClientServiceScheduleInfo> Services { get; set; }

        public ClientServiceScheduleInfoResponse() : base()
        {
            Services = new List<Models.ClientServiceScheduleInfo>();
        }
    }
}