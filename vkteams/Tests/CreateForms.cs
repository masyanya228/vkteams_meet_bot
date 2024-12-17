using vkteams.Entities;
using vkteams.Enums;

namespace vkteams.Tests
{
    public class CreateForms
    {
        public static Random Random = new Random();
        public static string[] Cities = new string[] { string.Empty, "Ульяновск", "Казнь" };
        public static Person[] Persons = DBContext.Persons.FindAll().ToArray();
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
