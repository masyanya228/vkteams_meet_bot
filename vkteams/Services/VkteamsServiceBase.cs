using Buratino.Attributes;
using Buratino.Xtensions;

using System.Reflection;

using vkteams.DTOs.Teams;
using vkteams.Entities;
using vkteams.Enums;

namespace vkteams.Services
{
    public class VkteamsServiceBase
    {
        public RelevantQueueService QueueService = new RelevantQueueService();

        public ReportService ReportService { get; set; }
        public VKTeamsAPI VKTeamsAPI { get; set; }
        public LikeDeliveryService LikeDeliveryService { get; set; }

        private IEnumerable<KeyValuePair<MethodInfo, TGPointerAttribute>> _availablePointers = null;
        public IEnumerable<KeyValuePair<MethodInfo, TGPointerAttribute>> AvailablePointers
        {
            get
            {
                if (_availablePointers is null)
                {
                    _availablePointers = this.GetMethodsWithAttribute<TGPointerAttribute>();
                }
                return _availablePointers;
            }
            set => _availablePointers = value;
        }

        /// <summary>
        /// Обработать обновления
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="updates"></param>
        public void PipeLine(object sender, Root updates)
        {
            foreach (var item in updates.events)
            {
                try
                {
                    VKTeamsAPI.UpdateLastMsg(item.eventId);

                    if (item.type == EventType.NewMessage && item.payload.chat.type == "private")
                    {
                        ProcessTextCommand(item);
                    }
                    else if (item.type == EventType.CallbackQuery)
                    {
                        ProcessCallbackQuery(item);
                    }
                    else
                    {
                        VKTeamsAPI.SendOrEdit(item.payload.chat.chatId, "Я не умею работать с такими сообщениями :(");
                    }
                }
                catch
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Обрабокать колбэк запрос
        /// </summary>
        /// <param name="sourceEvent"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private string ProcessCallbackQuery(Event sourceEvent)
        {
            var com = ParseCommand(sourceEvent.payload.callbackData, out string[] args);
            var chatId = sourceEvent.GetChatId();
            var person = VKTeamsAPI.GetPerson(sourceEvent.payload.from, chatId);

            var queryId = sourceEvent.GetQueryId();
            var messageId = sourceEvent.payload.message?.msgId ?? string.Empty;

            var method = GetMethod(com);
            if (method.Key is not null)
            {
                var res = InvokeCommand(method, chatId, messageId, args, person, sourceEvent);
                VKTeamsAPI.AnswerCallbackQuery(queryId);
                return res;
            }
            else
            {
                throw new Exception("Не поддерживаемая команда");
            }
        }

        /// <summary>
        /// Обработать текстовый запрос
        /// </summary>
        /// <param name="sourceEvent"></param>
        /// <returns></returns>
        private string ProcessTextCommand(Event sourceEvent)
        {
            var com = ParseCommand(sourceEvent.payload.text, out string[] args);
            var chatId = sourceEvent.GetChatId();
            var person = VKTeamsAPI.GetPerson(sourceEvent.payload.from, chatId);

            if (com == null)
            {
                if (person.WaitingText != WaitingTextType.None)
                {
                    com = person.WaitingText.GetAttribute<TGPointerAttribute>()?.Pointers.SingleOrDefault();
                    if (person.WaitingText == WaitingTextType.Message)
                    {
                        args = new string[1] { sourceEvent.payload.text };
                    }
                }
                else
                {
                    return VKTeamsAPI.SendOrEdit(chatId, "Я не знаю что вам ответить на это:)");
                }
            }

            var method = GetMethod(com);
            if (method.Key is not null)
            {
                return InvokeCommand(method, chatId, null, args, person, sourceEvent);
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Разобрать запрос на команду и аргументы
        /// </summary>
        /// <param name="query"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        private string ParseCommand(string query, out string[] args)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                throw new ArgumentException($"\"{nameof(query)}\" не может быть пустым или содержать только пробел.", nameof(query));
            }
            if (query.StartsWith('/'))
            {
                query = query.Substring(1);
                args = query.Split('/').Skip(1).ToArray();
                return query.Split('/').First().ToLower();
            }
            else
            {
                args = query.Split("/").ToArray();
                return null;
            }
        }

        /// <summary>
        /// Получить метод
        /// </summary>
        /// <param name="com"></param>
        /// <returns></returns>
        private KeyValuePair<MethodInfo, TGPointerAttribute> GetMethod(string com)
        {
            var method = AvailablePointers.SingleOrDefault(x => x.Value.Pointers.Contains(com));
            return method;
        }

        /// <summary>
        /// Подготовить аргументы и вызвать метод
        /// </summary>
        /// <param name="method"></param>
        /// <param name="chat"></param>
        /// <param name="messageId"></param>
        /// <param name="args"></param>
        /// <param name="person"></param>
        /// <param name="sourceEvent"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        private string InvokeCommand(KeyValuePair<MethodInfo, TGPointerAttribute> method, object chat, object messageId, string[] args, Person person = null, Event sourceEvent = null)
        {
            var parameters = method.Key.GetParameters();
            var arguments = new object[parameters.Length];
            var comArgs = new Queue<string>(args);
            for (int i = 0; i < parameters.Length; i++)
            {
                var item = parameters[i];
                if (item.Name == "chatId")
                    arguments[i] = chat;
                else if (item.Name == "messageId")
                    arguments[i] = messageId;
                else if (item.Name == "person")
                    arguments[i] = person;
                else if (item.Name == "source")
                    arguments[i] = sourceEvent;
                else if (comArgs.Any())
                {
                    arguments[i] = comArgs.Dequeue().Cast(item.ParameterType);
                }
                else if (item.IsOptional)
                {
                    arguments[i] = item.DefaultValue;
                }
                else
                {
                    throw new ArgumentException("Не хватает аргументов для вызова метода");
                }
            }
            return method.Key.Invoke(this, arguments).ToString();
        }
    }
}