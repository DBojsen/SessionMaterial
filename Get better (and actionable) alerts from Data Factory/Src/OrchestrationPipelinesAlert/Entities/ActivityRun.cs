using Azure.ResourceManager.DataFactory.Models;
// ReSharper disable UnusedMember.Global

namespace DBojsen.OrchestrationPipelinesAlert.Entities
{
    public class ActivityRun(PipelineActivityRunInformation activityRunInformation)
    {
        public PipelineActivityRunInformation ActivityRunInformation { get; } = activityRunInformation;
        public string ActivityStart => ActivityRunInformation.StartOn!.Value.DateTime.ToString("yyyy-MM-dd HH:mm:ss");
        public string ActivityEnd => ActivityRunInformation.EndOn!.Value.DateTime.ToString("yyyy-MM-dd HH:mm:ss");
        public ActivityError ActivityError { get; private set; } = activityRunInformation.Error.ToObjectFromJson<ActivityError>();
        // TODO: Input not deserializing properly
        public ActivityInputVariable? ActivityInput { get; } = activityRunInformation.Input.ToObjectFromJson<ActivityInputVariable>();
        public bool HasActivityInputs => ActivityInput != null;
    }
}
