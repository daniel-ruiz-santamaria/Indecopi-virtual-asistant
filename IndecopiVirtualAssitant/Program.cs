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
            /*
            Console.WriteLine("Table storage sample!");

            var storageConnectionString = "DefaultEndpointsProtocol=https;AccountName=storagepoc5;AccountKey=5byHFKBRaZPw4H3MFa5UgbNyo2UDjZWxkGD14422PmfdOj7j+hlXUMeDJbc5VBkVBLCXJe/PX63XKOxXaBlxPw==;EndpointSuffix=core.windows.net";
            var tableName = "demo4";

            CloudStorageAccount storageAccount;

            storageAccount = CloudStorageAccount.Parse(storageConnectionString);

            Console.WriteLine("Hasta aqui OK!");

            CloudTableClient tableClient = storageAccount.CreateCloudTableClient(new TableClientConfiguration());
            CloudTable table = tableClient.GetTableReference(tableName);
            table.CreateIfNotExists();

            CustomerEntity customer = new CustomerEntity("Harp", "Walter")
            {
                Email = "daniel.ruiz.eng@gmail.com",
                PhoneNumber = "660546628"
            };

            MergeUser(table, customer).Wait();
            */
            CreateWebHostBuilder(args).Build().Run();
            // MainAsync().GetAwaiter().GetResult();
        }

        public static async Task MergeUser(CloudTable table, CustomerEntity customer) {
            TableOperation insertOrMergeOperation = TableOperation.InsertOrMerge(customer);

            // Execute
            TableResult result = await table.ExecuteAsync(insertOrMergeOperation);
            CustomerEntity insertedCustomer = result.Result as CustomerEntity;
            Console.WriteLine("Added User.");
        }

        public class CustomerEntity : TableEntity
        {
            public CustomerEntity()
            {

            }

            public CustomerEntity(string lastName, string firstName)
            {
                PartitionKey = lastName;
                RowKey = firstName;
            }

            public string Email { get; set; }
            public string PhoneNumber { get; set; }
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
