namespace vkteams.DTOs.Teams
{
    public class Message
    {
        public From from { get; set; }
        public string msgId { get; set; }
        public string text { get; set; }
        public Format format { get; set; }
        public int timestamp { get; set; }
    }
}
