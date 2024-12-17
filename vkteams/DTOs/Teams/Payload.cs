namespace vkteams.DTOs.Teams
{
    public class Payload
    {
        public Chat chat { get; set; }
        public From from { get; set; }
        public string msgId { get; set; }
        public Message message { get; set; }
        public List<Part> parts { get; set; }
        public string text { get; set; }
        public int timestamp { get; set; }
        public string fileId { get; set; }
        public string type { get; set; }//image
        public int userId { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string caption { get; set; }
        public string callbackData { get; set; }
        public string queryId { get; set; }
    }
}
