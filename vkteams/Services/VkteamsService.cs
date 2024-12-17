using Buratino.Attributes;
using Buratino.Xtensions;

using System.Reflection;
using vkteams.DTOs.Teams;
using vkteams.Entities;
using vkteams.Enums;

namespace vkteams.Services
{
    public class VkteamsService
    {
        public RelevantQueueService QueueService = new RelevantQueueService();
        public ReportService ReportService {  get; set; }
        public VKTeamsAPI VKTeamsAPI { get; set; }
        public LikeDeliveryService LikeDeliveryService { get; set; }
        public VkteamsService(LogService logService, VKTeamsAPI vKTeamsAPI)
        {
            VKTeamsAPI = vKTeamsAPI;
            VKTeamsAPI.UpdateEvent += PipeLine;
            vKTeamsAPI.Listen();
            ReportService = new ReportService(logService, this);
            LikeDeliveryService = new LikeDeliveryService(logService, this);
        }

        private void PipeLine(object sender, Root updates)
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

        private string ProcessTextCommand(Event sourceEvent)
        {
            From from = sourceEvent.payload.from;
            Chat chat = sourceEvent.payload.chat;
            var chatId = chat?.chatId ?? from.userId;
            var person = VKTeamsAPI.GetPerson(from, chatId);

            var com = ParseCommand(sourceEvent.payload.text, out string[] args);
            if (com == null)
            {
                if (person.WaitingTextType != WaitingTextType.None)
                {
                    com = person.WaitingTextType.GetAttribute<TGPointerAttribute>()?.Pointers.SingleOrDefault();
                    if (person.WaitingTextType == WaitingTextType.Message)
                    {
                        args = new string[1] { sourceEvent.payload.text };
                    }
                }
                else
                {
                    return VKTeamsAPI.SendOrEdit(chatId, "Я не знаю что вам ответить на это:)");
                }
            }

            var availablePointers = this.GetMethodsWithAttribute<TGPointerAttribute>();
            var method = availablePointers.SingleOrDefault(x => x.Value.Pointers.Contains(com));
            if (method.Key is not null)
            {
                return InvokeCommand(method, chatId, null, args, person, sourceEvent);
            }
            else
            {
                return string.Empty;
            }
        }

