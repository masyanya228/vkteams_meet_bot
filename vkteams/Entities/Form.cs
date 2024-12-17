using Buratino.Attributes;
using Buratino.Xtensions;

using vkteams.Enums;

namespace vkteams.Entities
{
    /// <summary>
    /// Анкета
    /// </summary>
    public class Form : EntityBase
    {
        public Person Author { get; set; }
        public FormType Type{ get; set; }
        public string Text { get; set; }
        public int AgeOfPairMin { get; set; }
        public int AgeOfPairMax { get; set; }
        public Sex SexOfPair { get; set; }
        public bool IsActive { get; set; }
        public bool IsCompleted { get; set; }
        public int Age { get; set; }
        public Sex Sex { get; set; }
        public string City { get; set; }
        public string ImageId { get; set; }

        public int GetAccurateByAge(int age)
        {
            if (age == -1)
            {
                return 10;
            }
            if (AgeOfPairMin > 0 && AgeOfPairMax > 0)
            {
                if (age.Between(AgeOfPairMin, AgeOfPairMax))
                    return 0;
                else
                {
                    if (AgeOfPairMin < age)
                        return age - AgeOfPairMax;
                    else
                        return AgeOfPairMin - age;
                }
            }
            else if (AgeOfPairMin > 0)
            {
                if (AgeOfPairMin <= age)
                    return 0;
                else
                    return AgeOfPairMin - age;
            }
            else if (AgeOfPairMax > 0)
            {
                if (AgeOfPairMax >= age)
                    return 0;
                else
                    return age - AgeOfPairMax;
            }
            else
            {
                return 0;
            }
        }

        public string GetAgeOfPairRange()
        {
            if (AgeOfPairMin > 0 && AgeOfPairMax > 0)
            {
                return $"от {AgeOfPairMin} до {AgeOfPairMax}";
            }
            else if (AgeOfPairMin > 0)
            {
                return $"от {AgeOfPairMin}";
            }
            else if (AgeOfPairMax > 0)
            {
                return $"до {AgeOfPairMax}";
            }
            else
            {
                return "не важно";
            }
        }

        public string GetFormForAuthor(Person person)
        {
            string text;
            if (Type == FormType.Frendship)
            {
                text = $"😁 {person.FirstName}, {Age}";
                if (City != default)
                    text += $", {City}";
                text += $"\r\n{Text}" +
                    $"\r\n\r\nЭта информация видна только вам:" +
                    $"\r\nИщем: {SexOfPair.GetAttribute<DisplayTextAttribute>().NameGenitive}" +
                    $"\r\nВозрастом: {GetAgeOfPairRange()}";
            }
            else if (Type == FormType.Help)
            {
                text = $"🚑 {person.FirstName}";
                if (City != default)
                    text += $", {City}";
                text += $"{Text}";
            }
            else if (Type == FormType.Club)
            {
                text = $"🎭 {person.FirstName}";
                if (City != default)
                    text += $", {City}";
                text += $"{Text}";
            }
            else
            {
                text = $"{Type} {person.FirstName}";
                if (City != default)
                    text += $", {City}";
                text += $"{Text}";
            }
            return text;
        }

        public string GetForm(Person person)
        {
            string text;
            if (Type == FormType.Frendship)
            {
                text = $"😁 {person.FirstName}, {Age}";
                if (City != default)
                    text += $", {City}";
                text += $"\r\n{Text}";
            }
            else if (Type == FormType.Help)
            {
                text = $"🚑 {person.FirstName}";
                if (City != default)
                    text += $", {City}";
                text += $"{Text}";
            }
            else if (Type == FormType.Club)
            {
                text = $"🎭 {person.FirstName}";
                if (City != default)
                    text += $", {City}";
                text += $"{Text}";
            }
            else
            {
                text = $"🗽 {person.FirstName}";
                if (City != default)
                    text += $", {City}";
                text += $"{Text}";
            }
            return text;
        }
    }
}
