using System.Text.Json.Serialization;
// ReSharper disable UnusedMember.Global

namespace DBojsen.OrchestrationPipelinesAlert.Entities
{
    public class ActivityInputVariable
    {
        [JsonPropertyName("variableName")]
        public string VariableName { get; set; } = null!;

        [JsonPropertyName("value")]
        public object Value { get; set; } = null!;
    }
}
