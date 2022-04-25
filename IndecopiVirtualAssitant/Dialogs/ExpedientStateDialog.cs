using IndecopiVirtualAsistant.Services;
using IndecopiVirtualAssitant.Models;
using IndecopiVirtualAssitant.Models.AzureTable;
using IndecopiVirtualAssitant.Repositories;
using IndecopiVirtualAssitant.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using static IndecopiVirtualAssitant.Models.AzureTable.Enums;

namespace IndecopiVirtualAssitant.Dialogs
{
    public class ExpedientStateDialog : ComponentDialog 
    {
        private const string ANSWERS_TABLE = "answers";
        private const string USERS_TABLE = "users";
        private readonly SessionsData _sessionsData;
        private readonly State _state;
        private readonly ExpedientRequestService _expedientRequestService;
        private User _user;
        private string _name;
        private string _documentNumber;
        private string _expedientNumber;
        private Dictionary<string, string> _input;

        private readonly string DlgDocumentId = "DocumentDialog";
        private readonly string DlgNameId = "NameDialog";
        private readonly string DlgExpedientId = "ExpedientDialog";
        private int countErrors = 0;

        private readonly IAzureTableRepository _tableRepository;
        public ExpedientStateDialog(IAzureTableRepository tableRepository, ExpedientRequestService expedientRequestService, SessionsData sessionsData, State state, Dictionary<string,string> d)
        {
            _tableRepository = tableRepository;
            _expedientRequestService = expedientRequestService;
            _sessionsData = sessionsData;
            _state = state;
            _input = d;
            countErrors = 0;
            // _sessionState = sessionsData.getSesionState(_state.idSession);
            var waterFallStep = new WaterfallStep[] {
                Initialize,
                ConfirmName,
                SetFullName,
                ConfirmDocument,
                SetDocumentNumber,
                ConfirmExpedientNumber,
                SetExpedientNumber,
                Confirmation,
                FinalProcess
            };
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterFallStep));
            AddDialog(new TextPrompt(DlgNameId, NameValidatorAsync));
            AddDialog(new TextPrompt(DlgDocumentId, DocumentNumberValidatorAsync));
            AddDialog(new TextPrompt(DlgExpedientId, ExpedientNumberValidatorAsync));
            AddDialog(new TextPrompt(nameof(TextPrompt)));
        }

        private async Task<DialogTurnResult> Initialize(WaterfallStepContext stepContext, CancellationToken cancellationToken) {
            countErrors = 0;
            _name = null;
            _documentNumber = null;
            _expedientNumber = null;
            return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
        }

        private async Task<bool> NameValidatorAsync(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            var response = promptContext.Context.Activity.Text;

            Regex regex = new Regex(@"^\b([A-ZÀ-ÿa-z][-,a-z. ']+[ ]*)+$");
            Match match = regex.Match(response.ToString());
            if (response != null && match.Success)
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



        private async Task<DialogTurnResult> ConfirmName(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (_input.ContainsKey("Name")) // Si los datos contienen un valor
            {
                _name = _input["Name"];
                return await stepContext.PromptAsync(
                    nameof(TextPrompt),
                    new PromptOptions { Prompt = CreateButtonsConfirmation(_name, "ExpedientNameComfirmation", "* Usted se ha registrado con el nombre: $$value$$,  **el nombre ha de coincidir con el usado al presentar la solicitud** ¿Quieres usar ese nombre para la búsqueda del estado del espediente?") },
                    cancellationToken
                );
            } else // Si no lo contienen paso a pedirlo
            {
                return await stepContext.NextAsync("No", cancellationToken: cancellationToken);
            }
        }

        private async Task<DialogTurnResult> SetFullName(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (countErrors > 3 || stepContext.Context.Activity.Text.ToLower().Trim().Equals("salir"))
            {
                if (countErrors > 3)
                {
                    await stepContext.Context.SendActivityAsync("Has superado el número de reintentos permitido, vamos a salir de este dialogo", cancellationToken: cancellationToken);
                    stepContext.Context.Activity.Text = "Menu";
                    return await stepContext.ReplaceDialogAsync(nameof(RootDialog), null, cancellationToken);
                }
                else
                {
                    await stepContext.Context.SendActivityAsync("La búsqueda ha sido cancelada", cancellationToken: cancellationToken);
                    return await stepContext.ReplaceDialogAsync(nameof(RootDialog), null, cancellationToken);
                }
            }

            if (stepContext.Context.Activity.Text.ToLower().Equals("no") || _name == null)
            { // Si pulse NO en el boton, entonces pido los datos
                string text = await _tableRepository.getAnswer(ANSWERS_TABLE, "SetNameSearchExpedient", "*Por favor introduce el nombre que figura en tu expediente");
                return await stepContext.PromptAsync(
                    DlgNameId,
                    new PromptOptions { 
                        Prompt = MessageFactory.Text(text.Replace("\\n", "\n")),
                        RetryPrompt = MessageFactory.Text("Formato de nombre invalido, por favor introduce tu nombre, usando mayusculas SOLO en la primera letra de cada palabra")
                    },
                    cancellationToken
                    );
            } else
            { // Saltamos al siguiente
                return await stepContext.NextAsync(_name, cancellationToken: cancellationToken);
            }
        }

        private async Task<bool> DocumentNumberValidatorAsync(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            var response = promptContext.Context.Activity.Text;

            Regex regex = new Regex(@"^[a-zA-Z]*(-)*\d+(-)*[a-zA-Z]*$");
            Match match = regex.Match(response.ToString());
            if (response != null && match.Success)
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

        private async Task<DialogTurnResult> ConfirmDocument(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (countErrors >= 3 || stepContext.Context.Activity.Text.ToLower().Trim().Equals("salir"))
            {
                if (countErrors >= 3)
                {
                    await stepContext.Context.SendActivityAsync("Has superado el número de reintentos permitido, vamos a salir de este dialogo", cancellationToken: cancellationToken);
                    stepContext.Context.Activity.Text = "Menu";
                    return await stepContext.ReplaceDialogAsync(nameof(RootDialog), null, cancellationToken);
                }
                else
                {
                    await stepContext.Context.SendActivityAsync("La búsqueda ha sido cancelada", cancellationToken: cancellationToken);
                    return await stepContext.ReplaceDialogAsync(nameof(RootDialog), null, cancellationToken);
                }
            }

            if (!stepContext.Context.Activity.Text.ToLower().Equals("si"))
            {
                _name = stepContext.Context.Activity.Text;
            }

            if (_input.ContainsKey("Document")) // Si los datos contienen un valor
            {
                _documentNumber = _input["Document"];
                return await stepContext.PromptAsync(
                    nameof(TextPrompt),
                    new PromptOptions { Prompt = CreateButtonsConfirmation(_documentNumber, "ExpedientDocumentComfirmation", "* Usted se ha registrado con el nº documento: $$value$$,  **el nº documento ha de coincidir con el usado al presentar la solicitud** ¿Quieres usar ese nº de documento para la búsqueda del estado del espediente?") },
                    cancellationToken
                );
            }
            else // Si no lo contienen paso a pedirlo
            {
                return await stepContext.NextAsync("No", cancellationToken: cancellationToken);
            }
        }

        private async Task<DialogTurnResult> SetDocumentNumber(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (countErrors >= 3 || stepContext.Context.Activity.Text.ToLower().Trim().Equals("salir"))
            {
                if (countErrors >= 3)
                {
                    await stepContext.Context.SendActivityAsync("Has superado el número de reintentos permitido, vamos a salir de este dialogo", cancellationToken: cancellationToken);
                    stepContext.Context.Activity.Text = "Menu";
                    return await stepContext.ReplaceDialogAsync(nameof(RootDialog), null, cancellationToken);
                }
                else
                {
                    await stepContext.Context.SendActivityAsync("La búsqueda ha sido cancelada", cancellationToken: cancellationToken);
                    return await stepContext.ReplaceDialogAsync(nameof(RootDialog), null, cancellationToken);
                }
            }

            if (stepContext.Context.Activity.Text.ToLower().Equals("no") || _documentNumber == null)
            { // Si pulse NO en el boton, entonces pido los datos
                string text = await _tableRepository.getAnswer(ANSWERS_TABLE, "SetDocumentNumberSearchExpedient", "*Por favor introduce el **nº de documento** a búscar");
                return await stepContext.PromptAsync(
                    DlgDocumentId,
                    new PromptOptions { 
                        Prompt = MessageFactory.Text(text.Replace("\\n", "\n")),
                        RetryPrompt = MessageFactory.Text("Formato de documento no valido, por favor introducelo de nuevo")
                    },
                    cancellationToken
                    );
            }
            else
            { // Saltamos al siguiente
                return await stepContext.NextAsync(_documentNumber, cancellationToken: cancellationToken);
            }
        }

        private async Task<bool> ExpedientNumberValidatorAsync(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            var response = promptContext.Context.Activity.Text;

            Regex regex = new Regex(@"^((\d+)+(-\d+)?)+$");
            Match match = regex.Match(response.ToString());
            if (response != null && match.Success)
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

        private async Task<DialogTurnResult> ConfirmExpedientNumber(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (countErrors >= 3 || stepContext.Context.Activity.Text.ToLower().Trim().Equals("salir"))
            {
                if (countErrors >= 3)
                {
                    await stepContext.Context.SendActivityAsync("Has superado el número de reintentos permitido, vamos a salir de este dialogo", cancellationToken: cancellationToken);
                    stepContext.Context.Activity.Text = "Menu";
                    return await stepContext.ReplaceDialogAsync(nameof(RootDialog), null, cancellationToken);
                }
                else
                {
                    await stepContext.Context.SendActivityAsync("La búsqueda ha sido cancelada", cancellationToken: cancellationToken);
                    return await stepContext.ReplaceDialogAsync(nameof(RootDialog), null, cancellationToken);
                }
            }

            if (!stepContext.Context.Activity.Text.ToLower().Equals("si"))
            {
                _documentNumber = stepContext.Context.Activity.Text;
            }

            if (_input.ContainsKey("Expedient")) // Si los datos contienen un valor
            {
                _expedientNumber = _input["Expedient"];
                return await stepContext.PromptAsync(
                    nameof(TextPrompt),
                    new PromptOptions { Prompt = CreateButtonsConfirmation(_expedientNumber, "ExpedientExpedientComfirmation", "* Usted ha indicado el número de expediente: $$value$$,  **el nº de expediente ha de coincidir con el usado al presentar la solicitud** ¿Quieres usar ese nº de expediente para la búsqueda del estado del espediente?") },
                    cancellationToken
                );
            }
            else // Si no lo contienen paso a pedirlo
            {
                return await stepContext.NextAsync("No", cancellationToken: cancellationToken);
            }
        }

        private async Task<DialogTurnResult> SetExpedientNumber(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (countErrors >= 3 || stepContext.Context.Activity.Text.ToLower().Trim().Equals("salir"))
            {
                if (countErrors >= 3)
                {
                    await stepContext.Context.SendActivityAsync("Has superado el número de reintentos permitido, vamos a salir de este dialogo", cancellationToken: cancellationToken);
                    stepContext.Context.Activity.Text = "Menu";
                    return await stepContext.ReplaceDialogAsync(nameof(RootDialog), null, cancellationToken);
                }
                else
                {
                    await stepContext.Context.SendActivityAsync("La búsqueda ha sido cancelada", cancellationToken: cancellationToken);
                    return await stepContext.ReplaceDialogAsync(nameof(RootDialog), null, cancellationToken);
                }
            }

            if (stepContext.Context.Activity.Text.ToLower().Equals("no") || _expedientNumber == null)
            { // Si pulse NO en el boton, entonces pido los datos
                string text = await _tableRepository.getAnswer(ANSWERS_TABLE, "SetExpedientNumberSearchExpedient", "*Por favor introduce el nº expediente que figura en tu expediente");
                return await stepContext.PromptAsync(
                    DlgExpedientId,
                    new PromptOptions { 
                        Prompt = MessageFactory.Text(text.Replace("\\n", "\n")),
                        RetryPrompt = MessageFactory.Text("Formato de expediente no valido, por favor introducelo de nuevo")
                    },
                    cancellationToken
                    );
            }
            else
            { // Saltamos al siguiente
                return await stepContext.NextAsync(_expedientNumber, cancellationToken: cancellationToken);
            }
        }

        private async Task<DialogTurnResult> Confirmation(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (countErrors >= 3 || stepContext.Context.Activity.Text.ToLower().Trim().Equals("salir"))
            {
                if (countErrors >= 3)
                {
                    await stepContext.Context.SendActivityAsync("Has superado el número de reintentos permitido, vamos a salir de este dialogo", cancellationToken: cancellationToken);
                    stepContext.Context.Activity.Text = "Menu";
                    return await stepContext.ReplaceDialogAsync(nameof(RootDialog), null, cancellationToken);
                }
                else
                {
                    await stepContext.Context.SendActivityAsync("La búsqueda ha sido cancelada", cancellationToken: cancellationToken);
                    return await stepContext.ReplaceDialogAsync(nameof(RootDialog), null, cancellationToken);
                }
            }

            if (!stepContext.Context.Activity.Text.ToLower().Equals("si"))
            {
                _expedientNumber = stepContext.Context.Activity.Text;
            }

            return await stepContext.PromptAsync(
                nameof(TextPrompt),
                new PromptOptions { Prompt = CreateButtonsConfirmationFinal() },
                cancellationToken
                );
        }

        private async Task<DialogTurnResult> FinalProcess(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (countErrors > 3 || stepContext.Context.Activity.Text.ToLower().Trim().Equals("salir"))
            {
                await stepContext.Context.SendActivityAsync("Has superado el número de reintentos permitido, vamos a salir de este dialogo", cancellationToken: cancellationToken);
                stepContext.Context.Activity.Text = "Menu";
                return await stepContext.ReplaceDialogAsync(nameof(RootDialog), null, cancellationToken);
            }
            var confirmation = stepContext.Context.Activity.Text;
            if (confirmation.ToLower().Equals("si")) 
            {
                string text = await _tableRepository.getAnswer(ANSWERS_TABLE, "ExpedientComfirmedBeforeSearch", "* Has confirmado tus datos, estoy realizando la búsqueda del estado de tu expediente....");
                await stepContext.Context.SendActivityAsync(text.Replace("\\n", "\n"), cancellationToken: cancellationToken);

                Dictionary<string,string> response =  await _expedientRequestService.SearchExpedientState(_name, _documentNumber, _expedientNumber);

                if (response == null || (response["state"] == null && response["term"] == null)) {
                    text = await _tableRepository.getAnswer(ANSWERS_TABLE, "ExpedientSearchNotFound", "* No hemos encontrado ningún resultado para el expediente $$expedient$$, con nombre de titular $$name$$ y número de documento $$document$$");
                    text = new Regex("\\$\\$name\\$\\$").Replace(text.ToString(), _name);
                    text = new Regex("\\$\\$document\\$\\$").Replace(text.ToString(), _documentNumber);
                    text = new Regex("\\$\\$expedient\\$\\$").Replace(text.ToString(), _expedientNumber);
                    await stepContext.Context.SendActivityAsync(text.Replace("\\n", "\n"), cancellationToken: cancellationToken);
                } else
                {
                    text = await _tableRepository.getAnswer(ANSWERS_TABLE, "ExpedientSearchFound", "* He encontrado el siguiente resultado para el expediente $$expedient$$, con nombre de titular $$name$$ y número de documento $$document$$");
                    text = new Regex("\\$\\$name\\$\\$").Replace(text.ToString(), _name);
                    text = new Regex("\\$\\$document\\$\\$").Replace(text.ToString(), _documentNumber);
                    text = new Regex("\\$\\$expedient\\$\\$").Replace(text.ToString(), _expedientNumber);
                    await stepContext.Context.SendActivityAsync(text.Replace("\\n", "\n"), cancellationToken: cancellationToken);

                    if (response.ContainsKey("state")) {
                        Thread.Sleep(1000);
                        string state = response["state"];
                        text = await _tableRepository.getAnswer(ANSWERS_TABLE, "ExpedientSearchFoundStatus", "* Es estado del expediente es: $$state$$");
                        text = new Regex("\\$\\$state\\$\\$").Replace(text.ToString(), state);
                        await stepContext.Context.SendActivityAsync(text.Replace("\\n", "\n"), cancellationToken: cancellationToken);
                    }

                    if (response.ContainsKey("term"))
                    {
                        Thread.Sleep(1000);
                        string term = response["term"];
                        text = await _tableRepository.getAnswer(ANSWERS_TABLE, "ExpedientSearchFoundTerm", "* Es plazo del expediente es: $$term$$");
                        text = new Regex("\\$\\$term\\$\\$").Replace(text.ToString(), term);
                        await stepContext.Context.SendActivityAsync(text.Replace("\\n", "\n"), cancellationToken: cancellationToken);
                    }
                }
            } 
            else if(confirmation.ToLower().Equals("no")) 
            {
                string text = await _tableRepository.getAnswer(ANSWERS_TABLE, "ExpedientNotComfirmedBeforeSearch", "* Has cancelado tus datos, vamos a volver a realizar al búsqueda ");
                await stepContext.Context.SendActivityAsync(text.Replace("\\n", "\n"), cancellationToken: cancellationToken);
                return await stepContext.ReplaceDialogAsync(nameof(ExpedientStateDialog), cancellationToken: cancellationToken);
            }
            else if (confirmation.ToLower().Equals("cancelar"))
            {
                string text = await _tableRepository.getAnswer(ANSWERS_TABLE, "ExpedientCancelComfirmedBeforeSearch", "Has cancelado la búsqueda del estado del expediente");
                await stepContext.Context.SendActivityAsync(text.Replace("\\n", "\n"), cancellationToken: cancellationToken);
            }
            else 
            {
                string text = await _tableRepository.getAnswer(ANSWERS_TABLE, "ExpedientAbortComfirmedBeforeSearch", "Registro abortado");
                await stepContext.Context.SendActivityAsync(text.Replace("\\n", "\n"), cancellationToken: cancellationToken);
            }
            return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
        }

        private Activity CreateButtonsConfirmation(string value, string key, string defalt)
        {
            var answerName = _tableRepository.getAnswer(ANSWERS_TABLE, key, defalt);
            answerName.Wait();
            string answerNameClear = new Regex("\\$\\$[^\\$]*\\$\\$").Replace(answerName.Result.ToString(), value);
            var reply = MessageFactory.Text(answerNameClear);
            reply.SuggestedActions = new SuggestedActions()
            {
                Actions = new List<CardAction>()
                {
                    new CardAction() { Title = "Si, quiero usar " + value, Value = "Si", Type = ActionTypes.ImBack },
                    new CardAction() { Title = "No, quiero introducir otro", Value = "No", Type = ActionTypes.ImBack },
                    new CardAction() { Title = "Cancelar", Value = "Salir", Type = ActionTypes.ImBack }
                }
            };
            return reply as Activity;
        }

        private Activity CreateButtonsConfirmationFinal()
        {

            var answer = _tableRepository.getAnswer(ANSWERS_TABLE, "ExpedientComfirmation", "¿Quieres confirmar y realizar la busqueda de el expediente?");
            answer.Wait();
            string answerFull = answer.Result.ToString()+ "\n\tNombre: **" + _name + "**\n\tNº Documento: **" + _documentNumber + "**\n\tNº Expediente: **" + _expedientNumber+ "**";
            var reply = MessageFactory.Text(answerFull);
            reply.SuggestedActions = new SuggestedActions()
            {
                Actions = new List<CardAction>()
                {
                    new CardAction() { Title = "Si, quiero buscar este expediente", Value = "Si", Type = ActionTypes.ImBack },
                    new CardAction() { Title = "No, quiero volver a introducir los datos", Value = "No", Type = ActionTypes.ImBack },
                    new CardAction() { Title = "Quiero cancelar la búsqueda", Value = "Cancelar", Type = ActionTypes.ImBack }
                }
            };
            return reply as Activity;
        }

    }
}
