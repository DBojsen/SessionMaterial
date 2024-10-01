using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure;
using Azure.ResourceManager.DataFactory.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OrchestrationPipelinesAlert.Entities
{
    public class ActivityRun
    {
        public ActivityRun(PipelineActivityRunInformation activityRunInformation)
        {
            ActivityRunInformation = activityRunInformation;
            
            ActivityInput = activityRunInformation.Input.ToObjectFromJson<ActivityInputVariable>();
            ActivityError = activityRunInformation.Error.ToObjectFromJson<ActivityError>();
        }

        public PipelineActivityRunInformation ActivityRunInformation { get; private set; }
        public string ActivityStart => ActivityRunInformation.StartOn.Value.DateTime.ToString("yyyy-MM-dd HH:mm:ss");
        public string ActivityEnd => ActivityRunInformation.EndOn.Value.DateTime.ToString("yyyy-MM-dd HH:mm:ss");
        public ActivityError ActivityError { get; private set; }
        public ActivityInputVariable? ActivityInput { get; private set; }
        public bool HasActivityInputs => ActivityInput != null;
    }
}
