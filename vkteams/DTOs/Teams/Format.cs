namespace vkteams.DTOs.Teams
{
    public class Format
    {
        public List<Bold> bold { get; set; }
        public List<Italic> italic { get; set; }
        public List<Underline> underline { get; set; }
        public List<Strikethrough> strikethrough { get; set; }
        public List<Link> link { get; set; }
        public List<Mention> mention { get; set; }
        public List<InlineCode> inline_code { get; set; }
        public List<Pre> pre { get; set; }
        public List<OrderedList> ordered_list { get; set; }
        public List<UnorderedList> unordered_list { get; set; }
        public List<Quote> quote { get; set; }
    }
}
