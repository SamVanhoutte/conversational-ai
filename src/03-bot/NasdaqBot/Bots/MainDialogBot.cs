using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using NasdaqBot.Dialogs;
using Newtonsoft.Json;

namespace NasdaqBot.Bots
{
    public class MainDialogBot<T> : DialogBot<T>
        where T : Dialog
    {


        public MainDialogBot(ConversationState conversationState, UserState userState, T dialog, ILogger<DialogBot<T>> logger) 
            : base(conversationState, userState, dialog, logger)
        {
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    // Pick random welcome message
                    await turnContext.SendActivityAsync(MessageFactory.Text(MainDialog.GetWelcomeMessage(turnContext.Activity.From.Name)), cancellationToken);
                }
            }
        }

    }
}