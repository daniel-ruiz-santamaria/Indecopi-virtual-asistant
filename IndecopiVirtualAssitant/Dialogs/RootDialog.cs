using IndecopiVirtualAssitant.Common.Cards;
using IndecopiVirtualAssitant.Infraestructure.Luis;
using Microsoft.Bot.Builder;
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

        public RootDialog(ILuisService luisService)
        {
            _luisService = luisService;
            var waterfallSteps = new WaterfallStep[]
            {
                InitialProcess,
                FinalProcess
            };
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
            var topIntent = luisResult.GetTopScoringIntent();
            if (topIntent.score < 0.5)
            { // si no estoy seguro
                Console.Write("Score: {0}", topIntent.score);
                await IntentNone(stepContext, luisResult, cancellationToken);
            }
            else
            {
                switch (topIntent.intent)
                {
                    case "Saludar":
                        await IntentSaludar(stepContext, luisResult, cancellationToken);
                        break;
                    case "Agradecer":
                        await IntentAgradecer(stepContext, luisResult, cancellationToken);
                        break;
                    case "Despedir":
                        await IntentDespedir(stepContext, luisResult, cancellationToken);
                        break;
                    case "Chiste":
                        await IntentChiste(stepContext, luisResult, cancellationToken);
                        break;
                    case "VerOpciones":
                        await IntentVerOpciones(stepContext, luisResult, cancellationToken);
                        break;
                    case "None":
                        await IntentNone(stepContext, luisResult, cancellationToken);
                        break;
                    default:
                        break;
                }
            }
            return await stepContext.NextAsync(cancellationToken: cancellationToken);
        }


        /*
        private async Task<DialogTurnResult> HandleIntent(WaterfallStepContext stepContext, RecognizerResult luisResult, CancellationToken cancellationToken) {
        }
        */

        #region intentLuis
        private async Task IntentVerOpciones(WaterfallStepContext stepContext, RecognizerResult luisResult, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync("Estas son las opciones", cancellationToken: cancellationToken);
            await MainOptionsCard.ToShow(stepContext, cancellationToken);
        }
        private async Task IntentSaludar(WaterfallStepContext stepContext, RecognizerResult luisResult, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync("Hola, ¿en que puedo ayudarte?", cancellationToken: cancellationToken);
        }

        private async Task IntentAgradecer(WaterfallStepContext stepContext, RecognizerResult luisResult, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync("No te preocupes, me gusta ayudar", cancellationToken: cancellationToken);
        }

        private async Task IntentDespedir(WaterfallStepContext stepContext, RecognizerResult luisResult, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync("Espero verte proto", cancellationToken: cancellationToken);
        }
        private async Task IntentChiste(WaterfallStepContext stepContext, RecognizerResult luisResult, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync("Van dos y se cayo el de en medio", cancellationToken: cancellationToken);
        }
        private async Task IntentNone(WaterfallStepContext stepContext, RecognizerResult luisResult, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync("No te entiendo lo que me dices", cancellationToken: cancellationToken);
        }


        #endregion

        private async Task<DialogTurnResult> FinalProcess(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }
    }
}
