namespace vkteams
{
    public class InlineKeyboardMarkupRow
    {
        public List<InlineKeyboardMarkupButton> Buttons { get; set; } = new List<InlineKeyboardMarkupButton>();

        public InlineKeyboardMarkupRow(params InlineKeyboardMarkupButton[] buttons)
        {
            Buttons.AddRange(buttons);
        }
    }
}
