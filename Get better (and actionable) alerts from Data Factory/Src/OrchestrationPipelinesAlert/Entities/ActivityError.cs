using System.Text.Json.Serialization;
// ReSharper disable UnusedMember.Global

namespace DBojsen.OrchestrationPipelinesAlert.Entities
{
    public class ActivityError
    {
        [JsonPropertyName("errorCode")]
        public string ErrorCode { get; set; } = null!;
        [JsonPropertyName("message")]
        public string Message { get; set; } = null!;
        [JsonPropertyName("failureType")]
        public string FailureType { get; set; } = null!;
        [JsonPropertyName("target")]
        public string Target { get; set; } = null!;
        [JsonPropertyName("details")]
        public string Details { get; set; } = null!;
    }
}
