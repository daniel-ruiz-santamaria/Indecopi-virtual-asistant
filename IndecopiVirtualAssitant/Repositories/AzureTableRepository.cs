using IndecopiVirtualAssitant.Models;
using IndecopiVirtualAssitant.Models.AzureTable;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IndecopiVirtualAssitant.Repositories
{
    public class AzureTableRepository: IAzureTableRepository {

        public String storageConnectionString { get; set; }

        public AzureTableRepository(IConfiguration conf)
        {
            storageConnectionString = conf["AzureStorageConnectionString"];
        }

        public async Task<CloudTable> CreateTableAzureStorage(String tableName) {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageConnectionString);
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient(new TableClientConfiguration());
            CloudTable table = tableClient.GetTableReference(tableName);
            await table.CreateIfNotExistsAsync();
            return table;
        }

        public async Task<Audit> SaveAuditData(string tableName, Audit audit)
        {
            CloudTable table = await this.CreateTableAzureStorage(tableName);
            TableOperation insertOrMergeOperation = TableOperation.InsertOrMerge(audit);
            TableResult result = await table.ExecuteAsync(insertOrMergeOperation);
            Audit insertedAudit = result.Result as Audit;
            return insertedAudit;
        }

        public async Task<User> SaveUserData(string tableName, User user)
        {
            CloudTable table = await this.CreateTableAzureStorage(tableName);
            TableOperation insertOrMergeOperation = TableOperation.InsertOrMerge(user);
            TableResult result = await table.ExecuteAsync(insertOrMergeOperation);
            User insertedUser = result.Result as User;
            return insertedUser;
        }

        public async Task<SupportRequest> SaveSupportRequestData(string tableName, SupportRequest sr)
        {
            CloudTable table = await this.CreateTableAzureStorage(tableName);
            TableOperation insertOrMergeOperation = TableOperation.InsertOrMerge(sr);
            TableResult result = await table.ExecuteAsync(insertOrMergeOperation);
            SupportRequest inserted = result.Result as SupportRequest;
            return inserted;
        }

        public async Task<Feedback> SaveFeedbackData(string tableName, Feedback f)
        {
            CloudTable table = await this.CreateTableAzureStorage(tableName);
            TableOperation insertOrMergeOperation = TableOperation.InsertOrMerge(f);
            TableResult result = await table.ExecuteAsync(insertOrMergeOperation);
            Feedback inserted = result.Result as Feedback;
            return inserted;
        }

        public async Task<string> getAnswer(string tableName, string intent, String defaultAnswer)
        {
            List<Answer> answers = new List<Answer>();
            CloudTable table = await this.CreateTableAzureStorage(tableName);
            string filter = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, intent);
            TableContinuationToken continuationToken = null;
            do
            {
                var result = await table.ExecuteQuerySegmentedAsync(new TableQuery<Answer>().Where(filter), continuationToken);
                continuationToken = result.ContinuationToken;
                if (result.Results != null)
                {
                    foreach (Answer entity in result.Results)
                    {
                        answers.Add(entity);
                    }
                }

            } while (continuationToken != null);
            if (answers.Count > 0)
            {
                int index = new Random().Next(0, answers.Count);
                return answers[index].answer;
            }
            return defaultAnswer;
        }

        public async Task<List<AssistantData>> getAssistantData(string tableName, string partitionKey, string key)
        {
            List<AssistantData> data = new List<AssistantData>();
            CloudTable table = await this.CreateTableAzureStorage(tableName);
            string partitionFilter = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey);
            string keyFilter = TableQuery.GenerateFilterCondition("key", QueryComparisons.Equal, key);
            string filter = TableQuery.CombineFilters(partitionFilter, TableOperators.And, keyFilter);
            TableContinuationToken continuationToken = null;
            do
            {
                var result = await table.ExecuteQuerySegmentedAsync(new TableQuery<AssistantData>().Where(filter), continuationToken);
                continuationToken = result.ContinuationToken;
                if (result.Results != null)
                {
                    foreach (AssistantData entity in result.Results)
                    {
                        data.Add(entity);
                    }
                }

            } while (continuationToken != null);
            return data;
        }

        public async Task<List<AssistantData>> getAssistantData(string tableName, string partitionKey)
        {
            List<AssistantData> data = new List<AssistantData>();
            CloudTable table = await this.CreateTableAzureStorage(tableName);
            string filter = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey);
            TableContinuationToken continuationToken = null;
            do
            {
                var result = await table.ExecuteQuerySegmentedAsync(new TableQuery<AssistantData>().Where(filter), continuationToken);
                continuationToken = result.ContinuationToken;
                if (result.Results != null)
                {
                    foreach (AssistantData entity in result.Results)
                    {
                        data.Add(entity);
                    }
                }

            } while (continuationToken != null);
            return data;
        }
    }
}
