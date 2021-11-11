using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using NasdaqBot.Languages;

namespace NasdaqBot.Middleware
{
    public class MultiLingualMiddleware : IMiddleware
    {
        private readonly CognitiveTranslater _translator;
        private readonly IStatePropertyAccessor<string> _languageStateProperty;

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiLingualMiddleware"/> class.
        /// </summary>
        /// <param name="translator">Translator implementation to be used for text translation.</param>
        /// <param name="languageStateProperty">State property for current language.</param>
        public MultiLingualMiddleware(CognitiveTranslater translator, UserState userState)
        {
            _translator = translator ?? throw new ArgumentNullException(nameof(translator));
            if (userState == null)
            {
                throw new ArgumentNullException(nameof(userState));
            }

            _languageStateProperty = userState.CreateProperty<string>("LanguagePreference");
        }

        public async Task OnTurnAsync(ITurnContext turnContext, NextDelegate next,
            CancellationToken cancellationToken = new CancellationToken())
        {
            if (turnContext == null)
            {
                throw new ArgumentNullException(nameof(turnContext));
            }

            var translate = await ShouldTranslateAsync(turnContext, cancellationToken);

            if (translate.Item1)
            {
                if (turnContext.Activity.Type == ActivityTypes.Message)
                {
                        turnContext.Activity.Text = await _translator.TranslateAsync(turnContext.Activity.Text,
                        TranslationSettings.DefaultLanguage, cancellationToken);
                }
            }

            turnContext.OnSendActivities(async (newContext, activities, nextSend) =>
            {
                // string userLanguage =
                //     await _languageStateProperty.GetAsync(turnContext, () => TranslationSettings.DefaultLanguage) ??
                //     TranslationSettings.DefaultLanguage;
                // bool shouldTranslate = userLanguage != TranslationSettings.DefaultLanguage;

                // Translate messages sent to the user to user language
                if (translate.Item1)
                {
                    List<Task> tasks = new List<Task>();
                    foreach (Activity currentActivity in activities.Where(a => a.Type == ActivityTypes.Message))
                    {
                        tasks.Add(TranslateMessageActivityAsync(currentActivity.AsMessageActivity(), translate.Item2));
                    }

                    if (tasks.Any())
                    {
                        await Task.WhenAll(tasks).ConfigureAwait(false);
                    }
                }

                return await nextSend();
            });

            turnContext.OnUpdateActivity(async (newContext, activity, nextUpdate) =>
            {
                // string userLanguage =
                //     await _languageStateProperty.GetAsync(turnContext, () => TranslationSettings.DefaultLanguage) ??
                //     TranslationSettings.DefaultLanguage;
                // bool shouldTranslate = userLanguage != TranslationSettings.DefaultLanguage;

                // Translate messages sent to the user to user language
                if (activity.Type == ActivityTypes.Message)
                {
                    if (translate.Item1)
                    {
                        await TranslateMessageActivityAsync(activity.AsMessageActivity(), translate.Item2);
                    }
                }

                return await nextUpdate();
            });

            await next(cancellationToken).ConfigureAwait(false);
        }

        private async Task TranslateMessageActivityAsync(IMessageActivity activity, string targetLocale,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (activity.Type == ActivityTypes.Message)
            {
                activity.Text = await _translator.TranslateAsync(activity.Text, targetLocale);
            }
        }

        private async Task<Tuple<bool, string>> ShouldTranslateAsync(ITurnContext turnContext,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            string userLanguage =
                await _languageStateProperty.GetAsync(turnContext, () => null,
                    cancellationToken);

            if (userLanguage == null)
            {
                userLanguage = "nl"; // Detect language here
            }

            return new Tuple<bool, string>(userLanguage != TranslationSettings.DefaultLanguage, userLanguage);
        }
    }
}