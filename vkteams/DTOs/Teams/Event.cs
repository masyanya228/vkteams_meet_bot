using System.Text.Json.Serialization;

using vkteams.Enums;

namespace vkteams.DTOs.Teams
{
    public class Event
    {
        public int eventId { get; set; }
        public Payload payload { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public EventType type { get; set; }

        public string GetChatId() => payload.chat?.chatId ?? payload.from.userId;

        public string GetQueryId() => payload.queryId;
    }
}
