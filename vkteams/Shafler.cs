using vkteams.Entities;

namespace vkteams
{
    public class Shafler
    {
        public IEnumerable<Pair> Shafle(IEnumerable<Person> people, out IList<Pair> doubles, out Stack<Person> withOutPair, DateTime date)
        {
            Random rnd = new Random();
            var newPairs = new List<Pair>();
            doubles = new List<Pair>();
            withOutPair = new Stack<Person>();

            var minDate = date.AddMonths(-4);
            var allPairs = DBContext.Pairs.Query().Where(x => x.Created > minDate).ToArray();

            var byCity = people.GroupBy(x => "Без города").ToList();
            foreach (var peoples in byCity)
            {
                var city = peoples.Key;
                var currentMatch = allPairs.Where(x => peoples.Any(y => y == x.Person) && peoples.Any(y => y == x.SubPerson)).ToList();

                var oldPairsByPeople = peoples
                    .Select(x => KeyValuePair.Create(x, currentMatch.Where(y => y.IsExist(x)).Select(y => y.GetPair(x))))
                    .OrderByDescending(x => x.Value.Count())
                    .ToList();

                for (int i = 0; i < oldPairsByPeople.Count; i++)
                {
                    bool isBestMatch;
                    var personWithOldPairs = oldPairsByPeople[i];

                    var allAvailable = oldPairsByPeople.Skip(i + 1).Select(x => x.Key);
                    if (!allAvailable.Any())
                    {
                        withOutPair.Push(personWithOldPairs.Key);
                        continue;
                    }
                    var bestAvailable = allAvailable.Except(personWithOldPairs.Value);
                    isBestMatch = bestAvailable.Any();
                    var available = isBestMatch ? bestAvailable : allAvailable;

                    var newSub = available.ToArray()[rnd.Next(available.Count())];
                    Pair newPair = new Pair(personWithOldPairs.Key, newSub, city)
                    {
                        Created = date
                    };
                    newPairs.Add(newPair);
                    if (!isBestMatch)
                    {
                        doubles.Add(newPair);
                    }

                    oldPairsByPeople.RemoveAt(oldPairsByPeople.IndexOf(oldPairsByPeople.Single(x => x.Key == newSub)));
                }
            }
            if (withOutPair.Any())
            {
                while (withOutPair.Any())
                {
                    var personWithOutPair = withOutPair.Pop();
                    var pairToAppend = newPairs.FirstOrDefault(x => !x.IsThirdPerson);
                    if (pairToAppend is null)
                        break;
                    pairToAppend.ThirdPerson = personWithOutPair;
                    pairToAppend.IsThirdPerson = true;
                }
            }
            DBContext.Pairs.Insert(newPairs);
            DBContext.DB.Checkpoint();
            return newPairs;
        }
    }
}
