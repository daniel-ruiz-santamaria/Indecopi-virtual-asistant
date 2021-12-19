using Microsoft.Bot.Builder.AI.QnA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IndecopiVirtualAssitant.Infraestructure.QnAMakerAI
{
    public interface IQnAMakerAIService
    {
        QnAMaker _qnaMakerResult { get; set; }
    }
}
