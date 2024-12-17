using vkteams.Enums;

namespace vkteams.Entities
{
    /// <summary>
    /// Просмотренные анкеты
    /// </summary>
    public class WatchedForm : EntityBase
    {
        public Form MainForm { get; set; }
        public Form Watched { get; set; }
        public ResponseType Response { get; set; }
        public bool IsWatched { get; set; }
        public DateTime ResponseWatched { get; set; }
    }
}
