namespace vkteams.Entities
{
    /// <summary>
    /// Просмотренные анкеты
    /// </summary>
    public class Strike : EntityBase
    {
        public Person Person { get; set; }
        public DateTime LastReportOfStrike { get; set; }
        public DateTime StrikeEnd { get; set; }
    }
}
