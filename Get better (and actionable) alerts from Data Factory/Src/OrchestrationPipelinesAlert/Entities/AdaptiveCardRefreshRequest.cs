namespace DBojsen.OrchestrationPipelinesAlert.Entities
{
    internal class AdaptiveCardRefreshRequest
    {
        public required string PipelineRunId { get; set; }
        public required string DataFactoryInstanceId { get; set; }
    }
}
