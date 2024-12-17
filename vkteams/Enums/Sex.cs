using Buratino.Attributes;

namespace vkteams.Enums
{
    public enum Sex
    {
        None = 0,
        
        [DisplayText("Мужчина", "Мужчину")]
        Man = 1,

        [DisplayText("Женщина", "Женщину")]
        Woman = 2,

        [DisplayText("Не важно", "Не важно")]
        Any = 3,
    }
}