        private string ProcessCallbackQuery(Event sourceEvent)
        {
            var com = ParseCommand(sourceEvent.payload.callbackData, out string[] args);
            var queryId = sourceEvent.payload.queryId;
            var chatId = sourceEvent.payload.chat?.chatId ?? sourceEvent.payload.from.userId;
            var messageId = sourceEvent.payload.message?.msgId ?? string.Empty;
            var person = VKTeamsAPI.GetPerson(sourceEvent.payload.from, chatId);

            var availablePointers = this.GetMethodsWithAttribute<TGPointerAttribute>();
            var method = availablePointers.SingleOrDefault(x => x.Value.Pointers.Contains(com));
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

        [TGPointer("start", "menu")]
        public string Start(object chatId, object messageId, Person person)
        {
            return VKTeamsAPI.SendOrEdit(chatId, "/rules - правила" +
                "\r\n/description - в чем смысл этого бота",
                messageId, new InlineKeyboardMarkup(
                    new InlineKeyboardMarkupButton("Смотреть анкеты", "/watch_forms"),
                    new InlineKeyboardMarkupButton("Моя анкета", "/my_form")
                ));
        }

        [TGPointer("description")]
        public string Description(object chatId, object messageId)
        {
            return VKTeamsAPI.SendOrEdit(chatId, "Этот бот сделан для сотрудников Симбирсофта сотрудниками Симбирсофта. Его роль - помогать с коммуникацией между сотрудниками из разных направлений компании." +
                "\r\nВот краткое описание возможностей:" +
                "\r\n- Поиск единомышленников" +
                "\r\n- Поиск специалистов для взаимопощи" +
                "\r\n- Поиск новых друзей. Возможно, кто-то найдет здесь свою вторую половинку <3" +
                "\r\n- Поиск желающих собраться с коллегами для проведения досуга",
                messageId,
                new InlineKeyboardMarkup(
                    new InlineKeyboardMarkupButton("🔙Назад", "/menu")
                ));
        }

        [TGPointer("rules")]
        public string Rules(object chatId, object messageId)
        {
            return VKTeamsAPI.SendOrEdit(chatId, "Мы за экологичное общение. Разрешено всё, что не запрещено здравым смыслом." +
                "\r\nДавайте договоримся, это наше пространство. Кусочек свободного интернета. И мы не будем тут гадить." +
                "\r\nУчтите, что есть система жалоб. Если посыл вашей анкеты не понравится большому количеству людей, анкета будет удалена." +
                "\r\nСпасибо за понимание. Если будут предложения, пишите сюда: %%",
                messageId,
                new InlineKeyboardMarkup(
                    new InlineKeyboardMarkupButton("🔙 Назад", "/menu")
                ));
        }

        [TGPointer("my_form")]
        public string MyForm(object chatId, object messageId, Person person)
        {
            var currentForm = person.GetCurrentForm();
            if (currentForm is null)
            {
                return VKTeamsAPI.SendOrEdit(chatId, "У тебя пока нет анкеты, давай заполним её!", messageId,
                    new InlineKeyboardMarkup(
                        new InlineKeyboardMarkupButton("📝 Заполнить", "/create_form"),
                        new InlineKeyboardMarkupButton("🔙 Назад", "/menu")
                    ));
            }
            if (currentForm.Type == FormType.None)
            {
                return SelectFormType(chatId, messageId, person);
            }
            else if (currentForm.Type == FormType.Frendship)
            {
                if (currentForm.Age == default)
                    return SelectAge(chatId, messageId, person);
                else if (currentForm.Sex == Sex.None)
                    return SelectSex(chatId, messageId, person);
                else if (currentForm.City == default)
                    return SelectCity(chatId, messageId, person);
                else if (currentForm.SexOfPair == default)
                    return SelectSexOfPair(chatId, messageId, person);
                else if (currentForm.AgeOfPairMin == default)
                    return SelectAgeOfPairMin(chatId, messageId, person);
                else if (currentForm.AgeOfPairMax == default)
                    return SelectAgeOfPairMax(chatId, messageId, person);
                else if (currentForm.ImageId == default)
                    return SelectImage(chatId, messageId, person);
            }
            else if (currentForm.Type == FormType.Help)
            {
                if (currentForm.Age == default)
                    return SelectAge(chatId, messageId, person);
                else if (currentForm.City == default)
                    return SelectCity(chatId, messageId, person);
            }
            else if (currentForm.Type == FormType.Club)
            {
                if (currentForm.Age == default)
                    return SelectAge(chatId, messageId, person);
                else if (currentForm.City == default)
                    return SelectCity(chatId, messageId, person);
            }
            else if (currentForm.Type == FormType.Regular)
            {
                if (currentForm.Age == default)
                    return SelectAge(chatId, messageId, person);
                else if (currentForm.City == default)
                    return SelectCity(chatId, messageId, person);
            }
            if (!currentForm.IsCompleted)
            {
                VKTeamsAPI.SendOrEdit(chatId, "Ура, ваша анкета заполнена. Теперь её увидят!");
                currentForm.IsCompleted = true;
                currentForm.IsActive = true;
                DBContext.Forms.Update(currentForm);
            }

            string text = currentForm.GetFormForAuthor(person);

            return VKTeamsAPI.SendOrEdit(chatId,
                text,
                messageId,
                new InlineKeyboardMarkup()
                    .AddButtonDown("🔍 Смотреть анкеты", "/watch_forms")
                    .AddButtonDown("📝 Заполнить заново", "/create_form")
                    .AddButtonDownIf(() => currentForm.IsActive, "❌ Скрыть анкету из поиска", "/hide_form")
                    .AddButtonDownIf(() => !currentForm.IsActive, "✅ Активировать анкету", "/show_form"),
                currentForm.ImageId
            );
        }

        private string SelectImage(object chatId, object messageId, Person person)
        {
            person.WaitingTextType = WaitingTextType.Image;
            DBContext.Persons.Update(person);
            
            var currentForm = DBContext.Forms.FindById(person.CurrentForm.Id);

            return VKTeamsAPI.SendOrEdit(chatId,
                $"С фото, твоя анкета будет намного привлекательнейю!" +
                $"\r\nПришли мне 1 картинку." +
                $"\r\n\r\nP.S.я не могу взять фото из vkteams:)",
                messageId,
                new InlineKeyboardMarkup()
                    .AddButtonDownIf(() => !string.IsNullOrEmpty(person.ImageId), $"Оставить прежнее фото", $"/set_lastImage")
            );
        }

        private string SelectAgeOfPairMax(object chatId, object messageId, Person person)
        {
            person.WaitingTextType = WaitingTextType.AgeOfPairMax;
            DBContext.Persons.Update(person);

            var currentForm = DBContext.Forms.FindById(person.CurrentForm.Id);

            return VKTeamsAPI.SendOrEdit(chatId,
                $"Укажите максимальный возраст друга:",
                messageId,
                new InlineKeyboardMarkup()
                    .AddButtonDownIf(() => currentForm.AgeOfPairMin > 0, $"Оставить только минимальный возраст", $"/set_ageOfPairMax/{-1}")
                    .AddButtonDownIf(() => currentForm.AgeOfPairMin < 1, $"Возраст неважен", $"/set_ageOfPairNone")
            );
        }

        private string SelectAgeOfPairMin(object chatId, object messageId, Person person)
        {
            person.WaitingTextType = WaitingTextType.AgeOfPairMin;
            DBContext.Persons.Update(person);
            return VKTeamsAPI.SendOrEdit(chatId,
                $"Укажите минимальный возраст друга:",
                messageId,
                new InlineKeyboardMarkup()
                    .AddButtonDown($"Указать только максимальный возраст", $"/set_ageOfPairMin/{-1}")
                    .AddButtonDown($"Возраст не важен", $"/set_ageOfPairNone")
            );
        }

        private string SelectSexOfPair(object chatId, object messageId, Person person)
        {
            return VKTeamsAPI.SendOrEdit(chatId,
                $"Выберите пол друга:",
                messageId,
                new InlineKeyboardMarkup()
                    .AddButtonDown("👨 Мужской", $"/set_sexOfPair/{nameof(Sex.Man)}")
                    .AddButtonDown("👩 Женский", $"/set_sexOfPair/{nameof(Sex.Woman)}")
                    .AddButtonDown("Неважно", $"/set_sexOfPair/{nameof(Sex.Any)}")
            );
        }

        private string SelectCity(object chatId, object messageId, Person person)
        {
            person.WaitingTextType = WaitingTextType.City;
            DBContext.Persons.Update(person);
            return VKTeamsAPI.SendOrEdit(chatId,
                $"Укажите ваш город:",
                messageId,
                new InlineKeyboardMarkup()
                    .AddButtonDownIf(() => person.CurrentCity != default, $"Оставить {person.CurrentCity}", $"/set_city/{person.CurrentCity}")
                    .AddButtonDown($"Не важно", $"/set_cityNone")
            );
        }

        private string SelectSex(object chatId, object messageId, Person person)
        {
            return VKTeamsAPI.SendOrEdit(chatId,
                $"Выберите ваш пол:",
                messageId,
                new InlineKeyboardMarkup()
                    .AddButtonDown("👨 Мужской", $"/set_sex/{nameof(Sex.Man)}")
                    .AddButtonDown("👩 Женский", $"/set_sex/{nameof(Sex.Woman)}")
            );
        }

        private string SelectAge(object chatId, object messageId, Person person)
        {
            person.WaitingTextType = WaitingTextType.Age;
            DBContext.Persons.Update(person);
            return VKTeamsAPI.SendOrEdit(chatId,
                $"Укажите ваш возраст:",
                messageId,
                new InlineKeyboardMarkup()
                    .AddButtonDownIf(() => person.Age != 0, $"Оставить {person.Age}", $"/set_age/{person.Age}")
            );
        }

        private string SelectFormType(object chatId, object messageId, Person person)
        {
            var formExist = DBContext.Forms.Exists(x => x.Author.Id == person.Id);
            return VKTeamsAPI.SendOrEdit(chatId,
                $"Выберите тип анкеты:",
                messageId,
                new InlineKeyboardMarkup()
                    .AddButtonDown("😁 Новые знакомства", "/set_form_type/Frendship")
                    .AddButtonDown("🚑 Взаимопомощь", "/set_form_type/Help")
                    .AddButtonDown("🎭 Клубы по интересам", "/set_form_type/Club")
                    .AddButtonDown("🗽 Свободная анкета", "/set_form_type/Regular")
                    .AddButtonDownIf(() => formExist, "🔙 Я передумал, оставлю старую анкету", "/return_oldform")
            );
        }

        [TGPointer("watch_forms")]
        private string Watch_forms(object chatId, object messageId, Person person)
        {
            var currentForm = person.GetCurrentForm();
            if (currentForm is null)
            {
                return VKTeamsAPI.SendOrEdit(chatId, "Чтобы просматривать чужие анкеты, для начала заполни свою.", messageId,
                    new InlineKeyboardMarkup(
                        new InlineKeyboardMarkupButton("📝 Заполнить", "/create_form"),
                        new InlineKeyboardMarkupButton("🔙 Назад", "/menu")
                    ));
            }

            var nextForm = QueueService.PeekNext(person);
            if (nextForm == null)
            {
                return VKTeamsAPI.SendOrEdit(
                    chatId,
                    $"Кажется у нас кончились анкета:(" +
                    $"\r\nНо вы не расстраивайтесь, попробуйте вернусть завтра." +
                    $"\r\nВаша анкета осталась активной. Возможно она кого-то заинтересует, тогда вам придет уведомление.",
                    null,
                    new InlineKeyboardMarkup()
                        .AddButtonDown("🔍 Смотреть анкеты", "/watch_forms")
                        .AddButtonDown("📝 Заполнить анкету заного", "/create_form"));
            }
            var author = DBContext.Persons.FindById(nextForm.Author.Id);
            return VKTeamsAPI.SendOrEdit(
                chatId,
                nextForm.GetForm(author),
                null,
                new InlineKeyboardMarkup()
                    .AddButtonRight("👍", $"/like/{nextForm.Id}")
                    .AddButtonRight("👎", $"/dislike/{nextForm.Id}")
                    .AddButtonRight("✉️", $"/message/{nextForm.Id}")
                    .AddButtonDown("👮‍♀️ Жалоба", $"/report/{nextForm.Id}")
                    .AddButtonRight("🍵 Пауза", $"/stop/{nextForm.Id}"),
                nextForm.ImageId);
        }

        [TGPointer("like")]
        private string Like(object chatId, object messageId, Person person, Event source, Guid formId)
        {
            var form = DBContext.Forms.FindById(formId);
            var author = DBContext.Persons.FindById(form.Author.Id);
            DBContext.WatchedForms.Insert(new WatchedForm()
            {
                MainForm = new Form()
                {
                    Id = person.CurrentForm.Id
                },
                Watched = form,
                Response = ResponseType.Liked,
            });

            QueueService.Dequeue(person);

            VKTeamsAPI.SendOrEdit(chatId, form.GetForm(author), messageId,
                new InlineKeyboardMarkup()
                    .AddButtonDown("Лайк", ""),
                form.ImageId
                );

            return Watch_forms(chatId, null, person);
        }

        [TGPointer("dislike")]
        private string Dislike(object chatId, object messageId, Person person, Event source, Guid formId)
        {
            var form = DBContext.Forms.FindById(formId);
            var author = DBContext.Persons.FindById(form.Author.Id);
            DBContext.WatchedForms.Insert(new WatchedForm()
            {
                MainForm = new Form()
                {
                    Id = person.CurrentForm.Id
                },
                Watched = form,
                Response = ResponseType.Disliked,
            });

            QueueService.Dequeue(person);

            VKTeamsAPI.SendOrEdit(chatId, form.GetForm(author), messageId,
                new InlineKeyboardMarkup()
                    .AddButtonDown("Дизлайк", ""),
                form.ImageId
                );

            return Watch_forms(chatId, null, person);
        }

        [TGPointer("report")]
        private string Report(object chatId, object messageId, Person person, Event source, Guid formId)
        {
            var form = DBContext.Forms.FindById(formId);
            var author = DBContext.Persons.FindById(form.Author.Id);
            DBContext.WatchedForms.Insert(new WatchedForm()
            {
                MainForm = new Form()
                {
                    Id = person.CurrentForm.Id
                },
                Watched = form,
                Response = ResponseType.Reported,
            });

            QueueService.Dequeue(person);

            bool isStriked = ReportService.Report(author, form);
            if(!isStriked)
            {
                VKTeamsAPI.AnswerCallbackQuery(source.payload.queryId, "Жалоба отправлена. Анкета скрыта от вас.");
            }

            VKTeamsAPI.SendOrEdit(chatId, form.GetForm(author), messageId,
                new InlineKeyboardMarkup()
                    .AddButtonDown("Жалоба", ""),
                form.ImageId
            );

            if (isStriked)
            {
                VKTeamsAPI.SendOrEdit(
                    chatId,
                    "Пользователь собрал 10 жалоб и получил страйк!",
                    null
                );
            }

            return Watch_forms(chatId, null, person);
        }

        [TGPointer("message")]
        private string Message(object chatId, Person person, Guid formId)
        {
            var form = DBContext.Forms.FindById(formId);
            person.WaitingTextType = WaitingTextType.Message;
            person.FormToMessage = form;
            DBContext.Persons.Update(person);

            return VKTeamsAPI.SendOrEdit(chatId,
                "Что передать?",
                null,
                new InlineKeyboardMarkup()
                    .AddButtonDown("Я передумал", "/cancel_message")
                );
        }

        [TGPointer("send_message")]
        private string SendMessage(object chatId, object messageId, Person person, string msg)
        {
            var form = DBContext.Forms.FindById(person.FormToMessage.Id);
            var author = DBContext.Persons.FindById(form.Author.Id);

            person.WaitingTextType = WaitingTextType.None;
            person.FormToMessage = null;
            DBContext.Persons.Update(person);

            //Отправка получателю
            VKTeamsAPI.SendOrEdit(person.TeamsUserLogin, msg);

            DBContext.WatchedForms.Insert(new WatchedForm()
            {
                MainForm = new Form()
                {
                    Id = person.CurrentForm.Id
                },
                Watched = form,
                Response = ResponseType.LikedWithMessage,
            });

            QueueService.Dequeue(person);

            VKTeamsAPI.SendOrEdit(chatId, form.GetForm(author), messageId,
                new InlineKeyboardMarkup()
                    .AddButtonDown("Лайк и сообщение", ""),
                form.ImageId
                );

            VKTeamsAPI.SendOrEdit(chatId, "Сообщение отправлено, ждем реакции!");

            return Watch_forms(chatId, null, person);
        }

        [TGPointer("cancel_message")]
        private string CancelMessage(object chatId, object messageId, Person person)
        {
            person.WaitingTextType = WaitingTextType.None;
            person.FormToMessage = null;
            DBContext.Persons.Update(person);

            return VKTeamsAPI.Delete(chatId, messageId);
        }

        [TGPointer("stop")]
        private string Stop(object chatId, object messageId, Person person, Guid formId)
        {
            return MyForm(chatId, null, person);
        }

        [TGPointer("set_image")]
        private string SetImage(object chatId, object messageId, Person person, Event source)
        {
            person.WaitingTextType = WaitingTextType.None;
            person.ImageId = source.payload.parts.FirstOrDefault(x => x.type == PartType.File).payload.fileId;
            DBContext.Persons.Update(person);

            var currentForm = DBContext.Forms.FindById(person.CurrentForm.Id);
            currentForm.ImageId = person.ImageId;
            DBContext.Forms.Update(currentForm);
            return MyForm(chatId, messageId, person);
        }

        [TGPointer("set_lastimage")]
        private string SetLastImage(object chatId, object messageId, Person person)
        {
            person.WaitingTextType = WaitingTextType.None;
            DBContext.Persons.Update(person);

            var currentForm = DBContext.Forms.FindById(person.CurrentForm.Id);
            currentForm.ImageId = person.ImageId;
            DBContext.Forms.Update(currentForm);
            return MyForm(chatId, messageId, person);
        }

        [TGPointer("set_ageofpairmin")]
        private string SetAgeOfPairMin(object chatId, object messageId, Person person, int age)
        {
            person.WaitingTextType = WaitingTextType.None;
            DBContext.Persons.Update(person);

            var currentForm = DBContext.Forms.FindById(person.CurrentForm.Id);
            currentForm.AgeOfPairMin = age;
            DBContext.Forms.Update(currentForm);
            return MyForm(chatId, messageId, person);
        }

        [TGPointer("set_ageofpairmax")]
        private string SetAgeOfPairMax(object chatId, object messageId, Person person, int age)
        {
            person.WaitingTextType = WaitingTextType.None;
            DBContext.Persons.Update(person);

            var currentForm = DBContext.Forms.FindById(person.CurrentForm.Id);
            currentForm.AgeOfPairMax = age;
            DBContext.Forms.Update(currentForm);
            return MyForm(chatId, messageId, person);
        }

        [TGPointer("set_ageofpairnone")]
        private string SetAgeOfPairNone(object chatId, object messageId, Person person)
        {
            person.WaitingTextType = WaitingTextType.None;
            DBContext.Persons.Update(person);

            var currentForm = DBContext.Forms.FindById(person.CurrentForm.Id);
            currentForm.AgeOfPairMin = -1;
            currentForm.AgeOfPairMax = -1;
            DBContext.Forms.Update(currentForm);
            return MyForm(chatId, messageId, person);
        }

        [TGPointer("set_sexofpair")]
        private string SetSexOfPair(object chatId, object messageId, Person person, Sex sex)
        {
            var currentForm = DBContext.Forms.FindById(person.CurrentForm.Id);
            currentForm.SexOfPair = sex;
            DBContext.Forms.Update(currentForm);
            return MyForm(chatId, messageId, person);
        }

        [TGPointer("set_citynone")]
        private string SetCityNone(object chatId, object messageId, Person person)
        {
            var currentForm = DBContext.Forms.FindById(person.CurrentForm.Id);
            currentForm.City = string.Empty;
            DBContext.Forms.Update(currentForm);
            return MyForm(chatId, messageId, person);
        }

        [TGPointer("set_city")]
        private string SetCity(object chatId, object messageId, Person person, string city)
        {
            person.CurrentCity = city;
            DBContext.Persons.Update(person);

            var currentForm = DBContext.Forms.FindById(person.CurrentForm.Id);
            currentForm.City = city;
            DBContext.Forms.Update(currentForm);
            return MyForm(chatId, messageId, person);
        }

        [TGPointer("set_sex")]
        private string SetSex(object chatId, object messageId, Person person, Sex sex)
        {
            person.Sex = sex;
            DBContext.Persons.Update(person);

            var currentForm = DBContext.Forms.FindById(person.CurrentForm.Id);
            currentForm.Sex = sex;
            DBContext.Forms.Update(currentForm);
            return MyForm(chatId, messageId, person);
        }

        [TGPointer("set_age")]
        private string SetAge(object chatId, object messageId, Person person, int age)
        {
            person.Age = age;
            person.WaitingTextType = WaitingTextType.None;
            DBContext.Persons.Update(person);

            var currentForm = DBContext.Forms.FindById(person.CurrentForm.Id);
            currentForm.Age = age;
            DBContext.Forms.Update(currentForm);
            return MyForm(chatId, messageId, person);
        }

        [TGPointer("set_form_type")]
        private string SetFormType(object chatId, object messageId, Person person, FormType formType)
        {
            var currentForm = DBContext.Forms.FindById(person.CurrentForm.Id);
            currentForm.Type = formType;
            DBContext.Forms.Update(currentForm);
            return MyForm(chatId, messageId, person);
        }

        [TGPointer("hide_form")]
        public string HideForm(object chatId, object messageId, Person person)
        {
            if (person.CurrentForm is null)
            {
                throw new Exception("У вас нет анкеты.");
            }

            var currentForm = DBContext.Forms.FindById(person.CurrentForm.Id);
            currentForm.IsActive = false;
            DBContext.Forms.Update(currentForm);

            return MyForm(chatId, messageId, person);
        }

        [TGPointer("show_form")]
        public string ShowForm(object chatId, object messageId, Person person)
        {
            if (person.CurrentForm is null)
            {
                throw new Exception("У вас нет анкеты.");
            }

            var currentForm = DBContext.Forms.FindById(person.CurrentForm.Id);
            currentForm.IsActive = true;
            DBContext.Forms.Update(currentForm);

            return MyForm(chatId, messageId, person);
        }

        [TGPointer("create_form")]
        public string CreateForm(object chatId, object messageId, Person person)
        {
            var currentForm = person.GetCurrentForm();
            if (currentForm != null)
            {
                currentForm.IsActive = false;
                DBContext.Forms.Update(currentForm);
            }
            currentForm = new Form() { Author = person };
            DBContext.Forms.Insert(currentForm);

            person.CurrentForm = currentForm;
            DBContext.Persons.Update(person);

            return MyForm(chatId, messageId, person);
        }

        [TGPointer("return_oldform")]
        public string ReturnOldForm(object chatId, object messageId, Person person)
        {
            var oldForm = DBContext.Forms.Query().Where(x => x.Author.Id == person.Id).OrderByDescending(x => x.Created).First();
            person.CurrentForm = oldForm;
            DBContext.Persons.Update(person);

            oldForm.IsActive = true;
            DBContext.Forms.Update(oldForm);
            
            return MyForm(chatId, messageId, person);
        }

        [TGPointer("view_response")]
        public string ViewResponse(object chatId, object messageId, Person person, Guid watchId)
        {
            var watch = DBContext.WatchedForms.FindById(watchId);
            

            return MyForm(chatId, messageId, person);
        }
    }
}
