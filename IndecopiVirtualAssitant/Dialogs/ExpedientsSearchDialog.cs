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
    public class ExpedientsSearchDialog : ComponentDialog 
    {
        private const string ANSWERS_TABLE = "answers";
        private const string USERS_TABLE = "users";
        private readonly SessionsData _sessionsData;
        private readonly State _state;
        private readonly ExpedientRequestService _expedientRequestService;
        private User _user;
        private string _name;
        private string _documentNumber;
        private string _year;
        private Dictionary<string, string> _input;

        private readonly string DlgDocumentId = "DocumentDialog";
        private readonly string DlgNameId = "NameDialog";
        private readonly string DlgYearId = "YearDialog";
        private int countErrors = 0;

        private readonly IAzureTableRepository _tableRepository;
        public ExpedientsSearchDialog(IAzureTableRepository tableRepository, ExpedientRequestService expedientRequestService, SessionsData sessionsData, State state, Dictionary<string,string> d)
        {
            _tableRepository = tableRepository;
            _expedientRequestService = expedientRequestService;
            _sessionsData = sessionsData;
            _state = state;
            _input = d;
            countErrors = 0;
            // _sessionState = sessionsData.getSesionState(_state.idSession);
            var waterFallStep = new WaterfallStep[] {
                ConfirmName,
                SetFullName,
                ConfirmDocument,
                SetDocumentNumber,
                ConfirmYear,
                SetYear,
                Confirmation,
                FinalProcess
            };
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterFallStep));
            AddDialog(new TextPrompt(DlgNameId, NameValidatorAsync));
            AddDialog(new TextPrompt(DlgDocumentId, DocumentNumberValidatorAsync));
            AddDialog(new TextPrompt(DlgYearId, YearValidatorAsync));
            AddDialog(new TextPrompt(nameof(TextPrompt)));
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
            countErrors = 0;
            if (countErrors >= 3 || stepContext.Context.Activity.Text.ToLower().Trim().Equals("salir"))
            {
                if (countErrors >= 3)
                {
                    await stepContext.Context.SendActivityAsync("Has superado el número de reintentos permitido, vamos a salir de este dialogo", cancellationToken: cancellationToken);
                }
                else
                {
                    await stepContext.Context.SendActivityAsync("La búsqueda ha sido cancelada", cancellationToken: cancellationToken);
                }
                stepContext.Context.Activity.Text = "Menu";
                return await stepContext.ReplaceDialogAsync(nameof(RootDialog), null, cancellationToken);
            }

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
            if (countErrors >= 3 || stepContext.Context.Activity.Text.ToLower().Trim().Equals("salir"))
            {
                if (countErrors >= 3)
                {
                    await stepContext.Context.SendActivityAsync("Has superado el número de reintentos permitido, vamos a salir de este dialogo", cancellationToken: cancellationToken);
                }
                else
                {
                    await stepContext.Context.SendActivityAsync("La búsqueda ha sido cancelada", cancellationToken: cancellationToken);
                }
                stepContext.Context.Activity.Text = "Menu";
                return await stepContext.ReplaceDialogAsync(nameof(RootDialog), null, cancellationToken);
            }

            else if (stepContext.Context.Activity.Text.ToLower().Equals("no") || _name == null)
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
                }
                else
                {
                    await stepContext.Context.SendActivityAsync("La búsqueda ha sido cancelada", cancellationToken: cancellationToken);
                }
                stepContext.Context.Activity.Text = "Menu";
                return await stepContext.ReplaceDialogAsync(nameof(RootDialog), null, cancellationToken);
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
                }
                else
                {
                    await stepContext.Context.SendActivityAsync("La búsqueda ha sido cancelada", cancellationToken: cancellationToken);
                }
                stepContext.Context.Activity.Text = "Menu";
                return await stepContext.ReplaceDialogAsync(nameof(RootDialog), null, cancellationToken);
            }

            else if (stepContext.Context.Activity.Text.ToLower().Equals("no") || _documentNumber == null)
            { // Si pulse NO en el boton, entonces pido los datos
                string text = await _tableRepository.getAnswer(ANSWERS_TABLE, "SetExpedientNumberSearchExpedient", "*Por favor introduce el nº de expediete a búscar");
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

        private async Task<bool> YearValidatorAsync(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            var response = promptContext.Context.Activity.Text;

            Regex regex = new Regex(@"^(19|20)*[0-9]{2}$");
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

        private async Task<DialogTurnResult> ConfirmYear(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (countErrors >= 3 || stepContext.Context.Activity.Text.ToLower().Trim().Equals("salir"))
            {
                if (countErrors >= 3)
                {
                    await stepContext.Context.SendActivityAsync("Has superado el número de reintentos permitido, vamos a salir de este dialogo", cancellationToken: cancellationToken);
                }
                else
                {
                    await stepContext.Context.SendActivityAsync("La búsqueda ha sido cancelada", cancellationToken: cancellationToken);
                }
                stepContext.Context.Activity.Text = "Menu";
                return await stepContext.ReplaceDialogAsync(nameof(RootDialog), null, cancellationToken);
            }
            if (countErrors >= 3 || stepContext.Context.Activity.Text.ToLower().Trim().Equals("salir"))
            {
                if (countErrors >= 3)
                {
                    await stepContext.Context.SendActivityAsync("Has superado el número de reintentos permitido, vamos a salir de este dialogo", cancellationToken: cancellationToken);
                }
                else
                {
                    await stepContext.Context.SendActivityAsync("La búsqueda ha sido cancelada", cancellationToken: cancellationToken);
                }
                stepContext.Context.Activity.Text = "Menu";
                return await stepContext.ReplaceDialogAsync(nameof(RootDialog), null, cancellationToken);
            }

            if (!stepContext.Context.Activity.Text.ToLower().Equals("si"))
            {
                _documentNumber = stepContext.Context.Activity.Text;
            }

            if (_input.ContainsKey("Year")) // Si los datos contienen un valor
            {
                _year = _input["Year"];
                return await stepContext.PromptAsync(
                    nameof(TextPrompt),
                    new PromptOptions { Prompt = CreateButtonsConfirmation(_year, "ExpedientYearComfirmation", "* Usted ha indicado el año: **$$value$$** para realizar la busqueda, ¿es correcto?") },
                    cancellationToken
                );
            }
            else // Si no lo contienen paso a pedirlo
            {
                return await stepContext.NextAsync("No", cancellationToken: cancellationToken);
            }
        }

        private async Task<DialogTurnResult> SetYear(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (countErrors >= 3 || stepContext.Context.Activity.Text.ToLower().Trim().Equals("salir"))
            {
                if (countErrors >= 3)
                {
                    await stepContext.Context.SendActivityAsync("Has superado el número de reintentos permitido, vamos a salir de este dialogo", cancellationToken: cancellationToken);
                }
                else
                {
                    await stepContext.Context.SendActivityAsync("La búsqueda ha sido cancelada", cancellationToken: cancellationToken);
                }
                stepContext.Context.Activity.Text = "Menu";
                return await stepContext.ReplaceDialogAsync(nameof(RootDialog), null, cancellationToken);
            }

            else if (stepContext.Context.Activity.Text.ToLower().Equals("no") || _year == null)
            { // Si pulse NO en el boton, entonces pido los datos
                string text = await _tableRepository.getAnswer(ANSWERS_TABLE, "SetYearSearchExpedients", "*Por favor introduce un año para realizar la búsqueda de expedientes (la longitud ha de ser de 2 o 4 dígitos");
                return await stepContext.PromptAsync(
                    DlgYearId,
                    new PromptOptions { 
                        Prompt = MessageFactory.Text(text.Replace("\\n", "\n")),
                        RetryPrompt = MessageFactory.Text("Formato de año no valido, 4 o 2 digitos para el año")
                    },
                    cancellationToken
                    );
            }
            else
            { // Saltamos al siguiente
                return await stepContext.NextAsync(_year, cancellationToken: cancellationToken);
            }
        }

        private async Task<DialogTurnResult> Confirmation(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (countErrors >= 3 || stepContext.Context.Activity.Text.ToLower().Trim().Equals("salir"))
            {
                if (countErrors >= 3)
                {
                    await stepContext.Context.SendActivityAsync("Has superado el número de reintentos permitido, vamos a salir de este dialogo", cancellationToken: cancellationToken);
                }
                else
                {
                    await stepContext.Context.SendActivityAsync("La búsqueda ha sido cancelada", cancellationToken: cancellationToken);
                }
                stepContext.Context.Activity.Text = "Menu";
                return await stepContext.ReplaceDialogAsync(nameof(RootDialog), null, cancellationToken);
            }

            if (!stepContext.Context.Activity.Text.ToLower().Equals("si"))
            {
                _year = stepContext.Context.Activity.Text;
                if (_year.Length == 2) {
                    string prefix = new string(DateTime.Now.Year.ToString().Take(2).ToArray());
                    _year = prefix + _year;
                }
            }

            return await stepContext.PromptAsync(
                nameof(TextPrompt),
                new PromptOptions { Prompt = CreateButtonsConfirmationFinal() },
                cancellationToken
                );
        }

        private async Task<DialogTurnResult> FinalProcess(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var confirmation = stepContext.Context.Activity.Text;
            if (confirmation.ToLower().Equals("si")) 
            {
                string text = await _tableRepository.getAnswer(ANSWERS_TABLE, "ExpedientsComfirmedBeforeSearch", "* Has confirmado tus datos, estoy realizando la búsqueda del estado de tu expediente....");

                await stepContext.Context.SendActivityAsync(text.Replace("\\n", "\n"), cancellationToken: cancellationToken);

                List<Dictionary<string,string>> response =  await _expedientRequestService.SearchExpedientsByYear(_name, _documentNumber, _year);

                if (response == null || response.Count == 0) {
                    text = await _tableRepository.getAnswer(ANSWERS_TABLE, "ExpedientSearchNotFound", "* No hemos encontrado ningún resultado para el expediente $$expedient$$, con nombre de titular $$name$$ y el año $$year$$");
                    text = new Regex("\\$\\$name\\$\\$").Replace(text.ToString(), _name);
                    text = new Regex("\\$\\$document\\$\\$").Replace(text.ToString(), _documentNumber);
                    text = new Regex("\\$\\$year\\$\\$").Replace(text.ToString(), _year);
                    await stepContext.Context.SendActivityAsync(text.Replace("\\n", "\n"), cancellationToken: cancellationToken);
                } else
                {
                    text = await _tableRepository.getAnswer(ANSWERS_TABLE, "ExpedientsSearchFound", "* He encontrado los siguientes resultados para el expediente $$expedient$$, con nombre de titular $$name$$ y el año $$year$$");
                    text = new Regex("\\$\\$name\\$\\$").Replace(text.ToString(), _name);
                    text = new Regex("\\$\\$document\\$\\$").Replace(text.ToString(), _documentNumber);
                    text = new Regex("\\$\\$year\\$\\$").Replace(text.ToString(), _year);
                    await stepContext.Context.SendActivityAsync(text.Replace("\\n", "\n"), cancellationToken: cancellationToken);


                    foreach (Dictionary<string, string> item in response)
                    {
                        text = await _tableRepository.getAnswer(ANSWERS_TABLE, "ExpedientsSearchFoundMainText", "* Encontrado el expediente: **$$expedientNumber$$**, de tipo: **$$requestType$$**, con signo: **$$signType$$**");

                        string expedientNumber = item.ContainsKey("expedientNumber") ?item["expedientNumber"] :"No especificado";
                        text = new Regex("\\$\\$expedientNumber\\$\\$").Replace(text.ToString(), expedientNumber);

                        string requestType = item.ContainsKey("requestType") ? item["requestType"] : "No especificado";
                        text = new Regex("\\$\\$requestType\\$\\$").Replace(text.ToString(), requestType);

                        string signType = item.ContainsKey("signType") ? item["signType"] : "No especificado";
                        text = new Regex("\\$\\$signType\\$\\$").Replace(text.ToString(), signType);

                        string state = item.ContainsKey("text") ? item["text"] : "No especificado";
                        text = new Regex("\\$\\$state\\$\\$").Replace(text.ToString(), state);

                        await stepContext.Context.SendActivityAsync(text.Replace("\\n", "\n").Replace("\n", Environment.NewLine), cancellationToken: cancellationToken);
                        Thread.Sleep(1000);
                    }
                    return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);

                }
            } 
            else if(confirmation.ToLower().Equals("no")) 
            {
                string text = await _tableRepository.getAnswer(ANSWERS_TABLE, "ExpedientNotComfirmedBeforeSearch", "* Has cancelado tus datos, vamos a volver a realizar al búsqueda ");
                await stepContext.Context.SendActivityAsync(text.Replace("\\n", "\n"), cancellationToken: cancellationToken);
                return await stepContext.ReplaceDialogAsync(nameof(ExpedientsSearchDialog), cancellationToken: cancellationToken);
            }
            else if (confirmation.ToLower().Equals("cancelar"))
            {
                string text = await _tableRepository.getAnswer(ANSWERS_TABLE, "ExpedientsCancelComfirmedBeforeSearch", "Has cancelado la búsqueda del estado del expediente");
                await stepContext.Context.SendActivityAsync(text.Replace("\\n", "\n"), cancellationToken: cancellationToken);
            }
            else 
            {
                string text = await _tableRepository.getAnswer(ANSWERS_TABLE, "ExpedientsAbortComfirmedBeforeSearch", "* Búsqueda abortada");
                await stepContext.Context.SendActivityAsync(text.Replace("\\n", "\n"), cancellationToken: cancellationToken);
            }
            return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
        }

        private Activity CreateItemActivity(string text)
        {
            var reply = MessageFactory.Text(text);
            reply.SuggestedActions = new SuggestedActions()
            {
                Actions = new List<CardAction>()
                {
                    new CardAction() { Title = "Mostrar expediente", Value = "Mostrar expediente", Type = ActionTypes.ImBack }
                }
            };
            return reply as Activity;
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

            var answer = _tableRepository.getAnswer(ANSWERS_TABLE, "ExpedientsComfirmation", "¿Quieres confirmar y realizar la busqueda de el expediente?");
            answer.Wait();
            string answerFull = answer.Result.ToString()+ "\n\tNombre: **" + _name + "**\n\tNº Documento: **" + _documentNumber + "**\n\tAño: **" + _year + "**";
            var reply = MessageFactory.Text(answerFull);
            reply.SuggestedActions = new SuggestedActions()
            {
                Actions = new List<CardAction>()
                {
                    new CardAction() { Title = "Si, quiero buscar con estos datos", Value = "Si", Type = ActionTypes.ImBack },
                    new CardAction() { Title = "No, quiero volver a introducir los datos", Value = "No", Type = ActionTypes.ImBack },
                    new CardAction() { Title = "Quiero cancelar la búsqueda", Value = "Cancelar", Type = ActionTypes.ImBack }
                }
            };
            return reply as Activity;
        }

    }
}
