namespace vkteams.Entities
{
    public class Pair : EntityBase
    {
        public Person Person { get; set; }
        public Person SubPerson { get; set; }
        public Person ThirdPerson { get; set; }
        public bool IsThirdPerson {  get; set; }
        public string City { get; set; }
        public DateTime Created { get; set; } = DateTime.Now;

        public Pair(Person person, Person subPerson, string city)
        {
            Person = person;
            SubPerson = subPerson;
            City = city;
        }

        public bool IsExist(Person person) => Person == person || SubPerson == person;
        public Person GetPair(Person person) => Person == person ? SubPerson : (SubPerson == person ? Person : null);
    }
}
