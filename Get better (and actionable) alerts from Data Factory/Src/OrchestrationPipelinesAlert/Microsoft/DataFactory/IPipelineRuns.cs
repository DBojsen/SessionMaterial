using OrchestrationPipelinesAlert.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrchestrationPipelinesAlert.Microsoft.DataFactory
{
    public interface IPipelineRuns
    {
        List<PipelineRunFailed> GetFailedPipelineRuns(string dataFactoryResourceId, DateTime windowStartTime, DateTime windowEndTime);
        PipelineRunFailed GetPipelineRun(string dataFactoryResourceId, string pipelineRunId);
        string RerunFailedPipelineRun(PipelineRunFailed pipelineRunFailed);
    }
}
