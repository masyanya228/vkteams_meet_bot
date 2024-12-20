using vkteams.Entities;
using vkteams.Enums;

namespace vkteams.Tests
{
    public class TestFormFactory
    {
        public static Random Random = new Random();
        public static string[] Cities = new string[] { string.Empty, "Санкт-Петербург", "Ульяновск", "Казнь" };
        public static Person[] Persons = DBContext.Persons.FindAll().ToArray()
            /*.Where(x => x.TeamsUserLogin == "marsel.khabibullin@simbirsoft.com").ToArray()*/; //todo убрать тестовый блок

        /// <summary>
        /// Удаляет все анкеты, реации и страйки
        /// </summary>
        public void DeleteAllFormData()
        {
            DBContext.Forms.DeleteAll();
            DBContext.ReactionOnForms.DeleteAll();
            DBContext.Strikes.DeleteAll();
        }

        /// <summary>
        /// Создает 100 тестовых анкет
        /// </summary>
        public void GenerateTest()
        {
            Random random = new Random();
            var personTable = DBContext.Forms;
            Func<Form>[] funcs = new Func<Form>[]
            {
                () => CreateClubForm(),
                () => CreateHelpForm(),
                () => CreateRegularForm(),
                () => CreateFriendshipForm(),
            };
            foreach (var item in Enumerable.Range(0, 100))
            {
                personTable.Insert(funcs[random.Next(funcs.Length)]());
            }
            DBContext.DB.Checkpoint();
        }

        public Form CreateRegularForm()
        {
            var city = Cities[Random.Next(Cities.Length)];
            int age = Random.Next(18, 50);
            return new Form
            {
                Author = Persons[Random.Next(Persons.Length)],
                Type = FormType.Regular,
                Age = age,
                City = city,
                IsActive = true,
                ImageId = "0eAeP000GRHHVbGzBgAypa675f3c901ag",
                Text = $"Тестовый текст анкеты с параметрами:" +
                    $"\r\nВозраст: {age}" +
                    $"\r\nТип: {FormType.Regular}"
            };
        }

        public Form CreateHelpForm()
        {
            var city = Cities[Random.Next(Cities.Length)];
            int age = Random.Next(18, 50);
            return new Form
            {
                Author = Persons[Random.Next(Persons.Length)],
                Type = FormType.Help,
                Age = age,
                City = city,
                IsActive = true,
                Text = $"Тестовый текст анкеты с параметрами:" +
                    $"\r\nВозраст: {age}" +
                    $"\r\nТип: {FormType.Help}"
            };
        }

        public Form CreateClubForm()
        {
            var city = Cities[Random.Next(Cities.Length)];
            int age = Random.Next(18, 50);
            return new Form
            {
                Author = Persons[Random.Next(Persons.Length)],
                Type = FormType.Club,
                Age = age,
                City = city,
                IsActive = true,
                Text = $"Тестовый текст анкеты с параметрами:" +
                    $"\r\nВозраст: {age}" +
                    $"\r\nТип: {FormType.Club}"
            };
        }

        public Form CreateFriendshipForm()
        {
            int age = Random.Next(18, 50);
            var sex = Random.Next(0, 2) == 0
                ? Sex.Man
                : Sex.Woman;
            var sexOfPair = Random.Next(0, 2) != 0
                ? Random.Next(0, 2) == 0
                    ? Sex.Man
                    : Sex.Woman
                : Sex.Any;
            var city = Cities[Random.Next(Cities.Length)];
            var ageOfPairMin = Random.Next(0, 2) == 0
                ? -1
                : Random.Next(18, 30);
            var ageOfPairMax = Random.Next(0, 2) == 0
                ? -1
                : Random.Next(ageOfPairMin, 50);
            return new Form
            {
                Author = Persons[Random.Next(Persons.Length)],
                Type = FormType.Frendship,
                Age = age,
                Sex = sex,
                SexOfPair = sexOfPair,
                City = city,
                AgeOfPairMin = ageOfPairMin,
                AgeOfPairMax = ageOfPairMax,
                IsActive = true,
                ImageId = "0eAeP000GRHHVbGzBgAypa675f3c901ag",
                Text = $"Тестовый текст анкеты с параметрами:" +
                    $"\r\nТип: {FormType.Frendship}" +
                    $"\r\nВозраст: {age}" +
                    $"\r\nПол: {sex}" +
                    $"\r\nГород: {city}" +
                    $"\r\nПол друга: {sexOfPair}"
            };
        }
    }
}
