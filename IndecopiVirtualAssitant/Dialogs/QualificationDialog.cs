using IndecopiVirtualAssitant.Models;
using IndecopiVirtualAssitant.Repositories;
using IndecopiVirtualAssitant.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace IndecopiVirtualAsistant.Dialogs
{
    public class QualificationDialog : ComponentDialog
    {
        private readonly IAzureTableRepository _tableRepository;
        private readonly SessionsData _sessionsData;
        private readonly State _state;

        public QualificationDialog(IAzureTableRepository tableRepository, SessionsData sessionsData, State state)
        {
            _tableRepository = tableRepository;
            _sessionsData = sessionsData;
            _state = state;

            var waterfallSteps = new WaterfallStep[]
            {
                ToShowButton,
                ValidateOption
            };
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new TextPrompt(nameof(TextPrompt)));
        }
        private async Task<DialogTurnResult> ToShowButton(DialogContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync(
                    nameof(TextPrompt),
                    new PromptOptions
                    {
                        Prompt = CreateButtonsCalification()
                    },
                    cancellationToken
                  );
        }
        private async Task<DialogTurnResult> ValidateOption(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var option = stepContext.Context.Activity.Text;
            await stepContext.Context.SendActivityAsync($"Gracias por tu {option}", cancellationToken: cancellationToken);
            //GUARDAR CALIFICACIÓN
            await SaveQualification(stepContext, option);
            await Task.Delay(1000);
            await stepContext.Context.SendActivityAsync("¿En qué más puedo ayudarte?", cancellationToken: cancellationToken);

            return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
        }

        private async Task SaveQualification(WaterfallStepContext stepContext, string option)
        {
            var qualificationModel = new Feedback();
            /*
            qualificationModel.id = Guid.NewGuid().ToString();
            qualificationModel.idUser = stepContext.Context.Activity.From.Id;
            qualificationModel.qualification = option;
            qualificationModel.registerDate = DateTime.Now.Date;
            await _dataBaseService.Qualification.AddAsync(qualificationModel);
            await _dataBaseService.SaveAsync();
            */
        }

        public Activity CreateButtonsCalification()
        {
            var reply = MessageFactory.Text("Califícame por favor");

            reply.SuggestedActions = new SuggestedActions()
            {
                Actions = new List<CardAction>()
                {
                    new CardAction(){Title = "1⭐", Value = "1⭐", Type = ActionTypes.ImBack},
                    new CardAction(){Title = "2⭐", Value = "2⭐", Type = ActionTypes.ImBack},
                    new CardAction(){Title = "3⭐", Value = "3⭐", Type = ActionTypes.ImBack},
                    new CardAction(){Title = "4⭐", Value = "4⭐", Type = ActionTypes.ImBack},
                    new CardAction(){Title = "5⭐", Value = "5⭐", Type = ActionTypes.ImBack}
                }
            };
            return reply as Activity;
        }
    }
}
