using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IndecopiVirtualAssitant.Services
{
    public class State
    {
        public string idRequest { get; set; }
        public string idUser { get; set; }
        public string nameUser { get; set; }
        public string idSession { get; set; }
        public string nameChannel { get; set; }
        public string idChannel { get; set; }
        public Activity activity { get; set; }



        public void AddActivity(ITurnContext<IMessageActivity> turnContext) {
            this.activity = turnContext.Activity as Activity;
            this.idRequest = turnContext.Activity.Id;
            this.idChannel = turnContext.Activity.ChannelId;
            this.nameChannel = turnContext.Activity.From.Name;
            this.idSession = turnContext.Activity.From.Id;
        }

        public void AddActivity(Activity activity)
        {
            this.activity = activity;
            this.idRequest = activity.Id;
            this.idChannel = activity.ChannelId;
            this.nameChannel = activity.From.Name;
            this.idSession = activity.From.Id;
        }


    }
}
