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
    public class AnswerRepository
    {
        // Declarar objeto Storage
        CloudStorageAccount storageAccount;
        private const String tableName = "answer";
        private static int id = 0;

        public AnswerRepository()
        {
            storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("AzureStorageConnectionString"));
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
        public async Task<Answer> GetRandomAnswersByPartitionKey(String partitionKey) {
            List<Answer> answers = new List<Answer>();
            CloudTable cloudTable = await this.CreateTableAzureStorage();
            string filter = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey);
            TableContinuationToken continuationToken = null;
            do
            {
                var result = await cloudTable.ExecuteQuerySegmentedAsync(new TableQuery<Answer>().Where(filter), continuationToken);
                continuationToken = result.ContinuationToken;
                int index = 0;
                if (result.Results != null)
                {
                    foreach (Answer entity in result.Results)
                    {
                        answers.Add(entity);
                        index++;
                        if (index == 500)
                            break;
                    }
                }

            } while (continuationToken != null);
            if (answers.Count > 0)
                return answers[0];
            return null;
        }
    }
}
*/
