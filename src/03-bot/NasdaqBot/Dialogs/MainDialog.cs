using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using NasdaqBot.Models;
using NasdaqBot.Recognizers;
using Newtonsoft.Json.Linq;
using NasdaqBot.Extensions;
using Newtonsoft.Json;

namespace NasdaqBot.Dialogs
{
    public class MainDialog : ComponentDialog
    {
        private readonly StockStatusRecognizer _luisRecognizer;
        protected readonly ILogger<MainDialog> Logger;
        private readonly IMarketService _marketService;

        public static List<string> WelcomeMessages => new List<string>
        {
            "Hi, %User.Welcome to your Trading assistant",
            "Hey %User, what's up?  How can I help you doing trades today?",
            "Hello %User. An example: Buy 100 MSFT at 345.3",
            "%User, this is an example you can ask: How is Facebook doing today?",
            "Welcome, %User. How can I help getting you rich, today. #HODL",
            "How can I help you get rich today?\nSay something like \"Buy 100 MSFT at 300\""
        };

        public static string GetWelcomeMessage(string user="")
        {
            var rand = new Random();
            return WelcomeMessages[rand.Next(WelcomeMessages.Count)].Replace("%User", user);
        }
        
        public MainDialog(StockStatusRecognizer luisRecognizer, BuyStockDialog buyStockDialog,
            IMarketService marketService, ILogger<MainDialog> logger) : base(nameof(MainDialog))
        {
            _marketService = marketService;
            _luisRecognizer = luisRecognizer;
            Logger = logger;

            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(buyStockDialog);
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                WelcomeStepAsync,
                SpecifyRequestStepAsync,
                FinalStepAsync,
            }));


            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> WelcomeStepAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            if (!_luisRecognizer.IsConfigured)
            {
                await stepContext.Context.SendActivityAsync(
                    MessageFactory.Text(
                        "NOTE: LUIS is not configured. To enable all capabilities, add 'LuisAppId', 'LuisAPIKey' and 'LuisAPIHostName' to the appsettings.json file.",
                        inputHint: InputHints.IgnoringInput), cancellationToken);

                return await stepContext.NextAsync(null, cancellationToken);
            }

            if (stepContext.Options != null)
            {
                // If Options is not null, it's a replay of the dialog and we show the startup question
                // Use the text provided in FinalStepAsync or the default if it is the first time.
                
                var messageText = stepContext.Options?.ToString() ??
                                  GetWelcomeMessage(stepContext.Context.Activity.From.Name);
                var promptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage },
                    cancellationToken);
            }

            return await stepContext.NextAsync(null, cancellationToken);
        }

        private async Task<DialogTurnResult> SpecifyRequestStepAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            if (!_luisRecognizer.IsConfigured)
            {
                // LUIS is not configured, we just run the BuyStockDialog path with an empty BookingDetailsInstance.
                return await stepContext.BeginDialogAsync(nameof(BuyStockDialog), new BuyStockRequest(),
                    cancellationToken);
            }

            // Call LUIS and gather any potential booking details. (Note the TurnContext has the response to the prompt.)

            // var luisResult = await _luisRecognizer.RecognizeAsync<FlightBooking>(stepContext.Context, cancellationToken);
            var luisResult = await _luisRecognizer.RecognizeAsync(stepContext.Context, cancellationToken);
            var topIntent = luisResult.Intents
                .Where(i => i.Value.Score > 0.05)
                .OrderByDescending(i => i.Value.Score).FirstOrDefault().Key;

            switch (topIntent?.ToLower())
            {
                case "stock_buy":
                    try
                    {
                        var buyStockRequest = new BuyStockRequest();
                        buyStockRequest.StockSymbol = luisResult.ReadEntity<string>("StockSymbol", true);
                        buyStockRequest.Amount = luisResult.ReadEntity<int>("Amount");
                        buyStockRequest.OrderLimit = luisResult.ReadEntity<decimal>("Limit");
                        // Run the BuyStockDialog giving it whatever details we have from the LUIS call, it will fill out the remainder.
                        return await stepContext.BeginDialogAsync(nameof(BuyStockDialog), buyStockRequest,
                            cancellationToken);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        throw;
                    }

                    break;
                case "stock_query":
                    try
                    {
                        var stockSymbol = luisResult.ReadEntity<string>("StockSymbol", true);
                        var result = await _marketService.GetStockResultAsync(stockSymbol);
                        var message = MessageFactory.Attachment(CreateStockStatusMessage(result));
                        await stepContext.Context.SendActivityAsync(message, cancellationToken);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        throw;
                    }

                    break;
                default:
                    // Catch all for unhandled intents
                    var didntUnderstandMessageText =
                        $"Sorry, I didn't get that. Please try asking in a different way (intent was {topIntent})";
                    var didntUnderstandMessage = MessageFactory.Text(didntUnderstandMessageText,
                        didntUnderstandMessageText, InputHints.IgnoringInput);
                    await stepContext.Context.SendActivityAsync(didntUnderstandMessage, cancellationToken);
                    break;
            }

            return await stepContext.NextAsync(null, cancellationToken);
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            // If the child dialog ("BuyStockDialog") was cancelled, the user failed to confirm or if the intent wasn't BuyStock
            // the Result here will be null.
            if (stepContext.Result is BuyStockRequest stockRequest)
            {
                // Now we have all the booking details call the booking service.
                var orderResult = await _marketService.PlaceOrderAsync(stockRequest);

                // If the call to the booking service was successful tell the user.

                var messageText =
                    $"Order {orderResult.OrderNumber} has been created for {stockRequest.Amount} {stockRequest.StockSymbol} at {stockRequest.OrderLimit} with the following costs {orderResult.Costs:0.00}$";
                // var message = MessageFactory.Text(messageText, messageText, InputHints.IgnoringInput);
                var message = MessageFactory.Attachment(CreateOrderConfirmation(orderResult));
                await stepContext.Context.SendActivityAsync(message, cancellationToken);
            }

            // Restart the main dialog with a different message the second time around
            var promptMessage = $"What else can I do for you?";
            stepContext.ActiveDialog.State.Add("Restart", true);
            return await stepContext.ReplaceDialogAsync(InitialDialogId, promptMessage, cancellationToken);
        }

        public static Attachment CreateOrderConfirmation(StockOrder order)
        {
            try
            {
                // combine path for cross platform support
                var paths = new[] { ".", "Cards", "orderresult.json" };
                var adaptiveCardJson = File.ReadAllText(Path.Combine(paths));
                adaptiveCardJson = adaptiveCardJson.Replace("#StockSymbol", order.StockSymbol);
                adaptiveCardJson = adaptiveCardJson.Replace("#OrderId", order.OrderNumber);
                adaptiveCardJson = adaptiveCardJson.Replace("#Amount", order.Amount.ToString());
                adaptiveCardJson = adaptiveCardJson.Replace("#Limit", $"{order.Limit:#,###.00} $");
                adaptiveCardJson = adaptiveCardJson.Replace("#Costs", $"{order.Limit:#,###.00} $");
                var adaptiveCardAttachment = new Attachment()
                {
                    ContentType = "application/vnd.microsoft.card.adaptive",
                    Content = JsonConvert.DeserializeObject(adaptiveCardJson),
                };

                return adaptiveCardAttachment;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public static Attachment CreateStockStatusMessage(StockResult result)
        {
            try
            {
                // combine path for cross platform support
                var paths = new[] { ".", "Cards", "stockresult.json" };
                var adaptiveCardJson = File.ReadAllText(Path.Combine(paths));
                adaptiveCardJson = adaptiveCardJson.Replace("#StockSymbol", result.StockSymbol);
                adaptiveCardJson = adaptiveCardJson.Replace("#Result", result.Result.ToString("0.00") + " %");
                adaptiveCardJson = adaptiveCardJson.Replace("#Message",
                    $"{result.StockSymbol} is {(result.Result > 0 ? "up" : "down")} with {Math.Abs(result.Result):0.00}%");
                var adaptiveCardAttachment = new Attachment()
                {
                    ContentType = "application/vnd.microsoft.card.adaptive",
                    Content = JsonConvert.DeserializeObject(adaptiveCardJson),
                };

                return adaptiveCardAttachment;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}