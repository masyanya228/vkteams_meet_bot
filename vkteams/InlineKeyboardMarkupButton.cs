namespace vkteams
{
    public class InlineKeyboardMarkupButton
    {
        public string text {  get; set; }
        public string url { get; set; }
        public string callbackData {  get; set; }
        public string style { get; set; } = "primary";

        public InlineKeyboardMarkupButton(string text, string callbackData)
        {
            this.text = text;
            this.callbackData = callbackData;
        }
    }
}
