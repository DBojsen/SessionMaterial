using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Azure.ResourceManager.DataFactory;
using Azure.ResourceManager.DataFactory.Models;

namespace OrchestrationPipelinesAlert.Entities
{
    public class PipelineRunFailed(DataFactoryResource dataFactoryInstance, DataFactoryPipelineRunInfo pipelineRun, List<ActivityRun> allActivities, List<ActivityRun> failedActivities)
    {
        public string Environment { get; private set; } = 
            dataFactoryInstance.Id.Name.IndexOf("prd", StringComparison.InvariantCultureIgnoreCase) > 0 ? "Production" :
            dataFactoryInstance.Id.Name.IndexOf("tst", StringComparison.InvariantCultureIgnoreCase) > 0 ? "Test" : "Development";

        public string DataFactoryResourceName = dataFactoryInstance.Id.Name;
        public DataFactoryPipelineRunInfo PipelineRun { get; private set; } = pipelineRun;
        public List<ActivityRun> AllActivities { get; private set; } = allActivities;
        public List<ActivityRun> FailedActivities { get; private set; } = failedActivities;
        public int FailedActivityCount => FailedActivities.Count;
        public string DataFactoryInstanceUrlEncoded => HttpUtility.UrlEncode(dataFactoryInstance.Id.ToString());
        public string DataFactoryInstanceId => dataFactoryInstance.Id.ToString();
        public string PipelineRunStart = pipelineRun.RunStartOn.Value.DateTime.ToString("yyyy-MM-dd HH:mm:ss");
        public string PipelineRunEnd = pipelineRun.RunEndOn.Value.DateTime.ToString("yyyy-MM-dd HH:mm:ss");
        public bool HasFailedActivities = failedActivities.Count != 0;
        public bool ShowButtons { get; set; } = true;
        public string ActionUser { get; set; } = "";
        public string DecidedAction { get; set; } = "";
    }
}
