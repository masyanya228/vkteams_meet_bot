using vkteams.Entities;
using vkteams.Enums;
using vkteams.Xtensions;

namespace vkteams.Services
{
    /// <summary>
    /// Сервис уведомлений о полученных лайках и взаимных лайках
    /// </summary>
    public class LikeDeliveryService
    {
        public LogService LogService { get; }
        public VkteamsService VkteamsService { get; }
        
        public LikeDeliveryService(LogService logService, VkteamsService vkteamsService)
        {
            LogService = logService;
            VkteamsService = vkteamsService;
        }

        /// <summary>
        /// Возвращает полученные лайки
        /// </summary>
        /// <param name="person"></param>
        /// <returns></returns>
        public IEnumerable<ReactionOnForm> GetLikesByPerson(Person person)
        {
            person.Fetch();
            var forms = DBContext.Forms.Query().Where(x => x.Author.Id == person.Id).ToArray();
            var myFormIds = forms.Select(x => x.Id);

            var requestes = DBContext.ReactionOnForms.Query()
                .Where(x => !x.IsResponsed)
                .Where(x => myFormIds.Contains(x.RequestedForm.Id))
                .Where(x => x.Request == ReactionType.Liked || x.Request == ReactionType.LikedWithMessage)
                .ToArray();
            return requestes;
        }

        /// <summary>
        /// Возвращает взаимный лайки
        /// </summary>
        /// <param name="person"></param>
        /// <returns></returns>
        public IEnumerable<ReactionOnForm> GetMatchesByPerson(Person person)
        {
            person.Fetch();
            var forms = DBContext.Forms.Query().Where(x => x.Author.Id == person.Id).ToArray();
            var myFormIds = forms.Select(x => x.Id);

            var requestesWhatILiked = DBContext.ReactionOnForms.Query()
                .Where(x => myFormIds.Contains(x.RequestedForm.Id))
                .Where(x => x.Request == ReactionType.Liked || x.Request == ReactionType.LikedWithMessage)
                .Where(x => x.Response == ReactionType.Liked || x.Request == ReactionType.LikedWithMessage)
                .ToArray();

            var myLikesWithLikeResponsed = DBContext.ReactionOnForms.Query()
                .Where(x => myFormIds.Contains(x.MainForm.Id))
                .Where(x => x.Request == ReactionType.Liked || x.Request == ReactionType.LikedWithMessage)
                .Where(x => x.Response == ReactionType.Liked || x.Request == ReactionType.LikedWithMessage)
                .ToArray();

            return requestesWhatILiked.Concat(myLikesWithLikeResponsed).OrderBy(x => x.ResponseTime);
        }

        public string SendNewLikesNotification(Person person)
        {
            var likes = GetLikesByPerson(person);
            return person.SendMessage(VkteamsService, $"Вашей анкетой кто-то заинтересовался!" +
                $"\r\nУ вас {likes.Count().TrueNumbers("новый лайк", "новых лайка", "новых лайков")}.",
                new InlineKeyboardMarkup()
                    .AddButtonRight("Посмотреть лайки", "/view_likes"));
        }

        public string SendNewMathesNotification(Person person)
        {
            var likes = GetMatchesByPerson(person);
            return person.SendMessage(VkteamsService, $"Это МАТЧ!" +
                $"\r\nУ вас {likes.Count().TrueNumbers("взаимный лайк", "взаимных лайка", "взаимных лайков")}.",
                new InlineKeyboardMarkup()
                    .AddButtonRight("Посмотреть взаимные лайки", "/view_mathes"));
        }
    }
}
