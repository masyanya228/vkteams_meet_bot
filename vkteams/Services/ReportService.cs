using vkteams.Entities;
using vkteams.Enums;

namespace vkteams.Services
{
    /// <summary>
    /// Сервис подсчитывает количество жалоб и выдает страйки
    /// </summary>
    public class ReportService
    {
        /// <summary>
        /// Период блокировки аккаунта при 1-ом, 2-ом и последующих страйках
        /// </summary>
        public readonly int[] CallDownInDays = new int[3]
        {
            1,
            3,
            7
        };

        /// <summary>
        /// Количество репортов, достаточных для получения страйка
        /// </summary>
        public const int ReportCountToStrike = 10;

        public LogService LogService { get; }
        public VkteamsService VkteamsService { get; }

        public ReportService(LogService logService, VkteamsService vkteamsService)
        {
            LogService = logService;
            VkteamsService = vkteamsService;
        }

        /// <summary>
        /// Возвращает true, если пользователь получил Strike
        /// </summary>
        /// <param name="person"></param>
        /// <param name="form"></param>
        /// <returns></returns>
        public bool Report(Person person, Form form)
        {
            var strikes = DBContext.Strikes.Query().Where(x => x.Person.Id == person.Id).ToArray();
            var myReports = DBContext.ReactionOnForms.Query()
                .Where(x => x.Request == ReactionType.Reported)
                .Where(x => x.RequestedForm.Author.Id == person.Id)
                .Where(x => x.Created > DateTime.Now.AddDays(-7))
                .ToArray();
            if (strikes.Any())
            {
                myReports = myReports.Where(x => x.Created > strikes.Max(y => y.LastReportOfStrike)).ToArray();
            }

            if (myReports.Count() >= ReportCountToStrike)
            {
                var callDown = CallDownInDays[Math.Min(strikes.Length, 2)];
                Strike strike = new Strike()
                {
                    Person = person,
                    LastReportOfStrike = myReports.Max(x => x.Created),
                    StrikeEnd = DateTime.Now.AddDays(callDown)
                };
                DBContext.Strikes.Insert(strike);

                form.IsActive = false;

                person.SendMessage(VkteamsService, $"Ваша анкета от {form.Created:f} набрала слишком много жалоб." +
                    $"\r\nВам выдан {strikes.Length + 1} страйк." +
                    $"\r\nДоступ к боту ограничен. Блокировка закончится {strike.StrikeEnd:f} MSK");

                return true;
            }
            return false;
        }
    }
}
