using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using NasdaqBot.Models;

namespace NasdaqBot.Dialogs
{
    public class BuyStockDialog : CancelAndHelpDialog
    {
        private readonly IMarketService _marketService;

        public BuyStockDialog() : base(nameof(BuyStockDialog))
        {
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new NumberPrompt<int>(nameof(NumberPrompt<int>)));
            AddDialog(new NumberPrompt<double>(nameof(NumberPrompt<double>), LimitValidatorAsync));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
            // AddDialog(new DateResolverDialog());
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                StockSelectionStepAsync,
                AmountDefinitionStepAsync,
                LimitDefinitionStepAsync,
                ConfirmStepAsync,
                FinalStepAsync,
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> StockSelectionStepAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            string StockSelectionMsgText = "Which stock do you want to buy?";

            var stockRequest = (BuyStockRequest)stepContext.Options;

            if (stockRequest.StockSymbol == null)
            {
                var promptMessage = MessageFactory.Text(StockSelectionMsgText, StockSelectionMsgText,
                    InputHints.ExpectingInput);
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage },
                    cancellationToken);
            }

            return await stepContext.NextAsync(stockRequest.StockSymbol, cancellationToken);
        }

        private async Task<DialogTurnResult> AmountDefinitionStepAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            var stockRequest = (BuyStockRequest)stepContext.Options;

            stockRequest.StockSymbol = (string)stepContext.Result;
            string StockAmountMsgText = $"How much of {stockRequest.StockSymbol} should I buy?";

            if (stockRequest.Amount == 0)
            {
                var promptMessage =
                    MessageFactory.Text(StockAmountMsgText, StockAmountMsgText, InputHints.ExpectingInput);
                return await stepContext.PromptAsync(nameof(NumberPrompt<int>),
                    new PromptOptions { Prompt = promptMessage }, cancellationToken);
            }

            return await stepContext.NextAsync(stockRequest.Amount, cancellationToken);
        }

        private async Task<DialogTurnResult> LimitDefinitionStepAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            var stockRequest = (BuyStockRequest)stepContext.Options;

            stockRequest.Amount = Convert.ToInt32(stepContext.Result);
            string StockLimitMsgText = $"At what limit should I put the {stockRequest.Amount}x{stockRequest.StockSymbol} order?";

            if (stockRequest.OrderLimit == 0)
            {
                var promptMessage =
                    MessageFactory.Text(StockLimitMsgText, StockLimitMsgText, InputHints.ExpectingInput);
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage },
                    cancellationToken);
            }

            return await stepContext.NextAsync(stockRequest.OrderLimit, cancellationToken);
        }

        private async Task<DialogTurnResult> ConfirmStepAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            try
            {
                var stockRequest = (BuyStockRequest)stepContext.Options;
                stockRequest.OrderLimit = double.Parse(stepContext.Result.ToString());

                var messageText =
                    $"Please confirm, I will buy {stockRequest.Amount} {stockRequest.StockSymbol} at limit of {stockRequest.OrderLimit}. Is this correct?";
                var promptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);

                return await stepContext.PromptAsync(nameof(ConfirmPrompt),
                    new PromptOptions { Prompt = promptMessage }, cancellationToken);
            }
            catch (Exception e)
            {
                throw;
            }
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            if ((bool)stepContext.Result)
            {
                var stockRequest = (BuyStockRequest)stepContext.Options;
                return await stepContext.EndDialogAsync(stockRequest, cancellationToken);
            }

            return await stepContext.EndDialogAsync(null, cancellationToken);
        }

        private Task<bool> LimitValidatorAsync(PromptValidatorContext<double> promptContext,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(promptContext.Recognized.Succeeded && promptContext.Recognized.Value > 0 &&
                                   promptContext.Recognized.Value < 800);
        }
    }
}