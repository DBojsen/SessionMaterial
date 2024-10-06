using Azure;

namespace DBojsen.OrchestrationPipelinesAlert.Entities
{
    internal class PipelinesAlertTableData : Azure.Data.Tables.ITableEntity
    {
        public PipelinesAlertTableData()
        {
            // Parameterless constructor has to be here to be able to use the TableEntity
        }

        public PipelinesAlertTableData(string pipelineRunId)
        {
            RowKey = pipelineRunId;
            PipelineRunId = pipelineRunId;
        }

        //ITableEntity
        public string PartitionKey { get; set; } = Environment.GetEnvironmentVariable("AzureStorageTables_TableName") ?? throw new InvalidOperationException();
        public string RowKey { get; set; } = null!;
        public DateTimeOffset? Timestamp { get; set; } = DateTimeOffset.UtcNow;
        public ETag ETag { get; set; }

        // Entity specific
        public string PipelineRunId { get; set; } = null!;
        public string ChosenAction { get; set; } = null!;
        public string ActionUser { get; set; } = null!;
    }
}
