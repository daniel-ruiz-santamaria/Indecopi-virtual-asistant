using IndecopiVirtualAssitant.Dialogs;
using IndecopiVirtualAssitant.Models;
using IndecopiVirtualAssitant.Repositories;
using IndecopiVirtualAssitant.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace IndecopiVirtualAsistant.Dialogs
{
    
    public class CalificationDialog : ComponentDialog
    {
        private const string ANSWERS_TABLE = "answers";
        private const string ASSISTANT_DATA_TABLE = "asistantData";
        private const string SUPPORT_DATA_TABLE = "supportRequest";
        private const string FEEDBACK_DATA_TABLE = "feedback";
        private readonly SessionsData _sessionsData;
        private readonly State _state;
        private SessionState _sessionState;
        private readonly IAzureTableRepository _tableRepository;
        private Feedback _feedback;

        // Dialogs Id
        private readonly string DlgCalificationId = "CalificationDialog";
        private readonly string DlgMailId = "MailDialog";
        private readonly string DlgPhoneId = "PhoneDialog";

        public CalificationDialog(IAzureTableRepository tableRepository, SessionsData sessionsData, State state)
        {
            _tableRepository = tableRepository;
            _sessionsData = sessionsData;
            _state = state;
            _feedback = new Feedback();
            var waterFallSteps = new WaterfallStep[] {
                InitialProcess,
                WelcomeProcess,
                SetCalification,
                SetQuery,
                FinalProcess
            };
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterFallSteps));
            AddDialog(new TextPrompt(DlgCalificationId, CalificationValidatorAsync));
            AddDialog(new TextPrompt(nameof(TextPrompt)));
        }


        private async Task<DialogTurnResult> InitialProcess(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var user = _sessionsData.getSesionState(_state.idSession).user;
            _feedback.IdFeedback = _state.idRequest;
            _feedback.IdSession = _state.idSession;
            if (user.isLoged) {
                _feedback.IdRegistredUser = user.IdRegistredUser; 
            }
            this._sessionState = _sessionsData.getSesionState(_state.idSession);
            return await stepContext.NextAsync(new List<string>(), cancellationToken);
        }

        private async Task<DialogTurnResult> WelcomeProcess(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync("A continuación, puedes calificar al asistente, y añadir comentarios para su mejora", cancellationToken: cancellationToken);
            return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
        }

        private async Task<bool> CalificationValidatorAsync(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            var calification = promptContext.Context.Activity.Text;
            if (calification != null && (calification.Equals("1 ⭐") || calification.Equals("2 ⭐") || calification.Equals("3 ⭐") || calification.Equals("4 ⭐") || calification.Equals("5 ⭐")))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private async Task<DialogTurnResult> SetCalification(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync(
                DlgCalificationId,
                new PromptOptions() { 
                    Prompt = CreateButtonsQuelification("Por favor, valora la atención del asistente"),
                    RetryPrompt = CreateButtonsQuelification("Es necesario selecionar un valor de los indicados. Por favor, valora la atención del asistente")
                },
                cancellationToken
            );
            
        }

        private async Task<DialogTurnResult> SetQuery(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            _feedback.calification = stepContext.Context.Activity.Text;
            await stepContext.Context.SendActivityAsync($"Gracias por tu {_feedback.calification}", cancellationToken: cancellationToken);
            return await stepContext.PromptAsync(
               nameof(TextPrompt),
               new PromptOptions { Prompt = MessageFactory.Text("Por favor, añada si lo considera oportuno, cualquier sugerencia") },
               cancellationToken
               );
        }

        private async Task<DialogTurnResult> FinalProcess(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            _feedback.comments = stepContext.Context.Activity.Text;
            await _tableRepository.SaveFeedbackData(FEEDBACK_DATA_TABLE, _feedback);
            await stepContext.Context.SendActivityAsync("La valoración ha sido registrada, gracias por colaborar", cancellationToken: cancellationToken);
            return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
        }

        private Activity CreateButtonsQuelification(string message)
        {
            var reply = MessageFactory.Text(message);
            reply.SuggestedActions = new SuggestedActions()
            {
                Actions = new List<CardAction>() { 
                    new CardAction(){ Title = "1 ⭐", Value = "1 ⭐", Type= ActionTypes.ImBack},
                    new CardAction(){ Title = "2 ⭐", Value = "2 ⭐", Type= ActionTypes.ImBack},
                    new CardAction(){ Title = "3 ⭐", Value = "3 ⭐", Type= ActionTypes.ImBack},
                    new CardAction(){ Title = "4 ⭐", Value = "4 ⭐", Type= ActionTypes.ImBack},
                    new CardAction(){ Title = "5 ⭐", Value = "5 ⭐", Type= ActionTypes.ImBack},
                }
            };
            return reply as Activity;
        }

    }
}
