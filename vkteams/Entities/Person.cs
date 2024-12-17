using vkteams.Enums;
using vkteams.Services;

namespace vkteams.Entities
{
    /// <summary>
    /// Пользователь
    /// </summary>
    public class Person : EntityBase
    {
        public Form CurrentForm { get; set; }
        public string TeamsUserLogin { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string CurrentCity { get; set; }
        public DateTime Created { get; set; } = DateTime.Now;
        public DateTime LastActivity { get; set; } = DateTime.Now;
        public int Age { get; set; }
        public Sex Sex { get; set; }
        public WaitingTextType WaitingTextType { get; set; }
        public string ImageId { get; set; }
        public Form FormToMessage { get; set; }

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
