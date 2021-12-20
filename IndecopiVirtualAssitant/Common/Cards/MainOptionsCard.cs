using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.QnA;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace IndecopiVirtualAssitant.Common.Cards
{
    public class MainOptionsCard
    {

        public static async Task ToShow(DialogContext stepContext, CancellationToken cancellationToken) {
            await stepContext.Context.SendActivityAsync(activity: CreateCarousel(), cancellationToken);
        }

        public static async Task ToShowContact(DialogContext stepContext, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync(activity: CreateCarouselContactOptions(), cancellationToken);
        }

        public static async Task ToShowQnAResponse(DialogContext stepContext, CancellationToken cancellationToken, QueryResult queryResult)
        {
            List<CardAction> cardActions = new List<CardAction>();
            if (queryResult.Context.Prompts.Length > 0)
            {
                foreach (var p in queryResult.Context.Prompts)
                {
                    cardActions.Add(new CardAction() { Title = p.DisplayText, Value = p.DisplayText, Type = ActionTypes.ImBack });
                }
                var cardContact = new HeroCard
                {
                    Title = "FAQs",
                    Subtitle = stepContext.Context.Activity.Text,
                    Text = queryResult.Answer,
                    Images = new List<CardImage> { new CardImage("https://storagepoc5.blob.core.windows.net/images/FAQs.png") },
                    Buttons = cardActions
                };

                var optionAttachments = new List<Attachment>() {
                cardContact.ToAttachment()
            };

                var reply = MessageFactory.Attachment(optionAttachments);
                reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                Activity replyActivity = reply as Activity;
                await stepContext.Context.SendActivityAsync(activity: replyActivity, cancellationToken);
            } else
            {
                await stepContext.Context.SendActivityAsync(queryResult.Answer, cancellationToken: cancellationToken);
            }
        }

        private static Activity CreateCarousel() 
        {
            var cardFAQs = new HeroCard
            { 
                Title = "Preguntas fecuentes",
                Subtitle = "Opciones",
                Images = new List<CardImage> { new CardImage("https://storagepoc5.blob.core.windows.net/images/menu-1.png") },
                Buttons = new List<CardAction>()
                { 
                    new CardAction(){ Title = "Ver preguntas frecuentes",Value = "Ayuda",Type = ActionTypes.ImBack }
                }
            };
            var cardProcess = new HeroCard
            {
                Title = "Tramites",
                Subtitle = "Opciones",
                Images = new List<CardImage> { new CardImage("https://storagepoc5.blob.core.windows.net/images/menu-2.jpg") },
                Buttons = new List<CardAction>()
                {
                    new CardAction(){ Title = "Gestionar registro de marca",Value = "Registro de Marca",Type = ActionTypes.ImBack },
                    new CardAction(){ Title = "Ver estado de mi tramite",Value = "Ver estado de mi tramite",Type = ActionTypes.ImBack },
                    new CardAction(){ Title = "Ir a pagina Web de Indecopi",Value = "https://www.gob.pe/indecopi",Type = ActionTypes.OpenUrl }
                }
            };
            var cardContact = new HeroCard
            {
                Title = "Contactenos",
                Subtitle = "Opciones",
                Images = new List<CardImage> { new CardImage("https://storagepoc5.blob.core.windows.net/images/menu-3.jpg") },
                Buttons = new List<CardAction>()
                {
     
                    new CardAction(){ Title = "Datos de contacto",Value = "https://indecopi.gob.pe/contactenos",Type = ActionTypes.OpenUrl },
                    new CardAction(){ Title = "Facebook",Value = "https://facebook.com/IndecopiOficial",Type = ActionTypes.OpenUrl },
                    new CardAction(){ Title = "Twitter",Value = "https://twitter.com/IndecopiOficial",Type = ActionTypes.OpenUrl },
                    new CardAction(){ Title = "Instagram",Value = "https://instagram.com/indecopioficial",Type = ActionTypes.OpenUrl },
                    new CardAction(){ Title = "Youtube",Value = "https://youtube.com/IndecopiOficial",Type = ActionTypes.OpenUrl },
                    new CardAction(){ Title = "Linkedin",Value = "https://www.linkedin.com/company/indecopi",Type = ActionTypes.OpenUrl },
                    new CardAction(){ Title = "Hablar con un interlocutor humano",Value = "Hablar con un interlocutor humano",Type = ActionTypes.ImBack }
                }
            };
            var cardCalification = new HeroCard
            {
                Title = "Calificación",
                Subtitle = "Opciones",
                Images = new List<CardImage> { new CardImage("https://storagepoc5.blob.core.windows.net/images/menu-4.jpg") },
                Buttons = new List<CardAction>()
                {

                    new CardAction(){ Title = "Calificar al asistente virtual",Value = "Calificar Bot",Type = ActionTypes.ImBack }
                }
            };

            var optionAttachments = new List<Attachment>()
            {
                cardFAQs.ToAttachment(),
                cardProcess.ToAttachment(),
                cardContact.ToAttachment(),
                cardCalification.ToAttachment()
            };

            var reply = MessageFactory.Attachment(optionAttachments);
            reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            return reply as Activity;
        }

        private static Activity CreateCarouselContactOptions()
        {
            var cardContact = new HeroCard
            {
                Title = "Contactenos",
                Subtitle = "Opciones",
                Images = new List<CardImage> { new CardImage("https://storagepoc5.blob.core.windows.net/images/menu-3.jpg") },
                Buttons = new List<CardAction>()
                {

                    new CardAction(){ Title = "Datos de contacto",Value = "https://indecopi.gob.pe/contactenos",Type = ActionTypes.OpenUrl },
                    new CardAction(){ Title = "Facebook",Value = "https://facebook.com/IndecopiOficial",Type = ActionTypes.OpenUrl },
                    new CardAction(){ Title = "Twitter",Value = "https://twitter.com/IndecopiOficial",Type = ActionTypes.OpenUrl },
                    new CardAction(){ Title = "Instagram",Value = "https://instagram.com/indecopioficial",Type = ActionTypes.OpenUrl },
                    new CardAction(){ Title = "Youtube",Value = "https://youtube.com/IndecopiOficial",Type = ActionTypes.OpenUrl },
                    new CardAction(){ Title = "Linkedin",Value = "https://www.linkedin.com/company/indecopi",Type = ActionTypes.OpenUrl },
                    new CardAction(){ Title = "Hablar con un interlocutor humano",Value = "Hablar con un interlocutor humano",Type = ActionTypes.ImBack }
                }
            };

            var optionAttachments = new List<Attachment>() { 
                cardContact.ToAttachment()
            };

            var reply = MessageFactory.Attachment(optionAttachments);
            reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            return reply as Activity;
        }
    }
}
