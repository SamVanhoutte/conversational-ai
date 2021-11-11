using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace NasdaqBot.Languages
{
    public class CognitiveTranslater
    {
        private const string Host = "https://api.cognitive.microsofttranslator.com";
        private const string Path = "/translate?api-version=3.0";
        private const string UriParams = "&to=";
        private const string location = "westeurope";
        private static HttpClient _client = new HttpClient();

        private readonly string _key;
        private bool UseTranslatorService => _key != null;

        public CognitiveTranslater(IConfiguration configuration)
        {
            _key = configuration["TranslatorKey"];
        }

        public async Task<string> TranslateAsync(string text, string targetLocale,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (UseTranslatorService && !string.IsNullOrEmpty(text))
            {
                // From Cognitive Services translation documentation:
                // https://docs.microsoft.com/en-us/azure/cognitive-services/translator/quickstart-csharp-translate
                var body = new object[] { new { Text = text } };
                var requestBody = JsonConvert.SerializeObject(body);

                using (var request = new HttpRequestMessage())
                {
                    var uri = Host + Path + UriParams + targetLocale;
                    request.Method = HttpMethod.Post;
                    request.RequestUri = new Uri(uri);
                    request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                    request.Headers.Add("Ocp-Apim-Subscription-Key", _key);
                    request.Headers.Add("Ocp-Apim-Subscription-Region", location);
                    
                    var response = await _client.SendAsync(request, cancellationToken);

                    if (!response.IsSuccessStatusCode)
                    {
                        throw new Exception(
                            $"The call to the translation service returned HTTP status code {response.StatusCode}.");
                    }

                    var responseBody = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<TranslatorResponse[]>(responseBody);

                    return result?.FirstOrDefault()?.Translations?.FirstOrDefault()?.Text;
                }
            }

            return text;
        }
    }
}