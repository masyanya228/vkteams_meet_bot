using System.Text.Json;
using vkteams.DTOs.Teams;
using vkteams.Entities;
using vkteams.Enums;
using vkteams.Services;

namespace vkteams
{
    public class VKTeamsAPI
    {
        private const string APIUrl = "https://myteam.mail.ru/bot/v1/";
        private readonly LogService logService;
        private readonly string Token;
        private int LastEventId = 0;
        private readonly int PollTime = 30;
        private bool IsWorking;

        public delegate void APIUpdateEventHandler(object sender, Root updates);

        public event APIUpdateEventHandler UpdateEvent;

        public VKTeamsAPI(LogService logService, string token)
        {
            this.logService = logService;
            Token = token ?? "001.3196337833.1460183339:1011824590";
            IsWorking = true;
        }

        public void GetMe()
        {
            var res = new HttpClient().GetAsync(APIUrl + "/self/get" + $"?token={Token}").GetAwaiter().GetResult();
            var text = res.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        }

        public void Listen()
        {
            while (IsWorking)
            {
                try
                {
                    GetEvents();
                }
                catch (Exception ex)
                {
                    logService.Log(ex);
                }
            }
        }

        public void GetEvents()
        {
            Root root = WaitUpdates();
            UpdateEvent.Invoke(this, root);
        }

        private Root WaitUpdates()
        {
            var res = new HttpClient().GetAsync(APIUrl + "/events/get" + $"?token={Token}&lastEventId={LastEventId}&pollTime={PollTime}").GetAwaiter().GetResult();
            var text = res.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            Root root = JsonSerializer.Deserialize<Root>(text);
            return root;
        }

        public Person GetPerson(From from, string chatId)
        {
            var currentUser = DBContext.Persons.Query().Where(x => x.TeamsUserLogin == chatId).SingleOrDefault();
            if (currentUser == null)
            {
                currentUser = new Person()
                {
                    TeamsUserLogin = chatId,
                    FirstName = from.firstName,
                    LastName = from.lastName
                };
                DBContext.Persons.Insert(currentUser);
            }
            else
            {
                currentUser.LastActivity = DateTime.Now;
                DBContext.Persons.Update(currentUser);
            }

            return currentUser;
        }

        public string SendOrEdit(object chatId, string text, object msgId = null, InlineKeyboardMarkup inlineKeyboard = null, string imageId = null)
        {
            return msgId == null
                        ? imageId == null
                            ? Send(chatId, text, inlineKeyboard)
                            : SendFile(chatId, imageId, text, inlineKeyboard)
                        : imageId == null
                            ? Edit(chatId, msgId, text, inlineKeyboard)
                            : ReplaceMessages(chatId, msgId, text, inlineKeyboard, imageId);
        }

        private string Send(object chatId, string text, InlineKeyboardMarkup inlineKeyboard = null)
        {
            var response = new HttpClient().GetAsync(
                APIUrl + "/messages/sendText" + $"?token={Token}&chatId={chatId}&text={text}" +
                (inlineKeyboard?.GetData() != null ? $"&inlineKeyboardMarkup={inlineKeyboard.GetData()}" : "")
                ).GetAwaiter().GetResult();
            return response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        }

        private string Edit(object chatId, object messageId, string text, InlineKeyboardMarkup inlineKeyboard = null)
        {
            var response = new HttpClient().GetAsync(
                APIUrl + "/messages/editText" + $"?token={Token}&chatId={chatId}&msgId={messageId}&text={text}" +
                (inlineKeyboard?.GetData() != null ? $"&inlineKeyboardMarkup={inlineKeyboard.GetData()}" : "")
                ).GetAwaiter().GetResult();
            return response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        }

        public string Delete(object chatId, object messageId)
        {
            var response = new HttpClient().GetAsync(
                APIUrl + "/messages/deleteMessages" + $"?token={Token}&chatId={chatId}&msgId={messageId}").GetAwaiter().GetResult();
            return response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        }

        private string ReplaceMessages(object chatId, object messageId, string text, InlineKeyboardMarkup inlineKeyboard = null, string imageId = null)
        {
            Delete(chatId, messageId);
            return SendOrEdit(chatId, text, null, inlineKeyboard, imageId);
        }

        private string SendFile(object chatId, string imageId, string caption, InlineKeyboardMarkup inlineKeyboard = null)
        {
            var captionBlock = !string.IsNullOrEmpty(caption) ? $"&caption={caption}" : "";
            var response = new HttpClient().GetAsync(
                APIUrl + "/messages/sendFile" + $"?token={Token}&chatId={chatId}&fileId={imageId}{captionBlock}" +
                (inlineKeyboard?.GetData() != null ? $"&inlineKeyboardMarkup={inlineKeyboard.GetData()}" : "")
                ).GetAwaiter().GetResult();
            return response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        }

        public void AnswerCallbackQuery(object queryId, string text = null)
        {
            var query = $"?token={Token}&queryId={queryId}";
            if (text != null)
                query += $"&text={text}";
            var response = new HttpClient().GetAsync(APIUrl + "/messages/answerCallbackQuery" + query).GetAwaiter().GetResult();
            var txt = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        }

        public void SendActions(object chatId, params ChatAction[] actions)
        {
            var response = new HttpClient().GetAsync(APIUrl + "/chats/sendActions" + $"?token={Token}&chatId={chatId}&actions[]={actions[0].ToString().ToLower()}").GetAwaiter().GetResult();
            var txt = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        }

        public void UpdateLastMsg(int eventId)
        {
            LastEventId = LastEventId < eventId
                ? eventId
                : LastEventId;
        }
    }
}
