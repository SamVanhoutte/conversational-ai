using System;
using System.Threading.Tasks;
using Azure;
using Azure.AI.TextAnalytics;
using Microsoft.Extensions.Configuration;

namespace NasdaqBot.Languages
{
    public class CognitiveLanguageDetector
    {
        private TextAnalyticsClient _languageDetectionClient;

        public CognitiveLanguageDetector(IConfiguration configuration)
        {
            string key = configuration["LanguageDetectionKey"];
            string endpoint = configuration["LanguageDetectionEdpoint"];
            if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(endpoint))
            {
                _languageDetectionClient = new TextAnalyticsClient(new Uri(endpoint), new AzureKeyCredential(key));
            }
        }

        public bool IsConfigured => _languageDetectionClient != null;

        // Example method for detecting the language of text
        public async Task<string> DetectLanguageAsync(string text)
        {
            if (!IsConfigured) return null;
            var detectedLanguage = await _languageDetectionClient.DetectLanguageAsync(text);
            return detectedLanguage.Value.Iso6391Name;
        }
    }
}