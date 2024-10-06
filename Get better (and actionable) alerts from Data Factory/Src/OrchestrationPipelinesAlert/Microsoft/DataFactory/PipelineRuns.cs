using Azure.Core;
using Azure.ResourceManager.DataFactory.Models;
using DBojsen.OrchestrationPipelinesAlert.Entities;

namespace DBojsen.OrchestrationPipelinesAlert.Microsoft.DataFactory
{
    internal class PipelineRuns : IPipelineRuns
    {
        internal readonly ServiceConnector ServiceConnector = new();

        public List<PipelineRunFailed> GetFailedPipelineRuns(string dataFactoryResourceId, DateTime windowStartTime, DateTime windowEndTime)
        {
            var dataFactoryResourceIdentifier = new ResourceIdentifier(dataFactoryResourceId);
            var dataFactoryResource = ServiceConnector.Connect(
                dataFactoryResourceIdentifier.SubscriptionId ?? throw new InvalidOperationException(),
                dataFactoryResourceIdentifier.ResourceGroupName ?? throw new InvalidOperationException(),
                dataFactoryResourceIdentifier.Name);

            // Query the log from data factory
            var failedPipelineRuns = dataFactoryResource.GetPipelineRuns(new RunFilterContent(windowStartTime, windowEndTime) { Filters = { new RunQueryFilter(RunQueryFilterOperand.Status, RunQueryFilterOperator.EqualsValue,
                ["Failed"]) } });

            return (from failedPipeline in failedPipelineRuns let activities = dataFactoryResource.GetActivityRun(failedPipeline.RunId.ToString(), new RunFilterContent(failedPipeline.RunStartOn!.Value, failedPipeline.RunEndOn!.Value)) select new PipelineRunFailed(dataFactoryResource, failedPipeline, activities.ToList().Select(act => new ActivityRun(act)).ToList(), activities.ToList().Where(run => run.Status == "Failed").Select(act => new ActivityRun(act)).ToList())).ToList();
        }

        public PipelineRunFailed GetPipelineRun(string dataFactoryResourceId, string pipelineRunId)
        {
            var dataFactoryResourceIdentifier = new ResourceIdentifier(dataFactoryResourceId);
            var dataFactoryResource = ServiceConnector.Connect(
                dataFactoryResourceIdentifier.SubscriptionId ?? throw new InvalidOperationException(),
                dataFactoryResourceIdentifier.ResourceGroupName ?? throw new InvalidOperationException(),
                dataFactoryResourceIdentifier.Name);
            var pipelineRun = dataFactoryResource.GetPipelineRun(pipelineRunId).Value;
            var activities = dataFactoryResource.GetActivityRun(pipelineRunId, new RunFilterContent(pipelineRun.RunStartOn!.Value, pipelineRun.RunEndOn!.Value));
            return new PipelineRunFailed(
                dataFactoryResource,
                pipelineRun,
                activities.ToList().Select(act => new ActivityRun(act)).ToList(),
                activities.ToList().Where(run => run.Status == "Failed").Select(act => new ActivityRun(act)).ToList());
        }

        public string RerunFailedPipelineRun(PipelineRunFailed pipelineRunFailed)
        {
            var dataFactoryResourceIdentifier = new ResourceIdentifier(pipelineRunFailed.DataFactoryInstanceId);
            var dataFactoryResource = ServiceConnector.Connect(
                dataFactoryResourceIdentifier.SubscriptionId ?? throw new InvalidOperationException(),
                dataFactoryResourceIdentifier.ResourceGroupName ?? throw new InvalidOperationException(),
                dataFactoryResourceIdentifier.Name);

            var failedPipeline = pipelineRunFailed.PipelineRun;

            var rerunPipeline = dataFactoryResource.GetDataFactoryPipeline(failedPipeline.PipelineName).Value;
            var rerunResult = rerunPipeline.CreateRun(referencePipelineRunId: pipelineRunFailed.PipelineRun.RunId.ToString(), isRecovery: true);

            return rerunResult.Value.RunId.ToString();
        }
    }
}
