// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio EmptyBot v4.15.0

//using IndecopiVirtualAssitant.Models;
//using IndecopiVirtualAssitant.Models.AzureTable;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
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
            // MainAsync().GetAwaiter().GetResult();
        }

        private static async Task MainAsync()
        {
            /*
            AnswerRepository ams = new AnswerRepository();
            await ams.GetRandomAnswersByPartitionKey("PartitionKey");
            return;
            */
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                /*
                WebHost.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, config) =>
                {
                    //if (context.HostingEnvironment.IsProduction())
                    //{
                    var builtConfig = config.Build();

                    using (var store = new X509Store(StoreName.My,
                           StoreLocation.CurrentUser))
                    {
                        //Obtenemos el Certificado
                        store.Open(OpenFlags.ReadOnly);
                        var certs = store.Certificates
                            .Find(X509FindType.FindByThumbprint, builtConfig["AzureADCertThumbprint"], false);

                        try
                        {
                            //Nos conectamos a Azure Key Vault API
                            var certificados = certs.OfType<X509Certificate2>();
                            config.AddAzureKeyVault($"https://{builtConfig["KeyVaultName"]}.vault.azure.net/",
                                                    builtConfig["AzureADApplicationId"], certificados.First());
                        }
                        catch (Exception e)
                        {
                            throw e;
                        }

                        store.Close();
                    }
                    //}
                })
                */
                .UseStartup<Startup>();
    }
}
