// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with EmptyBot .NET Template version v4.14.1.2

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NasdaqBot.Bots;
using NasdaqBot.Dialogs;
using NasdaqBot.Languages;
using NasdaqBot.Middleware;
using NasdaqBot.Recognizers;
using NasdaqBot.Services;

namespace NasdaqBot
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpClient().AddControllers().AddNewtonsoftJson();

            // Create the User state. (Used in this bot's Dialog implementation.)
            services.AddSingleton<UserState>();
            // Create the Microsoft Translator responsible for making calls to the Cognitive Services translation service
            services.AddSingleton<CognitiveTranslater>();
            services.AddSingleton<CognitiveLanguageDetector>();

            // Create the Translation Middleware that will be added to the middleware pipeline in the AdapterWithErrorHandler
            services.AddSingleton<MultiLingualMiddleware>();
            
            // Create the storage we'll be using for User and Conversation state. (Memory is great for testing purposes.)
            services.AddSingleton<IStorage, MemoryStorage>();
            
            // Create the Conversation state. (Used by the Dialog system itself.)
            services.AddSingleton<ConversationState>();
            
            // Create the Bot Framework Adapter with error handling enabled.
            services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();

            // Create the bot as a transient. In this case the ASP Controller is expecting an IBot.
            // services.AddTransient<IBot, NasdaqBot.Bots.EmptyBot>();

            services.AddSingleton<StockStatusRecognizer>();
            
            // Register the BuyStockDialog.
            services.AddSingleton<BuyStockDialog>();

            // The MainDialog that will be run by the bot.
            services.AddSingleton<MainDialog>();
            services.AddTransient<IMarketService, StubMarketService>();
            
            // Create the bot as a transient. In this case the ASP Controller is expecting an IBot.
            services.AddTransient<IBot, MainDialogBot<MainDialog>>();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseDefaultFiles()
                .UseStaticFiles()
                .UseWebSockets()
                .UseRouting()
                .UseAuthorization()
                .UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                });

            // app.UseHttpsRedirection();
        }
    }
}
