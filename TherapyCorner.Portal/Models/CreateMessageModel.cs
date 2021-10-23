using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using www.therapycorner.com.company;

namespace TherapyCorner.Portal.Models
{
    public class CreateMessageModel : www.therapycorner.com.company.MessageInfo
    {
        [AllowHtml]
       public string MessageBody { get; set; }

       public string RecipientIds { get; set; }

        internal www.therapycorner.com.company.MessageInfo ToBase()
        {
            var result = new www.therapycorner.com.company.MessageInfo()
            {
                Client=this.Client,
                Contents=MessageBody,
                IsReply=this.IsReply,
                MessageId=-1,
                OriginalMessage=this.OriginalMessage,
                Recipients= new www.therapycorner.com.company.MessageRecipientList(),
                SentAt=DateTime.UtcNow,
                Subject=this.Subject 
            };

            result.Recipients.AddRange(from r in RecipientIds.Split(',')
                                      select new MessageRecipient() { UniqueId = int.Parse(r), Context = "Staff", HasRead = false });

            return result;
        }
    }
}