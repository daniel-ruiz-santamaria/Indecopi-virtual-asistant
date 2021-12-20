using IndecopiVirtualAssitant.Models.AzureTable;
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
        public RegisterDialog()
        {
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
            return await stepContext.PromptAsync(
                nameof(TextPrompt),
                new PromptOptions { Prompt = MessageFactory.Text("Para poder ayudarte mejor voy a necesitar que me facilite algunos datos:\n\n¿ Cual es su nombre completo ?")},
                cancellationToken
                );
        }

        private async Task<DialogTurnResult> SetDocumentType(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var fullName = stepContext.Context.Activity.Text;

            return await stepContext.PromptAsync(
                nameof(TextPrompt),
                new PromptOptions { Prompt = CreateButtonsDocumentType() },
                cancellationToken
            );
        }

        private async Task<DialogTurnResult> SetDocumentNumber(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var documentType = stepContext.Context.Activity.Text;

            return await stepContext.PromptAsync(
                nameof(TextPrompt),
                new PromptOptions { Prompt = MessageFactory.Text("Para terminar ingrese su número de documento") },
                cancellationToken
                );
        }

        private async Task<DialogTurnResult> Confirmation(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var documentNumber = stepContext.Context.Activity.Text;
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
                await stepContext.Context.SendActivityAsync("Pues de puta madre tio", cancellationToken: cancellationToken);
            } 
            else if(confirmation.ToLower().Equals("no")) 
            {
                await stepContext.Context.SendActivityAsync("No hay problema", cancellationToken: cancellationToken);
            } 
            else 
            {
                await stepContext.Context.SendActivityAsync("No hay problema", cancellationToken: cancellationToken);
            }
            return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
        }

        private Activity CreateButtonsConfirmation()
        {
            var reply = MessageFactory.Text("¿Confirmas tus datos?");
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
            List<CardAction> actions = new List<CardAction>();
            foreach (DocumentType dt in DocumentType.GetValues(typeof(DocumentType)))
            {
                actions.Add(new CardAction() { Title = Enums.GetDescription(dt), Value = dt.ToString(), Type = ActionTypes.ImBack });
            }
            var reply = MessageFactory.Text("¿Cual es su tipo de documento?");
            reply.SuggestedActions = new SuggestedActions()
            {
                Actions = actions
            };
            return reply as Activity;
        }
    }
}
