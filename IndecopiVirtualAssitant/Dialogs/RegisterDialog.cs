using IndecopiVirtualAssitant.Models;
using IndecopiVirtualAssitant.Models.AzureTable;
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
using static IndecopiVirtualAssitant.Models.AzureTable.Enums;

namespace IndecopiVirtualAssitant.Dialogs
{
    public class RegisterDialog : ComponentDialog 
    {
        private const string ANSWERS_TABLE = "answers";
        private const string USERS_TABLE = "users";
        private readonly SessionsData _sessionsData;
        private SessionState _sessionState;
        private readonly State _state;
        private User _user;
        private readonly IAzureTableRepository _tableRepository;
        public RegisterDialog(IAzureTableRepository tableRepository, SessionsData sessionsData, State state)
        {
            _tableRepository = tableRepository;
            _sessionsData = sessionsData;
            _state = state;
            // _sessionState = sessionsData.getSesionState(_state.idSession);
            var waterFallStep = new WaterfallStep[] {
                SetFullName,
                SetDocumentType,
                SetDocumentNumber,
                Confirmation,
                FinalProcess
            };
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterFallStep));
            AddDialog(new TextPrompt(nameof(TextPrompt)));
        }

        private async Task<DialogTurnResult> SetFullName(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            _sessionState = _sessionsData.getSesionState(_state.idSession);
            _user = _sessionState.user;
            _user.idSession = _state.idSession;
            string text = await _tableRepository.getAnswer(ANSWERS_TABLE, "SetName", "*Para poder ayudarte mejor voy a necesitar que me facilite algunos datos:\n\n¿ Cual es su nombre completo ?");
            return await stepContext.PromptAsync(
                nameof(TextPrompt),
                new PromptOptions { Prompt = MessageFactory.Text(text.Replace("\\n", "\n")) },
                cancellationToken
                );
        }

        private async Task<DialogTurnResult> SetDocumentType(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //var fullName = stepContext.Context.Activity.Text;
            _user.fullName = stepContext.Context.Activity.Text;

            return await stepContext.PromptAsync(
                nameof(TextPrompt),
                new PromptOptions { Prompt = CreateButtonsDocumentType() },
                cancellationToken
            );
        }

        private async Task<DialogTurnResult> SetDocumentNumber(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            _user.documentType = GetDocumentType(stepContext.Context.Activity.Text);
            string text = await _tableRepository.getAnswer(ANSWERS_TABLE, "SetDocumentNumber", "*Para terminar ingrese su número de documento");
            var documentType = stepContext.Context.Activity.Text;

            return await stepContext.PromptAsync(
                nameof(TextPrompt),
                new PromptOptions { Prompt = MessageFactory.Text(text.Replace("\\n", "\n")) },
                cancellationToken
                );
        }

        private async Task<DialogTurnResult> Confirmation(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            _user.IdDocument = stepContext.Context.Activity.Text.ToLower();
            _user.IdRegistredUser = _user.documentType.ToString() + "-" + _user.IdDocument;
            return await stepContext.PromptAsync(
                nameof(TextPrompt),
                new PromptOptions { Prompt = CreateButtonsConfirmation() },
                cancellationToken
                );
        }

        private async Task<DialogTurnResult> FinalProcess(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var confirmation = stepContext.Context.Activity.Text;
            if (confirmation.ToLower().Equals("si")) 
            {
                _user.isLoged = true;
                _sessionState.isLoged = true;
                _user = _sessionState.user;
                await _tableRepository.SaveUserData(USERS_TABLE, _user);
                string text = await _tableRepository.getAnswer(ANSWERS_TABLE, "RegisterDone", "Registro completado");
                await stepContext.Context.SendActivityAsync(text.Replace("\\n", "\n"), cancellationToken: cancellationToken);
            } 
            else if(confirmation.ToLower().Equals("no")) 
            {
                string text = await _tableRepository.getAnswer(ANSWERS_TABLE, "RegisterCancel", "Registro no completado");
                await stepContext.Context.SendActivityAsync(text.Replace("\\n", "\n"), cancellationToken: cancellationToken);
            } 
            else 
            {
                string text = await _tableRepository.getAnswer(ANSWERS_TABLE, "RegisterEscaped", "Registro abortado");
                await stepContext.Context.SendActivityAsync(text.Replace("\\n", "\n"), cancellationToken: cancellationToken);
            }
            return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
        }

        private Activity CreateButtonsConfirmation()
        {
            var answer = _tableRepository.getAnswer(ANSWERS_TABLE, "RegisterConfirmation", "*¿Quieres confirmar el registro?");
            answer.Wait();
            var reply = MessageFactory.Text(answer.Result.ToString());
            reply.SuggestedActions = new SuggestedActions()
            {
                Actions = new List<CardAction>()
                {
                    new CardAction() { Title = "Si", Value = "Si", Type = ActionTypes.ImBack },
                    new CardAction() { Title = "No", Value = "No", Type = ActionTypes.ImBack }
                }
            };
            return reply as Activity;
        }

        private Activity CreateButtonsDocumentType()
        {
            var answer =  _tableRepository.getAnswer(ANSWERS_TABLE, "SetDocumentType", "*¿Cual es su tipo de documento?");
            answer.Wait();
            List<CardAction> actions = new List<CardAction>();
            foreach (DocumentType dt in DocumentType.GetValues(typeof(DocumentType)))
            {
                actions.Add(new CardAction() { Title = Enums.GetDescription(dt), Value = dt.ToString(), Type = ActionTypes.ImBack });
            }
            var reply = MessageFactory.Text(answer.Result.ToString());
            reply.SuggestedActions = new SuggestedActions()
            {
                Actions = actions
            };
            return reply as Activity;
        }
    }
}
