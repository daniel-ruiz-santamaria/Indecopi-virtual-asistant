// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio EmptyBot v4.15.0

//using IndecopiVirtualAssitant.Models;
//using IndecopiVirtualAssitant.Models.AzureTable;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace IndecopiVirtualAssitant
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, config) => {
                    var root = config.Build();
                    config.AddAzureKeyVault(
                        $"https://{root["KeyVault:Vault"]}.vault.azure.net/",
                        root["KeyVault:ClientId"],
                        root["KeyVault:ClientSecret"]
                    );
                })
                .UseStartup<Startup>();
    }
}
