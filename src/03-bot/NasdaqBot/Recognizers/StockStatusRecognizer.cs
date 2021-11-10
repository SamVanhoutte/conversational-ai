using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Extensions.Configuration;

namespace NasdaqBot.Recognizers
{
    public class StockStatusRecognizer : IRecognizer
    {
        private readonly LuisRecognizer _recognizer;

        public StockStatusRecognizer(IConfiguration configuration)
        {
            var luisEnabled = !string.IsNullOrEmpty(configuration["LuisAppId"]) && !string.IsNullOrEmpty(configuration["LuisAPIKey"]) && !string.IsNullOrEmpty(configuration["LuisAPIHostName"]);
            if (luisEnabled)
            {
                var luisApplication = new LuisApplication(configuration["LuisAppId"], configuration["LuisAPIKey"],
                    $"https://{configuration["LuisAPIHostName"]}");
                _recognizer = new LuisRecognizer(new LuisRecognizerOptionsV3(luisApplication));
            }
        }

        public virtual bool IsConfigured => _recognizer != null;

        public virtual async Task<RecognizerResult> RecognizeAsync(ITurnContext turnContext,
            CancellationToken cancellationToken)
        {
            try
            {
                return await _recognizer.RecognizeAsync(turnContext, cancellationToken);

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            
        }


        public virtual async Task<T> RecognizeAsync<T>(ITurnContext turnContext, CancellationToken cancellationToken)
            where T : IRecognizerConvert, new()
        {
            try
            {
                return await _recognizer.RecognizeAsync<T>(turnContext, cancellationToken);

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            
        }
    }
}