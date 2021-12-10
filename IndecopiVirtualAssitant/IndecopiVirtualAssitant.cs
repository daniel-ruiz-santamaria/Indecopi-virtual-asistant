// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace IndecopiVirtualAssitant
{
    public class IndecopiVirtualAssitant : ActivityHandler
    {
        private readonly BotState _userState;
        private readonly BotState _conversationState;

        public IndecopiVirtualAssitant(UserState userState, ConversationState conversationState)
        {
            _userState = userState;
            _conversationState = conversationState;
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text($"Hello world!"), cancellationToken);
                }
            }
        }

        // Captura las actividades del usuario o del bot
        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
        {
            await base.OnTurnAsync(turnContext, cancellationToken);
            await _userState.SaveChangesAsync(turnContext, false, cancellationToken);
            await _conversationState.SaveChangesAsync(turnContext, false, cancellationToken);
        }

        // Captura todas las mensajes del usuario
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            // Lo que el usuario ha escrito
            var userMessage = turnContext.Activity.Text;
            await turnContext.SendActivityAsync($"User: {userMessage}", cancellationToken: cancellationToken);
        }
    }
}
