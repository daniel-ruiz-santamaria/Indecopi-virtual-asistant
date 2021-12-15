using Microsoft.Azure;
//using Microsoft.WindowsAzure.Storage;
//using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

/*
namespace IndecopiVirtualAssitant.Models.AzureTable
{
    public class AuditRepository
    {
        // Declarar objeto Storage
        CloudStorageAccount storageAccount;
        private const String tableName = "answer";
        private static int id = 0;

        public AuditRepository()
        {
            var x = CloudConfigurationManager.GetSetting("AzureStorageConnectionString");
            storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("AzureStorageConnectionString"));
        }

        // Insertar un nuevo audit
        public void SaveAudit(String idSession, String intent, string query, string answare, decimal score, DateTime date) { 
            
        }

        public async Task<CloudTable> CreateTableAzureStorage()
        {
            CloudTableClient tableAnsware = storageAccount.CreateCloudTableClient();
            CloudTable table = tableAnsware.GetTableReference(tableName);
            try
            {
                if (await table.CreateIfNotExistsAsync())
                {
                    Console.WriteLine("Created Table named: {0}", tableName);
                }
                else
                {
                    Console.WriteLine("Created Table named: {0}", tableName);
                }
            }
            catch (StorageException) 
            {
                Console.WriteLine("If you are running with the default configuration please make sure you have started the storage emulator. Press the Windows key and type Azure Storage to select and run it from the list of applications - then restart the sample.");
                Console.ReadLine();
                throw;
            }
            Console.WriteLine();
            return table;
        }

        // Metodo para recuperar Respuestas por partition key
        public async Task Save(String sessionId, String channel, String intent, decimal score, String query, String answer, DateTime date) {
            CloudTable table = await this.CreateTableAzureStorage();
            Audit audit = new Audit();
            audit.channel = channel;
            audit.intent = intent;
            audit.score = score;
            audit.answer = answer;
            audit.query = query;
            audit.date = date;
            audit.IdSession = sessionId;
            TableOperation insertOperation = TableOperation.Insert(audit);
            await table.ExecuteAsync(insertOperation);
        }
    }
}
*/
