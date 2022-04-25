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
    
    public class ContactDialog : ComponentDialog
    {
        private const string ANSWERS_TABLE = "answers";
        private const string ASSISTANT_DATA_TABLE = "asistantData";
        private const string SUPPORT_DATA_TABLE = "supportRequest";
        private readonly SessionsData _sessionsData;
        private readonly State _state;
        private User _user;
        private SessionState _sessionState;
        private readonly IAzureTableRepository _tableRepository;
        private SupportRequest _supportRequest;

        // Dialogs Id
        private readonly string DlgDepartmentId = "DepartmentDialog";
        private readonly string DlgMailId = "MailDialog";
        private readonly string DlgPhoneId = "PhoneDialog";
        private int countErrors = 0;

        public ContactDialog(IAzureTableRepository tableRepository, SessionsData sessionsData, State state)
        {
            _tableRepository = tableRepository;
            _sessionsData = sessionsData;
            _state = state;
            countErrors = 0;
            var waterFallSteps = new WaterfallStep[] {
                InitialProcess,
                WelcomeProcess,
                SetDepartment,
                SetMail,
                SetContactPhone,
                SetQuery,
                FinalProcess
            };
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterFallSteps));
            AddDialog(new TextPrompt(DlgDepartmentId, DepartementValidatorAsync));
            AddDialog(new TextPrompt(DlgMailId, EmailValidatorAsync));
            AddDialog(new TextPrompt(DlgPhoneId, PhoneValidatorAsync));
            AddDialog(new TextPrompt(nameof(TextPrompt)));
        }


        private async Task<DialogTurnResult> InitialProcess(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            countErrors = 0;
            await stepContext.Context.SendActivityAsync("Para que te atienda una persona, necesito hacerte algunas preguntas, para trasladar tu caso al interlocutor adecuado. Puedes escribir salir, para interrumpir la conversación y volver al menú", cancellationToken: cancellationToken);
            this._supportRequest = new SupportRequest(_state);
            this._sessionState = _sessionsData.getSesionState(_state.idSession);
            if (!_sessionState.isLoged) {
                return await stepContext.BeginDialogAsync(nameof(RegisterDialog), null, cancellationToken);
            } else {
                return await stepContext.NextAsync(new List<string>(), cancellationToken);
            }
        }

        private async Task<DialogTurnResult> WelcomeProcess(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            _supportRequest.user = _sessionsData.getSesionState(_state.idSession).user;
            _supportRequest.userFullName = _supportRequest.user.fullName;
            _supportRequest.documentType = _supportRequest.user.documentType.ToString();
            _supportRequest.documentId = _supportRequest.user.IdDocument;
            _supportRequest.IdRegistredUser = _supportRequest.user.IdRegistredUser;
            _supportRequest.IdSession = _supportRequest.user.idSession;
            await stepContext.Context.SendActivityAsync("A continuación, podrá exponer su consulta, y esta será atendida por un interlocutor humano", cancellationToken: cancellationToken);
            return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
        }

        private async Task<bool> DepartementValidatorAsync(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            var dep = promptContext.Context.Activity.Text;
            var results = await _tableRepository.getAssistantData(ASSISTANT_DATA_TABLE, "department", dep);
            if (results != null && results.Count > 0)
            {
                countErrors = 0;
                return true;
            }
            else
            {
                countErrors += 1;
                if (countErrors >= 3 || promptContext.Context.Activity.Text.ToLower().Trim().Equals("salir"))
                {
                    return true;
                }
                return false;
            }
        }

        private async Task<DialogTurnResult> SetDepartment(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (countErrors > 3 || stepContext.Context.Activity.Text.ToLower().Trim().Equals("salir")) {
                await stepContext.Context.SendActivityAsync("Has superado el número de reintentos permitido, vamos a salir de este dialogo", cancellationToken: cancellationToken);
                stepContext.Context.Activity.Text = "Menu";
                return await stepContext.ReplaceDialogAsync(nameof(RootDialog), null, cancellationToken);
            }
            return await stepContext.PromptAsync(
                DlgDepartmentId,
                new PromptOptions() { 
                    Prompt = CreateDepartmentOptions("Selecciona el departamento con el que quieres ponerte en contacto"),
                    RetryPrompt = CreateDepartmentOptions("Departamento no valido.\nSelecciona el departamento con el que quieres ponerte en contacto")
                },
                cancellationToken
            );
            
        }

        private async Task<bool> EmailValidatorAsync(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            var mail = promptContext.Context.Activity.Text;
            if (new EmailAddressAttribute().IsValid(mail)) 
            {
                countErrors = 0;
                return true;
            } else 
            {
                countErrors += 1;
                if (countErrors >= 3 || promptContext.Context.Activity.Text.ToLower().Trim().Equals("salir"))
                {
                    return true;
                }
                return false;
            }
        }

        private async Task<DialogTurnResult> SetMail(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {

            if (countErrors >= 3 || stepContext.Context.Activity.Text.ToLower().Trim().Equals("salir"))
            {
                await stepContext.Context.SendActivityAsync("Has superado el número de reintentos permitido, vamos a salir de este dialogo", cancellationToken: cancellationToken);
                stepContext.Context.Activity.Text = "Menu";
                return await stepContext.ReplaceDialogAsync(nameof(RootDialog), null, cancellationToken);
            }
            _supportRequest.departmentId = stepContext.Context.Activity.Text;
            var departmentNames = await _tableRepository.getAssistantData(ASSISTANT_DATA_TABLE, "department", _supportRequest.departmentId);
            _supportRequest.departmentName = departmentNames[0].value;
            return await stepContext.PromptAsync(
                                DlgMailId,
                                new PromptOptions {
                                    Prompt = MessageFactory.Text("Por favor, introduzca su email"),
                                    RetryPrompt = MessageFactory.Text("Mail introducido no valido.\nPor favor, introduzca su email"),
                                },
                                cancellationToken
                                );
        }

        private async Task<bool> PhoneValidatorAsync(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            var response = promptContext.Context.Activity.Text;

            Regex regex = new Regex(@"^([+]?[\s0-9]+)?(\d{3}|[(]?[0-9]+[)])?([-]?[\s]?[0-9])+$");
            Match match = regex.Match(response.ToString());
            if (response!=null && match.Success)
            {
                countErrors = 0;
                return true;
            }
            else
            {
                countErrors += 1;
                if (countErrors >= 3 || promptContext.Context.Activity.Text.ToLower().Trim().Equals("salir"))
                {
                    return true;
                }
                return false;
            }
        }

        private async Task<DialogTurnResult> SetContactPhone(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (countErrors > 3 || stepContext.Context.Activity.Text.ToLower().Trim().Equals("salir"))
            {
                await stepContext.Context.SendActivityAsync("Has superado el número de reintentos permitido, vamos a salir de este dialogo", cancellationToken: cancellationToken);
                stepContext.Context.Activity.Text = "Menu";
                return await stepContext.ReplaceDialogAsync(nameof(RootDialog), null, cancellationToken);
            }
            _supportRequest.email = stepContext.Context.Activity.Text;
            return await stepContext.PromptAsync(
               DlgPhoneId,
               new PromptOptions { 
                   Prompt = MessageFactory.Text("Por favor, introduzca su telefono de contacto"),
                   RetryPrompt = MessageFactory.Text("Telefono introducido no valido.\nPor favor, introduzca un telefono valido"),
               },
               cancellationToken
               );
        }

        private async Task<DialogTurnResult> SetQuery(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (countErrors >= 3 || stepContext.Context.Activity.Text.ToLower().Trim().Equals("salir"))
            {
                await stepContext.Context.SendActivityAsync("Has superado el número de reintentos permitido, vamos a salir de este dialogo", cancellationToken: cancellationToken);
                stepContext.Context.Activity.Text = "Menu";
                return await stepContext.ReplaceDialogAsync(nameof(RootDialog), null, cancellationToken);
            }
            _supportRequest.phone = stepContext.Context.Activity.Text;
            return await stepContext.PromptAsync(
               nameof(TextPrompt),
               new PromptOptions { Prompt = MessageFactory.Text("Por favor, exprese claramente su consulta") },
               cancellationToken
               );
        }

        private async Task<DialogTurnResult> FinalProcess(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (countErrors >= 3 || stepContext.Context.Activity.Text.ToLower().Trim().Equals("salir"))
            {
                await stepContext.Context.SendActivityAsync("Has superado el número de reintentos permitido, vamos a salir de este dialogo", cancellationToken: cancellationToken);
                stepContext.Context.Activity.Text = "Menu";
                return await stepContext.ReplaceDialogAsync(nameof(RootDialog), null, cancellationToken);
            }
            _supportRequest.request = stepContext.Context.Activity.Text;
            await _tableRepository.SaveSupportRequestData(SUPPORT_DATA_TABLE, _supportRequest);
            await stepContext.Context.SendActivityAsync("Consulta enviada, al departamento indicado, por favor, espere respuesta", cancellationToken: cancellationToken);
            return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
        }

        private Activity CreateDepartmentOptions(string message)
        {
            var answer = _tableRepository.getAssistantData(ASSISTANT_DATA_TABLE, "department").Result;
  
            List<CardAction> actions = new List<CardAction>();
            foreach (AssistantData ad in answer)
            {
                actions.Add(new CardAction() { Title = ad.value, Value = ad.key, Type = ActionTypes.ImBack });
            }
            var reply = MessageFactory.Text(message);
            reply.SuggestedActions = new SuggestedActions()
            {
                Actions = actions
            };
            return reply as Activity;
        }

        private List<String> GetDepartmentsList()
        {
            var answer = _tableRepository.getAssistantData(ASSISTANT_DATA_TABLE, "department").Result;

            List<String> departments = new List<String>();
            foreach (AssistantData ad in answer)
            {
                departments.Add(ad.value);
            }
            return departments;
        }
    }
}
