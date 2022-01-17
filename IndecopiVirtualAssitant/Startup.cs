// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio EmptyBot v4.15.0

using IndecopiVirtualAssitant.Dialogs;
using IndecopiVirtualAssitant.Infraestructure.Luis;
using IndecopiVirtualAssitant.Infraestructure.QnAMakerAI;
using IndecopiVirtualAssitant.Repositories;
using IndecopiVirtualAssitant.Services;
//using IndecopiVirtualAssitant.Models.AzureTable;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Azure.Blobs;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

namespace IndecopiVirtualAssitant
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
            // Conexion a Blob storage para el state
            var blobStorageState = new BlobsStorage(
                Configuration.GetSection("AzureStorageConnectionString").Value,
                Configuration.GetSection("BlobStorageStateContainer").Value
                );

            var x = Configuration["pepapi"];
            var userState = new UserState(blobStorageState);
            services.AddSingleton(userState);

            var conversationState = new ConversationState(blobStorageState);
            services.AddSingleton(conversationState);

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_3_0);

            // Create the Bot Framework Adapter with error handling enabled.
            services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();

            services.AddSingleton<ILuisService,LuisService>();

            services.AddSingleton<IQnAMakerAIService, QnAMakerAIService>();

            services.AddSingleton<IAzureTableRepository, AzureTableRepository>();

            services.AddSingleton<State>();
            services.AddSingleton<SessionsData>();

            // DB
            //services.AddSingleton<AnswerRepository>();
            //services.AddSingleton<AuditRepository>();

            // Dialog registration
            services.AddSingleton<RootDialog>();

            // Create the bot as a transient. In this case the ASP Controller is expecting an IBot.
            services.AddTransient<IBot, IndecopiVirtualAssitant<RootDialog>>();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }
            app.UseRouting();

            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseWebSockets();
            app.UseEndpoints(endpoints => {
                endpoints.MapControllers();
            });
        }
    }
}
