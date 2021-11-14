namespace NasdaqBot.Middleware
{
    public class TranslationInstruction
    {
        public TranslationInstruction(bool shouldTranslate, string userLanguage)
        {
            ShouldTranslate = shouldTranslate;
            UserLanguage = userLanguage;
        }
        public bool ShouldTranslate { get; set; }
        public string UserLanguage { get; set; }
    }
}