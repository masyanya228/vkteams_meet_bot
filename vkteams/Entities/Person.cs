using vkteams.Enums;
using vkteams.Services;

namespace vkteams.Entities
{
    /// <summary>
    /// Пользователь
    /// </summary>
    public class Person : EntityBase
    {
        /// <summary>
        /// Текущая активная форма
        /// </summary>
        public Form CurrentForm { get; set; }

        /// <summary>
        /// Логин и, по совместительству, chatId пользователя
        /// </summary>
        public string TeamsUserLogin { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string CurrentCity { get; set; }
        public int Age { get; set; }

        ///<inheritdoc cref="Enums.Sex"/>
        public Sex Sex { get; set; }

        ///<inheritdoc cref="WaitingTextType"/>
        public WaitingTextType WaitingText { get; set; }

        /// <summary>
        /// vkteams fileId
        /// </summary>
        public string ImageId { get; set; }

        /// <summary>
        /// Анкета, которой пользователь хочет отправить сообщение
        /// </summary>
        public Form FormToMessage { get; set; }

        public DateTime LastActivity { get; set; } = DateTime.Now;

        public bool IsActive { get; set; }

        public Form GetCurrentForm()
        {
            if (CurrentForm != null)
                return DBContext.Forms.FindById(CurrentForm.Id);
            return null;
        }

        public string SendMessage(VkteamsService vkteamsService, string message, InlineKeyboardMarkup inlineKeyboard=null)
        {
            return vkteamsService.VKTeamsAPI.SendOrEdit(TeamsUserLogin, message, null, inlineKeyboard);
        }
    }
}
