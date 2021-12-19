using Microsoft.Bot.Builder.AI.QnA;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IndecopiVirtualAssitant.Infraestructure.QnAMakerAI
{
    public class QnAMakerAIService : IQnAMakerAIService
    {
        public QnAMaker _qnaMakerResult { get; set; }

        public QnAMakerAIService(IConfiguration conf)
        {
            _qnaMakerResult = new QnAMaker(new QnAMakerEndpoint {
                KnowledgeBaseId = conf["QnAMakerBaseId"],
                EndpointKey = conf["QnAMakerKey"],
                Host = conf["QnAMakerBaseHostName"]
            });
        }
    }
}
