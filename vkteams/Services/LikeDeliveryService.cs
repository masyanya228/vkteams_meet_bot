using vkteams.Entities;

namespace vkteams.Services
{
    /// <summary>
    /// Сервис постепенной рассылки первичных уведомлений о полученных лайках и сообщений
    /// </summary>
    public class LikeDeliveryService
    {
        private const int _resposiesPerIteration = 10;
        private Queue<WatchedForm> Resonsies = new Queue<WatchedForm>();

        public LikeDeliveryService(LogService logService, VkteamsService vkteamsService)
        {
            LogService = logService;
            VkteamsService = vkteamsService;
        }

        public LogService LogService { get; }
        public VkteamsService VkteamsService { get; }

        public int SendResponsies()
        {
            for (int i = 0; i < _resposiesPerIteration; i++)
            {
                var next = Resonsies.Dequeue();
                if (next == null)
                    return i;
                var watched = DBContext.Forms.FindById(next.Watched.Id);
                var mainForm = DBContext.Forms.FindById(next.MainForm.Id);
                var watchedPerson = DBContext.Persons.FindById(watched.Author.Id);
                var mainFormPerson = DBContext.Persons.FindById(mainForm.Author.Id);

                watchedPerson.SendMessage(VkteamsService, "Вашей анкетой кто-то заинтересовался!",
                    new InlineKeyboardMarkup()
                        .AddButtonRight("Посмотреть", $"/view_response/{next.Id}")
                );
            }
            return _resposiesPerIteration;
        }
    }
}
