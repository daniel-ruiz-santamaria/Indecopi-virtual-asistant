using IndecopiVirtualAssitant.Models;
using IndecopiVirtualAssitant.Models.AzureTable;
using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IndecopiVirtualAssitant.Repositories
{
    public interface IAzureTableRepository
    {
        public Task<CloudTable> CreateTableAzureStorage(String tableName);

        public Task<Audit> SaveAuditData(String tableName, Audit audit);

        public Task<User> SaveUserData(String tableName, User user);

        public Task<SupportRequest> SaveSupportRequestData(string tableName, SupportRequest sr);

        public Task<Feedback> SaveFeedbackData(string tableName, Feedback f);

        public Task<String> getAnswer(String tableName, String intent, String defaultAnswer);


        public Task<List<AssistantData>> getAssistantData(string tableName, string partitionKey, string key);

        public Task<List<AssistantData>> getAssistantData(string tableName, string partitionKey);
    }
}
