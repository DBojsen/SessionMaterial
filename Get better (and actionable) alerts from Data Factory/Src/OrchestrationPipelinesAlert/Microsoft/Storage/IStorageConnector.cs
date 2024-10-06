using Azure.Data.Tables;

namespace DBojsen.OrchestrationPipelinesAlert.Microsoft.Storage
{
    public interface IStorageConnector
    {
        public TableClient TableClient { get; }
    }
}
