using Newtonsoft.Json;

namespace NasdaqBot.Languages
{
    /// <summary>
    /// Translation result from Translator API v3.
    /// </summary>
    internal class TranslatorResult
    {
        [JsonProperty("text")] public string Text { get; set; }

        [JsonProperty("to")] public string To { get; set; }
    }
}