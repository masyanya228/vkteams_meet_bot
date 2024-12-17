using vkteams;
using vkteams.Entities;
using vkteams.Services;
using vkteams.Tests;

LogService logService = new LogService();
Console.WriteLine("Hello, World!");

//DBContext.Forms.DeleteAll();
//GenerateTest();
new VkteamsService(logService, new VKTeamsAPI(logService, "001.3196337833.1460183339:1011824590"));

static void GenerateTest()
{
    var formFactory = new CreateForms();
    Random random = new Random();
    var personTable = DBContext.Forms;
    Func<Form>[] funcs = new Func<Form>[]
    {
        ()=>formFactory.CreateClubForm(),
        ()=>formFactory.CreateHelpForm(),
        ()=>formFactory.CreateRegularForm(),
        ()=>formFactory.CreateFriendshipForm(),
    };
    foreach (var item in Enumerable.Range(0, 100))
    {
        personTable.Insert(funcs[random.Next(funcs.Length)]());
    }
    DBContext.DB.Checkpoint();
}

static void TestShafle()
{
    IList<Pair> doubles;
    Stack<Person> withOutPair;
    int round = 0;
    var date = DateTime.Now;
    Random rnd = new Random();
    do
    {
        round++;
        var testPersons = DBContext.Persons.FindAll()
            //.Take(40)
            .Skip(rnd.Next(1, 500))
            .Take(rnd.Next(1, 1000))
            .ToArray();
        var pairs = new Shafler().Shafle(testPersons, out doubles, out withOutPair, date);
        //check
        var personsInPairs = pairs.SelectMany(x => new Person[] { x.SubPerson, x.Person });
        var ch1 = personsInPairs.Count() + withOutPair.Count() == testPersons.Count();
        var ch2 = personsInPairs.Count() + withOutPair.Count() == personsInPairs.Union(withOutPair).Count();
        if (!ch1 | !ch2)
            throw new Exception();

        date = date.AddMonths(1);
    } while (true);
}