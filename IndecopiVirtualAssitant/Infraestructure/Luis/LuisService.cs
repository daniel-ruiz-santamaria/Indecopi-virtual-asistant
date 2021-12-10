using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IndecopiVirtualAssitant.Infraestructure.Luis
{
    public class LuisService
    {
        public LuisRecognizer _luisRecognizer { get; set; }

        public LuisService(IConfiguration conf)
        {
            var luisApplication = new LuisApplication(
                conf["LuisAppId"],
                conf["LuisApiKey"],
                conf["LuisHostName"]
            );
        }
    }
}
