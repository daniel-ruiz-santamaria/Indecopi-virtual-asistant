using IndecopiVirtualAsistant.Dialogs;
using IndecopiVirtualAssitant.Common.Cards;
using IndecopiVirtualAssitant.Infraestructure.Luis;
using IndecopiVirtualAssitant.Infraestructure.QnAMakerAI;
using IndecopiVirtualAssitant.Models;
using IndecopiVirtualAssitant.Models.AzureTable;
using IndecopiVirtualAssitant.Repositories;
using IndecopiVirtualAssitant.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.QnA;
using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace IndecopiVirtualAssitant.Dialogs
{
    public class RootDialog: ComponentDialog
    {
        private readonly ILuisService _luisService;
        private readonly UserState _userState;
        private readonly State _state;
        private readonly SessionsData _sessionsData;
        private readonly IAzureTableRepository _tableRepository;
        private readonly IQnAMakerAIService _qnaMakerAIService;
        private const string ASSISTANT_DATA_TABLE = "asistantData";
        private const string AUDIT_TABLE = "audit";
        private const string ANSWERS_TABLE = "answers";

        public RootDialog(ILuisService luisService, IQnAMakerAIService qnaMakerAIService, UserState userState, State state ,SessionsData sessionsData, IAzureTableRepository tableRepository)
        {
            _luisService = luisService;
            _qnaMakerAIService = qnaMakerAIService;
            _userState = userState;
            _state = state;
            _sessionsData = sessionsData;
            _tableRepository = tableRepository;
            var waterfallSteps = new WaterfallStep[]
            {
                InitialProcess,
                FinalProcess
            };
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));

            AddDialog(new RegisterDialog(tableRepository, _sessionsData, _state));
            AddDialog(new ContactDialog(tableRepository, _sessionsData, _state));
            AddDialog(new CalificationDialog(tableRepository, _sessionsData, _state));
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> InitialProcess(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Obtengo el resultado de LUIS
            var luisResult = await _luisService._luisRecognizer.RecognizeAsync(stepContext.Context,cancellationToken);
            return await ManageIntentions(stepContext, luisResult, cancellationToken);

        }

        // Metodo para gestionar las intenciones de luis
        private async Task<DialogTurnResult> ManageIntentions(WaterfallStepContext stepContext, RecognizerResult luisResult, CancellationToken cancellationToken)
        {
            Audit audit = new Audit(_state);
            var topIntent = luisResult.GetTopScoringIntent();
            var resultQna = await _qnaMakerAIService._qnaMakerResult.GetAnswersAsync(stepContext.Context);
            var resultQnaAnsware = resultQna.FirstOrDefault();
            var score = (resultQna.FirstOrDefault()?.Score != null) ? resultQna.FirstOrDefault()?.Score : 0;
            audit.query = luisResult.Text;
            audit.intent = topIntent.intent;
            audit.score = topIntent.score;
            // var turnState = stepContext.Context.TurnState.Get[0];
            if (topIntent.score < 0.3 && score < 0.3)
            { // si no estoy seguro
                Console.Write("Score: {0}", topIntent.score);
                await IntentNoneLuis(stepContext, luisResult, cancellationToken, audit);
            }
            else if (audit.score >= 0.6 || (topIntent.score - score) > 0.2)
            {
                switch (topIntent.intent)
                {
                    case "Saludar":
                        await IntentSaludar(stepContext, luisResult, cancellationToken, audit);
                        break;
                    case "Agradecer":
                        await IntentAgradecer(stepContext, luisResult, cancellationToken, audit);
                        break;
                    case "Despedir":
                        await IntentDespedir(stepContext, luisResult, cancellationToken, audit);
                        break;
                    case "Chiste":
                        await IntentChiste(stepContext, luisResult, cancellationToken, audit);
                        break;
                    case "VerOpciones":
                        await IntentVerOpciones(stepContext, luisResult, cancellationToken, audit);
                        break;
                    case "Contactanos":
                        await IntentContatanos(stepContext, luisResult, cancellationToken, audit);
                        break;
                    case "RedesSociales":
                        await IntentRedesSociales(stepContext, luisResult, cancellationToken, audit);
                        break;
                    case "CanalesVirtuales":
                        await IntentCanalesVirtuales(stepContext, luisResult, cancellationToken, audit);
                        break;
                    case "HablarHumano":
                        await _tableRepository.SaveAuditData(AUDIT_TABLE, audit);
                        return await stepContext.BeginDialogAsync(nameof(ContactDialog), cancellationToken: cancellationToken);
                    case "Registrar":
                        await _tableRepository.SaveAuditData(AUDIT_TABLE, audit);
                        return await stepContext.BeginDialogAsync(nameof(RegisterDialog), cancellationToken: cancellationToken);
                    case "Calificar":
                        await _tableRepository.SaveAuditData(AUDIT_TABLE, audit);
                        return await stepContext.BeginDialogAsync(nameof(CalificationDialog), cancellationToken: cancellationToken);
                    case "DatosContacto":
                        await IntentVerDatosContacto(stepContext, luisResult, cancellationToken, audit);
                        break;
                    case "None":
                    if ((score - topIntent.score) > 0.2)
                    {
                        await IntentQnaMaker(stepContext, resultQnaAnsware, cancellationToken, audit);
                    }
                    else
                    {
                        await IntentNoneLuis(stepContext, luisResult, cancellationToken, audit);
                    }
                    break;
                    default:

                        if ((score - topIntent.score) > 0.2)
                        {
                            await IntentQnaMaker(stepContext, resultQnaAnsware, cancellationToken, audit);
                        }
                        else
                        {
                            await IntentNoneLuis(stepContext, luisResult, cancellationToken, audit);
                        }
                        break;
                    }
                } 
                else
                {
                    await IntentQnaMaker(stepContext, resultQnaAnsware, cancellationToken, audit);
                }
                return await stepContext.NextAsync(cancellationToken: cancellationToken);
        }



        /*
private async Task<DialogTurnResult> HandleIntent(WaterfallStepContext stepContext, RecognizerResult luisResult, CancellationToken cancellationToken) {
}
*/

        #region intentLuis
        private async Task IntentVerOpciones(WaterfallStepContext stepContext, RecognizerResult luisResult, CancellationToken cancellationToken, Audit audit)
        {
            audit.answer = await _tableRepository.getAnswer(ANSWERS_TABLE, audit.intent, "Estas son las opciones:");
            await _tableRepository.SaveAuditData(AUDIT_TABLE, audit);
            await stepContext.Context.SendActivityAsync(audit.answer, cancellationToken: cancellationToken);
            await MainOptionsCard.ToShow(_tableRepository,stepContext, cancellationToken);
        }
        private async Task IntentContatanos(WaterfallStepContext stepContext, RecognizerResult luisResult, CancellationToken cancellationToken, Audit audit)
        {

            audit.answer = await _tableRepository.getAnswer(ANSWERS_TABLE, audit.intent, "Datos de contacto:");
            await _tableRepository.SaveAuditData(AUDIT_TABLE, audit);
            await stepContext.Context.SendActivityAsync(audit.answer, cancellationToken: cancellationToken);
            await MainOptionsCard.ToShowContact(_tableRepository,stepContext, cancellationToken);
        }

        private async Task IntentRedesSociales(WaterfallStepContext stepContext, RecognizerResult luisResult, CancellationToken cancellationToken, Audit audit)
        {
            audit.answer = await _tableRepository.getAnswer(ANSWERS_TABLE, audit.intent, "Datos de contacto:");
            await _tableRepository.SaveAuditData(AUDIT_TABLE, audit);
            await stepContext.Context.SendActivityAsync(audit.answer, cancellationToken: cancellationToken);
            await MainOptionsCard.ToShowSocialNetworks(_tableRepository, stepContext, cancellationToken);
        }

        private async Task IntentCanalesVirtuales(WaterfallStepContext stepContext, RecognizerResult luisResult, CancellationToken cancellationToken, Audit audit)
        {
            audit.answer = await _tableRepository.getAnswer(ANSWERS_TABLE, audit.intent, "Datos de contacto:");
            await _tableRepository.SaveAuditData(AUDIT_TABLE, audit);
            await stepContext.Context.SendActivityAsync(audit.answer, cancellationToken: cancellationToken);
            await MainOptionsCard.ToShowVirtualChannels(_tableRepository, stepContext, cancellationToken);
        }

        private async Task IntentVerDatosContacto(WaterfallStepContext stepContext, RecognizerResult luisResult, CancellationToken cancellationToken, Audit audit)
        {

            var generalText = await _tableRepository.getAssistantData(ASSISTANT_DATA_TABLE, "contact", "generalText");
            List<String> textChuncks = await generateContactText();
            String fullText = String.Join("", textChuncks);
            audit.answer = fullText;
            await _tableRepository.SaveAuditData(AUDIT_TABLE, audit);
            foreach (String s in textChuncks) {
                await stepContext.Context.SendActivityAsync(s, cancellationToken: cancellationToken);
                await Task.Delay(1000);
            }
            var addressTexts = await _tableRepository.getAssistantData(ASSISTANT_DATA_TABLE, "contact", "phoneText");
            string addressStr = addressTexts[new Random().Next(0, addressTexts.Count)].value + Environment.NewLine;
            await stepContext.Context.SendActivityAsync(addressStr, cancellationToken: cancellationToken);
            await MainOptionsCard.ToShowAddresses(_tableRepository, stepContext, cancellationToken);
            await Task.Delay(1000);
            var telephoneTexts = await _tableRepository.getAssistantData(ASSISTANT_DATA_TABLE, "contact", "phoneText");
            string telephoneStr = telephoneTexts[new Random().Next(0, telephoneTexts.Count)].value + Environment.NewLine;
            await stepContext.Context.SendActivityAsync(telephoneStr, cancellationToken: cancellationToken);
            await MainOptionsCard.ToShowPhones(_tableRepository, stepContext, cancellationToken);
            await Task.Delay(1000);
            var emailText = await _tableRepository.getAssistantData(ASSISTANT_DATA_TABLE, "contact", "emailText");
            string emailStr = emailText[new Random().Next(0, emailText.Count)].value + Environment.NewLine;
            await stepContext.Context.SendActivityAsync(emailStr, cancellationToken: cancellationToken);
            await MainOptionsCard.ToShowMails(_tableRepository, stepContext, cancellationToken);
            await Task.Delay(1000);

            var socialNetworksText = await _tableRepository.getAssistantData(ASSISTANT_DATA_TABLE, "contact", "socialNetworksText");
            string socialNetworksStr = socialNetworksText[new Random().Next(0, socialNetworksText.Count)].value + Environment.NewLine;
            await stepContext.Context.SendActivityAsync(socialNetworksStr, cancellationToken: cancellationToken);
            await MainOptionsCard.ToShowSocialNetworks(_tableRepository, stepContext, cancellationToken);
            await Task.Delay(1000);

            var virtualChannelText = await _tableRepository.getAssistantData(ASSISTANT_DATA_TABLE, "contact", "virtualChannelText");
            string virtualChannelStr = virtualChannelText[new Random().Next(0, virtualChannelText.Count)].value + Environment.NewLine;
            await stepContext.Context.SendActivityAsync(virtualChannelStr, cancellationToken: cancellationToken);
            await MainOptionsCard.ToShowVirtualChannels(_tableRepository,stepContext, cancellationToken);
            await Task.Delay(1000);
            var schedule = await _tableRepository.getAssistantData(ASSISTANT_DATA_TABLE, "contact", "schedule");
            foreach (AssistantData ad in schedule)
            {
                await stepContext.Context.SendActivityAsync(ad.value, cancellationToken: cancellationToken);
                await Task.Delay(1000);
            }
        }
        private async Task IntentSaludar(WaterfallStepContext stepContext, RecognizerResult luisResult, CancellationToken cancellationToken, Audit audit)
        {
            audit.answer = await _tableRepository.getAnswer(ANSWERS_TABLE, audit.intent, "Hola, ¿en que puedo ayudarte?");
            await _tableRepository.SaveAuditData(AUDIT_TABLE, audit);
            await stepContext.Context.SendActivityAsync(audit.answer, cancellationToken: cancellationToken);
        }

        private async Task IntentAgradecer(WaterfallStepContext stepContext, RecognizerResult luisResult, CancellationToken cancellationToken, Audit audit)
        {
            audit.answer = await _tableRepository.getAnswer(ANSWERS_TABLE, audit.intent, "No te preocupes, me gusta ayudar");
            await _tableRepository.SaveAuditData(AUDIT_TABLE, audit);
            await stepContext.Context.SendActivityAsync(audit.answer, cancellationToken: cancellationToken);
        }

        private async Task IntentDespedir(WaterfallStepContext stepContext, RecognizerResult luisResult, CancellationToken cancellationToken, Audit audit)
        {
            audit.answer = await _tableRepository.getAnswer(ANSWERS_TABLE, audit.intent, "Espero verte proto");
            await _tableRepository.SaveAuditData(AUDIT_TABLE, audit);
            await stepContext.Context.SendActivityAsync(audit.answer, cancellationToken: cancellationToken);
        }
        private async Task IntentChiste(WaterfallStepContext stepContext, RecognizerResult luisResult, CancellationToken cancellationToken, Audit audit)
        {
            audit.answer = await _tableRepository.getAnswer(ANSWERS_TABLE, audit.intent, "Van dos y se cayo el de en medio");
            await _tableRepository.SaveAuditData(AUDIT_TABLE, audit);
            await stepContext.Context.SendActivityAsync(audit.answer, cancellationToken: cancellationToken);
        }
        private async Task IntentNoneLuis(WaterfallStepContext stepContext, RecognizerResult luisResult, CancellationToken cancellationToken, Audit audit)
        {
            audit.answer = await _tableRepository.getAnswer(ANSWERS_TABLE, audit.intent, "No te entiendo lo que me dices");
            await _tableRepository.SaveAuditData(AUDIT_TABLE, audit);
            await stepContext.Context.SendActivityAsync(audit.answer, cancellationToken: cancellationToken);
        }
        private async Task IntentQnaMaker(WaterfallStepContext stepContext, QueryResult queryResult, CancellationToken cancellationToken, Audit audit)
        {
            audit.intent = "QnAMaker";
            audit.score = (double)queryResult?.Score;
            audit.answer = queryResult.Answer;
            await _tableRepository.SaveAuditData(AUDIT_TABLE, audit);
            await MainOptionsCard.ToShowQnAResponse(stepContext, cancellationToken,queryResult);
            // await stepContext.Context.SendActivityAsync(audit.answer, cancellationToken: cancellationToken);
        }
        /*
        private async Task<DialogTurnResult> IntentRegistrar(WaterfallStepContext stepContext, RecognizerResult luisResult, CancellationToken cancellationToken, Audit audit)
        {
            await _tableRepository.SaveAuditData(AUDIT_TABLE, audit);
            return await stepContext.BeginDialogAsync(nameof(RegisterDialog), cancellationToken: cancellationToken);
            // await stepContext.BeginDialogAsync(nameof(RegisterDialog), cancellationToken: cancellationToken);
        }

        private async Task<DialogTurnResult> IntentCalificar(WaterfallStepContext stepContext, RecognizerResult luisResult, CancellationToken cancellationToken, Audit audit)
        {
            await _tableRepository.SaveAuditData(AUDIT_TABLE, audit);
            return await stepContext.BeginDialogAsync(nameof(RegisterDialog), cancellationToken: cancellationToken);
        }
        */


        #endregion

        private async Task<DialogTurnResult> FinalProcess(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }

        private async Task<List<String>> generateContactText() {
            List<String> contactText = new List<string>();
            var generalText = await _tableRepository.getAssistantData(ASSISTANT_DATA_TABLE, "contact", "generalText");
            contactText.Add(generalText[new Random().Next(0, generalText.Count)].value + Environment.NewLine);
            /*
             * 
             String addressStr;
            var addressTexts = await _tableRepository.getAssistantData(ASSISTANT_DATA_TABLE, "contact", "addressText");
            addressStr = addressTexts[new Random().Next(0, addressTexts.Count)].value + Environment.NewLine;
            var addresses = await _tableRepository.getAssistantData(ASSISTANT_DATA_TABLE, "contact", "address");
            foreach (AssistantData ad in addresses)
            {
                addressStr = addressStr + "\t 🏢 " + ad.value + Environment.NewLine;
            }
            contactText.Add(addressStr + Environment.NewLine);
            String telephoneStr;
            var telephoneTexts = await _tableRepository.getAssistantData(ASSISTANT_DATA_TABLE, "contact", "phoneText");
            telephoneStr = telephoneTexts[new Random().Next(0, telephoneTexts.Count)].value + Environment.NewLine;
            var telephoneNumbers = await _tableRepository.getAssistantData(ASSISTANT_DATA_TABLE, "contact", "telephone");
            foreach (AssistantData ad in telephoneNumbers) {
                telephoneStr = telephoneStr + "\t 📞 " + ad.value + Environment.NewLine;
            }
            contactText.Add(telephoneStr + Environment.NewLine);

            String emailStr;
            var emailTexts = await _tableRepository.getAssistantData(ASSISTANT_DATA_TABLE, "contact", "emailText");
            emailStr = emailTexts[new Random().Next(0, emailTexts.Count)].value + Environment.NewLine;
            var emails = await _tableRepository.getAssistantData(ASSISTANT_DATA_TABLE, "contact", "mail");
            foreach (AssistantData ad in emails)
            {
                emailStr = emailStr + "\t 📧 " + ad.value + Environment.NewLine;
            }
            contactText.Add(emailStr + Environment.NewLine);
            */

            return contactText;
        }

    }
}
