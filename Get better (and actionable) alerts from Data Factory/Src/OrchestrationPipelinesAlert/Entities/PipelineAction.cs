namespace DBojsen.OrchestrationPipelinesAlert.Entities
{
    internal class PipelineAction
    {
        public required string Action { get; set; }
        public required string PipelineRunId { get; set; }
        public required string DataFactoryInstanceId { get; set; }
    }
}
