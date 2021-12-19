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
        private readonly IAzureTableRepository _tableRepository;
        private readonly IQnAMakerAIService _qnaMakerAIService;
        private const string ASSISTANT_DATA_TABLE = "asistantData";
        private const string AUDIT_TABLE = "audit";
        private const string ANSWERS_TABLE = "answers";
        private User logedUser;

        public RootDialog(ILuisService luisService, IQnAMakerAIService qnaMakerAIService, UserState userState, State state , IAzureTableRepository tableRepository)
        {
            _luisService = luisService;
            _qnaMakerAIService = qnaMakerAIService;
            _userState = userState;
            _state = state;
            _tableRepository = tableRepository;
            var waterfallSteps = new WaterfallStep[]
            {
                InitialProcess,
                FinalProcess
            };
            // AddDialog(new RegisterDialog());
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
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
            //    if (false && logedUser == null)
            //    {
            // 
            // return await stepContext.BeginDialogAsync(nameof(RegisterDialog), cancellationToken: cancellationToken);
            // } else { 
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
                } else
                {
                    await IntentQnaMaker(stepContext, resultQnaAnsware, cancellationToken, audit);
                }
                return await stepContext.NextAsync(cancellationToken: cancellationToken);
            // }
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
            await MainOptionsCard.ToShow(stepContext, cancellationToken);
        }
        private async Task IntentContatanos(WaterfallStepContext stepContext, RecognizerResult luisResult, CancellationToken cancellationToken, Audit audit)
        {

            audit.answer = await _tableRepository.getAnswer(ANSWERS_TABLE, audit.intent, "Datos de contacto:");
            await _tableRepository.SaveAuditData(AUDIT_TABLE, audit);
            await stepContext.Context.SendActivityAsync(audit.answer, cancellationToken: cancellationToken);
            await MainOptionsCard.ToShowContact(stepContext, cancellationToken);
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


        #endregion

        private async Task<DialogTurnResult> FinalProcess(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }

    }
}
