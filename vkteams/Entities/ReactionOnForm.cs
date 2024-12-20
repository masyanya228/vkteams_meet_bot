using vkteams.Enums;

namespace vkteams.Entities
{
    /// <summary>
    /// Реакции между анкетами
    /// </summary>
    public class ReactionOnForm : EntityBase
    {
        public Form MainForm { get; set; }
        public Form RequestedForm { get; set; }
        public ReactionType Request { get; set; }
        public string Message { get; set; }

        public bool IsResponsed { get; set; }
        public DateTime ResponseTime { get; set; }
        public ReactionType Response { get; set; }

        public string GetLinkOnOtherAuthor(Person person)
        {
            if (MainForm is null || RequestedForm is null)
                return string.Empty;

            //todo - сделать рекурсивный fetch
            Fetch();
            MainForm.Fetch();
            RequestedForm.Fetch();
            
            if (MainForm.Author == person)
                return $"@[{RequestedForm.Author?.TeamsUserLogin ?? string.Empty}]";
            else
                return $"@[{MainForm.Author?.TeamsUserLogin ?? string.Empty}]";
        }
    }
}
