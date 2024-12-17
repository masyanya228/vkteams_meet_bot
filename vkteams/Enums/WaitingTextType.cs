using Buratino.Attributes;

namespace vkteams.Enums
{
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
    }
}