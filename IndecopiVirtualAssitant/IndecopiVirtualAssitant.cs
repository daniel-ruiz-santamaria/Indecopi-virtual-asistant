// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

// using IndecopiVirtualAssitant.Models.AzureTable;
using Azure.Data.Tables;
using IndecopiVirtualAssitant.Repositories;
using IndecopiVirtualAssitant.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace IndecopiVirtualAssitant
{
    public class IndecopiVirtualAssitant<T> : ActivityHandler where T: Dialog
    {
        private readonly BotState _userState;
        private readonly BotState _conversationState;
        private readonly Dialog _dialog;
        private readonly State _state;
        private readonly IAzureTableRepository _tableRepository;
        // private readonly AuditRepository _auditRepository;

        public IndecopiVirtualAssitant(UserState userState, ConversationState conversationState, T dialog, State state, IAzureTableRepository tableRepository)
        {
            _userState = userState;
            _conversationState = conversationState;
            _dialog = dialog;
            _state = state;
            _tableRepository = tableRepository;
            // _auditRepository = auditRepository;
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    var answer = await _tableRepository.getAnswer("answers", "InitialGreeting", "Hola, soy un asistente virtual, estoy desando ayudarte ¿Cómo puedo ayudar?");
                    await turnContext.SendActivityAsync(MessageFactory.Text(answer), cancellationToken);
                }
            }
        }

        // Captura las actividades del usuario o del bot
        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
        {
            await _userState.SaveChangesAsync(turnContext, false, cancellationToken);
            await _conversationState.SaveChangesAsync(turnContext, false, cancellationToken);
        }

        // Captura todas las mensajes del usuario
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            // this._state.MessageActivity = turnContext.Activity;
            this._state.AddActivity(turnContext);
            await _dialog.RunAsync(
                turnContext,
                _conversationState.CreateProperty<DialogState>(nameof(DialogState)),
                cancellationToken
            );
        }
    }
}
