using Buratino.Attributes;

namespace vkteams.Enums
{
    /// <summary>
    /// Флаг, указывающий что ожидается в следующем сообщении от пользователя
    /// </summary>
    public enum WaitingTextType
    {
        None = 0,

        [TGPointer("set_age")]
        Age = 1,

        [TGPointer("set_ageofpairmin")]
        AgeOfPairMin = 3,

        [TGPointer("set_ageofpairmax")]
        AgeOfPairMax = 4,

        [TGPointer("set_city")]
        City = 7,

        [TGPointer("set_image")]
        Image = 8,

        [TGPointer("send_message")]
        Message = 9,

        [TGPointer("set_text")]
        Text = 10,
    }
}