using System.Text.Json;
using System.Text.Json.Serialization;

namespace vkteams
{
    public class InlineKeyboardMarkup
    {
        public List<InlineKeyboardMarkupRow> Rows { get; set; } = new List<InlineKeyboardMarkupRow>();

        /// <summary>
        /// Возвращает клавиатуру для сообщения в формате для TeamsAPI в виде JSON
        /// </summary>
        /// <returns></returns>
        public string GetData()
            => Rows == null || Rows.Count(x => x.Buttons != null && x.Buttons.Any()) > 0
                ? JsonSerializer.Serialize(Rows.Where(x => x.Buttons != null && x.Buttons.Any()).Select(x => x.Buttons), new JsonSerializerOptions() { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull })
                : null;

        public InlineKeyboardMarkup(params InlineKeyboardMarkupButton[] buttons)
        {
            if (buttons != null && buttons.Any())
                Rows.Add(new InlineKeyboardMarkupRow(buttons));
        }

        public InlineKeyboardMarkup AddButtonDown(string title, string callbackQuery)
        {
            Rows.Add(new InlineKeyboardMarkupRow(new InlineKeyboardMarkupButton(title, callbackQuery)));
            return this;
        }

        public InlineKeyboardMarkup AddButtonRight(string title, string callbackQuery)
        {
            if (Rows.Count == 0)
            {
                Rows.Add(new InlineKeyboardMarkupRow());
            }
            Rows.Last().Buttons.Add(new InlineKeyboardMarkupButton(title, callbackQuery));
            return this;
        }

        public InlineKeyboardMarkup AddUrlDown(string title, string url, InlineKeyboardButtonStyle style = InlineKeyboardButtonStyle.Primary)
        {
            Rows.Add(new InlineKeyboardMarkupRow(new InlineKeyboardMarkupButton(title, url, style)));
            return this;
        }

        public InlineKeyboardMarkup AddUrlRight(string title, string url, InlineKeyboardButtonStyle style = InlineKeyboardButtonStyle.Primary)
        {
            if (Rows.Count == 0)
            {
                Rows.Add(new InlineKeyboardMarkupRow());
            }
            Rows.Last().Buttons.Add(new InlineKeyboardMarkupButton(title, url, style));
            return this;
        }
    }
}
