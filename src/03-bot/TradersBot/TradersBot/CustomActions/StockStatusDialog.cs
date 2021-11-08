using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder.Dialogs;
using Newtonsoft.Json;

namespace TradersBot.CustomActions
{
    public class StockStatusDialog : Dialog
    {
        [JsonConstructor]
        public StockStatusDialog([CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
            : base()
        {
            // enable instances of this command as debug break point
            RegisterSourceLocation(sourceFilePath, sourceLineNumber);
        }
        [JsonProperty("$kind")]
        public const string Kind = "StockStatusDialog";

        [JsonProperty("stocksymbol")]
        public StringExpression StockSymbol { get; set; }

        [JsonProperty("resultProperty")]
        public StringExpression ResultProperty { get; set; }
        
        public override Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            var symbol = StockSymbol.GetValue(dc.State);

            var result = $"{symbol} is doing terrible today";
            if (this.ResultProperty != null)
            {
                dc.State.SetValue(this.ResultProperty.GetValue(dc.State), result);
            }

            return dc.EndDialogAsync(result: result, cancellationToken: cancellationToken);
        }
    }
}