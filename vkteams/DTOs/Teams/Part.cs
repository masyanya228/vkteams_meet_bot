using System.Text.Json.Serialization;

using vkteams.Enums;

namespace vkteams.DTOs.Teams
{
    public class Part
    {
        public Payload payload { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public PartType type { get; set; }
    }
}
