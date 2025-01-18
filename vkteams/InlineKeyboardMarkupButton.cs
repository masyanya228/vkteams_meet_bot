namespace vkteams
{
    public class InlineKeyboardMarkupButton
    {
        public string text { get; set; }
        public string url { get; set; }
        public string callbackData { get; set; }
        public InlineKeyboardButtonStyle style { get; set; } =  InlineKeyboardButtonStyle.Primary;

        public InlineKeyboardMarkupButton(string text, string callbackData)
        {
            this.text = text;
            this.callbackData = callbackData;
        }

        public InlineKeyboardMarkupButton(string text, string url, InlineKeyboardButtonStyle style = InlineKeyboardButtonStyle.Primary)
        {
            this.text = text;
            this.url = url;
            this.style = style;
        }
    }
}
