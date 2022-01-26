using IndecopiVirtualAssitant.Models;
using IndecopiVirtualAssitant.Repositories;
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
        private readonly IAzureTableRepository _tableRepository;
        private const string ASSISTANT_DATA_TABLE = "asistantData";

        public MainOptionsCard(IAzureTableRepository tableRepository)
        {
            _tableRepository = tableRepository;
        }

        public static async Task ToShow(IAzureTableRepository tableRepository, DialogContext stepContext, CancellationToken cancellationToken) {
            await stepContext.Context.SendActivityAsync(activity: await CreateCarousel(tableRepository), cancellationToken);
        }

        public static async Task ToShowContact(IAzureTableRepository tableRepository, DialogContext stepContext, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync(activity: await CreateCarouselContactOptions(tableRepository), cancellationToken);
        }

        public static async Task ToShowSocialNetworks(IAzureTableRepository tableRepository, DialogContext stepContext, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync(activity: await CreateCarouselSocialNetworksOptions(tableRepository), cancellationToken);
        }

        public static async Task ToShowVirtualChannels(IAzureTableRepository tableRepository, DialogContext stepContext, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync(activity: await CreateCarouselVirtualChanelsOptions(tableRepository), cancellationToken);
        }

        public static async Task ToShowPhones(IAzureTableRepository tableRepository, DialogContext stepContext, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync(activity: await CreateCarouselTelephonesOptions(tableRepository), cancellationToken);
        }

        public static async Task ToShowMails(IAzureTableRepository tableRepository, DialogContext stepContext, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync(activity: await CreateCarouselMailOptions(tableRepository), cancellationToken);
        }

        public static async Task ToShowAddresses(IAzureTableRepository tableRepository, DialogContext stepContext, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync(activity: await CreateCarouselAddressOptions(tableRepository), cancellationToken);
        }

        public static async Task ToShowQnAResponse(DialogContext stepContext, CancellationToken cancellationToken, QueryResult queryResult)
        {
            List<CardAction> cardActions = new List<CardAction>();
            if (queryResult.Context.Prompts.Length > 0)
            {
                foreach (var p in queryResult.Context.Prompts)
                {
                    if (p.DisplayText.StartsWith("..."))
                    {
                        cardActions.Add(new CardAction() { Title = p.DisplayText, Value = p.DisplayText, Type = ActionTypes.ImBack });
                    }
                    else
                    {
                        cardActions.Add(new CardAction() { Title = p.DisplayText, Value = p.DisplayText, Type = ActionTypes.ImBack });
                    }
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

        private async static Task<Activity> CreateCarousel(IAzureTableRepository tableRepository) 
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

            List<CardAction> bottons = new List<CardAction>();
            bottons.Add(new CardAction() { Title = "Ver datos de contacto", Value = "ver datos contacto", Type = ActionTypes.ImBack });
            bottons.Add(new CardAction() { Title = "Página de contacto", Value = "https://indecopi.gob.pe/contactenos", Type = ActionTypes.OpenUrl });
            bottons.Add(new CardAction() { Title = "Ver redes sociales", Value = "redes sociales", Type = ActionTypes.ImBack });
            bottons.Add(new CardAction() { Title = "Ver canales virtuales", Value = "canales virtuales", Type = ActionTypes.ImBack });
            bottons.Add(new CardAction() { Title = "Contactar con un interlocutor humano", Value = "Hablar con un interlocutor humano", Type = ActionTypes.ImBack });

            var cardContact = new HeroCard
            {
                Title = "Contactenos",
                Subtitle = "Opciones",
                Images = new List<CardImage> { new CardImage("https://storagepoc5.blob.core.windows.net/images/menu-3.jpg") },
                Buttons = bottons
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

        private async static Task<Activity> CreateCarouselContactOptions(IAzureTableRepository tableRepository)
        {

            List<CardAction> bottons = new List<CardAction>();
            bottons.Add(new CardAction() { Title = "Ver datos de contacto", Value = "ver datos contacto", Type = ActionTypes.ImBack });
            bottons.Add(new CardAction() { Title = "Página de contacto", Value = "https://indecopi.gob.pe/contactenos", Type = ActionTypes.OpenUrl });
            bottons.Add(new CardAction() { Title = "Ver redes sociales", Value = "redes sociales", Type = ActionTypes.ImBack });
            bottons.Add(new CardAction() { Title = "Ver canales virtuales", Value = "canales virtuales", Type = ActionTypes.ImBack });
            bottons.Add(new CardAction() { Title = "Contactar con un interlocutor humano", Value = "Hablar con un interlocutor humano", Type = ActionTypes.ImBack });

            var cardContact = new HeroCard
            {
                Title = "Contactenos",
                Subtitle = "Opciones",
                Images = new List<CardImage> { new CardImage("https://storagepoc5.blob.core.windows.net/images/menu-3.jpg") },
                Buttons = bottons
            };

            var optionAttachments = new List<Attachment>() { 
                cardContact.ToAttachment()
            };

            var reply = MessageFactory.Attachment(optionAttachments);
            reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            return reply as Activity;
        }

        private static async Task<Activity> CreateCarouselSocialNetworksOptions(IAzureTableRepository tableRepository1)
        {

            List<CardAction> bottons = new List<CardAction>();
            var results = await tableRepository1.getAssistantData(ASSISTANT_DATA_TABLE, "socialNetwork");

            foreach (AssistantData ad in results) {
                bottons.Add(new CardAction() { 
                    Title = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(ad.key.ToLower()), 
                    Value = ad.link, 
                    Type = ActionTypes.OpenUrl });
            }

            var cardContact = new HeroCard
            {
                Title = "Redes sociales",
                Images = new List<CardImage> { new CardImage("https://storagepoc5.blob.core.windows.net/images/social-networks-2.jpg") },
                Buttons = bottons
            };

            var optionAttachments = new List<Attachment>() {
                cardContact.ToAttachment()
            };

            var reply = MessageFactory.Attachment(optionAttachments);
            reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            return reply as Activity;
        }

        private static async Task<Activity> CreateCarouselVirtualChanelsOptions(IAzureTableRepository tableRepository1)
        {

            List<CardAction> buttons = new List<CardAction>();
            var results = await tableRepository1.getAssistantData(ASSISTANT_DATA_TABLE, "contact", "virtualChannel");

            foreach (AssistantData ad in results)
            {
                buttons.Add(new CardAction()
                {
                    Title = ad.value,
                    Value = ad.link,
                    Type = ActionTypes.OpenUrl
                });
            }

            var cardContact = new HeroCard
            {
                Title = "Canales virtuales",
                Images = new List<CardImage> { new CardImage("https://storagepoc5.blob.core.windows.net/images/virtualChanel.jpg") },
                Buttons = buttons
            };

            var optionAttachments = new List<Attachment>() {
                cardContact.ToAttachment()
            };

            var reply = MessageFactory.Attachment(optionAttachments);
            reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            return reply as Activity;
        }

        private static async Task<Activity> CreateCarouselTelephonesOptions(IAzureTableRepository tableRepository1)
        {

            List<CardAction> buttons = new List<CardAction>();
            var results = await tableRepository1.getAssistantData(ASSISTANT_DATA_TABLE, "contact", "telephone");

            foreach (AssistantData ad in results)
            {
                buttons.Add(new CardAction()
                {
                    Title = ad.value,
                    Value = ad.link,
                    Type = ActionTypes.OpenUrl
                });
            }

            var cardContact = new HeroCard
            {
                Title = "Nuestros teléfonos",
                Images = new List<CardImage> { new CardImage("https://storagepoc5.blob.core.windows.net/images/telephone.jpg") },
                Buttons = buttons
            };

            var optionAttachments = new List<Attachment>() {
                cardContact.ToAttachment()
            };

            var reply = MessageFactory.Attachment(optionAttachments);
            reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            return reply as Activity;
        }

        private static async Task<Activity> CreateCarouselMailOptions(IAzureTableRepository tableRepository1)
        {

            List<CardAction> buttons = new List<CardAction>();
            var results = await tableRepository1.getAssistantData(ASSISTANT_DATA_TABLE, "contact", "mail");

            foreach (AssistantData ad in results)
            {
                buttons.Add(new CardAction()
                {
                    Title = ad.value,
                    Value = ad.link,
                    Type = ActionTypes.OpenUrl
                });
            }

            var cardContact = new HeroCard
            {
                Title = "Nuestros mails",
                Images = new List<CardImage> { new CardImage("https://storagepoc5.blob.core.windows.net/images/mail.png") },
                Buttons = buttons
            };

            var optionAttachments = new List<Attachment>() {
                cardContact.ToAttachment()
            };

            var reply = MessageFactory.Attachment(optionAttachments);
            reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            return reply as Activity;
        }

        private static async Task<Activity> CreateCarouselAddressOptions(IAzureTableRepository tableRepository1)
        {

            List<CardAction> buttons = new List<CardAction>();
            var results = await tableRepository1.getAssistantData(ASSISTANT_DATA_TABLE, "contact", "address");

            foreach (AssistantData ad in results)
            {
                buttons.Add(new CardAction()
                {
                    Title = ad.value,
                    Value = ad.link,
                    Type = ActionTypes.OpenUrl
                });
            }

            var cardContact = new HeroCard
            {
                Title = "Nuestras direcciones:",
                Images = new List<CardImage> { new CardImage("https://storagepoc5.blob.core.windows.net/images/address.jpg") },
                Buttons = buttons
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
