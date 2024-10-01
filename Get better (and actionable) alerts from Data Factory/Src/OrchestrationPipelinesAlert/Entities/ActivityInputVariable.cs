using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace OrchestrationPipelinesAlert.Entities
{
    public class ActivityInputVariable
    {
        [JsonPropertyName("variableName")]
        public string VariableName { get; set; }
        [JsonPropertyName("value")]
        public object Value { get; set; }
    }
}
