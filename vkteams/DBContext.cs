using LiteDB;

using vkteams.Entities;

namespace vkteams
{
    public static class DBContext
    {
        public static readonly LiteDatabase DB = new LiteDatabase("db1.db");
        public static ILiteCollection<Person> Persons;
        public static ILiteCollection<Pair> Pairs;
        public static ILiteCollection<Form> Forms;
        public static ILiteCollection<ReactionOnForm> ReactionOnForms;
        public static ILiteCollection<Strike> Strikes;
        static DBContext()
        {
            Persons = DB.GetCollection<Person>();
            Pairs = DB.GetCollection<Pair>();
            Forms = DB.GetCollection<Form>();
            ReactionOnForms = DB.GetCollection<ReactionOnForm>();
            Strikes = DB.GetCollection<Strike>();

            DB.Mapper.EmptyStringToNull = false;
            DB.Mapper.EnumAsInteger = true;

            DB.Mapper.Entity<Person>().DbRef(x => x.CurrentForm);
            DB.Mapper.Entity<Person>().DbRef(x => x.FormToMessage);
            DB.Mapper.Entity<Pair>().DbRef(x => x.Person);
            DB.Mapper.Entity<Pair>().DbRef(x => x.SubPerson);
            DB.Mapper.Entity<Pair>().DbRef(x => x.ThirdPerson);
            DB.Mapper.Entity<Form>().DbRef(x => x.Author);
            DB.Mapper.Entity<ReactionOnForm>().DbRef(x => x.MainForm);
            DB.Mapper.Entity<ReactionOnForm>().DbRef(x => x.RequestedForm);
            DB.Mapper.Entity<Strike>().DbRef(x => x.Person);
        }
    }
}
