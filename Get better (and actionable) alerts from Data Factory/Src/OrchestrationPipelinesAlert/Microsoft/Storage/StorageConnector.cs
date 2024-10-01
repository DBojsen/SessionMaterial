using Azure.Data.Tables;

namespace OrchestrationPipelinesAlert.Microsoft.Storage
{
    internal class StorageConnector : IStorageConnector
    {
        public TableClient TableClient { get; internal set; }

        public StorageConnector()
        {
            TableClient = new TableClient(Environment.GetEnvironmentVariable("AzureWebJobsStorage"), Environment.GetEnvironmentVariable("AzureStorageTables_TableName"));
            TableClient.CreateIfNotExists();
        }
    }
}
