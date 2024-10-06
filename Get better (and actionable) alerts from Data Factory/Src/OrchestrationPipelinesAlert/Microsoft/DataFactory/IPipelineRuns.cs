using DBojsen.OrchestrationPipelinesAlert.Entities;

namespace DBojsen.OrchestrationPipelinesAlert.Microsoft.DataFactory
{
    public interface IPipelineRuns
    {
        List<PipelineRunFailed> GetFailedPipelineRuns(string dataFactoryResourceId, DateTime windowStartTime, DateTime windowEndTime);
        PipelineRunFailed GetPipelineRun(string dataFactoryResourceId, string pipelineRunId);
        string RerunFailedPipelineRun(PipelineRunFailed pipelineRunFailed);
    }
}
