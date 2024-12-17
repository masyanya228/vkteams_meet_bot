using vkteams;

namespace Buratino.Xtensions
{
    public static class KeyboardConstructorXtensions
    {
        public static InlineKeyboardMarkup AddButtonDownIf(this InlineKeyboardMarkup constructor, Func<bool> func, string title, string callbackQuery)
            => func()
                ? constructor.AddButtonDown(title, callbackQuery)
                : constructor;

        public static InlineKeyboardMarkup AddButtonRightIf(this InlineKeyboardMarkup constructor, Func<bool> func, string title, string callbackQuery)
            => func()
                ? constructor.AddButtonRight(title, callbackQuery)
                : constructor;
    }
}
