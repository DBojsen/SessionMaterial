using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace OrchestrationPipelinesAlert.Entities
{
    public class ActivityError
    {
        [JsonPropertyName("errorCode")]
        public string ErrorCode { get; set; }
        [JsonPropertyName("message")]
        public string Message { get; set; }
        [JsonPropertyName("failureType")]
        public string FailureType { get; set; }
        [JsonPropertyName("target")]
        public string Target { get; set; }
        [JsonPropertyName("details")]
        public string Details { get; set; }
    }
}
