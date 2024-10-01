using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrchestrationPipelinesAlert.Entities
{
    internal class AdaptiveCardRefreshRequest
    {
        public required string PipelineRunId { get; set; }
        public required string DataFactoryInstanceId { get; set; }
    }
}
