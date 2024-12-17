using Buratino.Xtensions;

using System.Collections.Concurrent;

using vkteams.Entities;
using vkteams.Enums;
using vkteams.Xtensions;

namespace vkteams.Services
{
    public class RelevantQueueService
    {
        private ConcurrentDictionary<Person, Queue<Form>> RelevantQueues = new ConcurrentDictionary<Person, Queue<Form>>();
        public Form PeekNext(Person person)
        {
            if (RelevantQueues.TryGetValue(person, out var forms) && forms.Any())
            {
                return forms.Peek();
            }
            else
            {
                RelevantQueues[person] = CreateQueue(person);
                return RelevantQueues[person].Peek();
            }
        }

        public Form Dequeue(Person person)
        {
            if (RelevantQueues.TryGetValue(person, out var forms) && forms.Any())
            {
                return forms.Dequeue();
            }
            return null;
        }

        private Queue<Form> CreateQueue(Person person)
        {
            var currentForm = person.GetCurrentForm();
            if (currentForm is null)
            {
                throw new ArgumentNullException("У вас пока нет анкеты. Заполните её, чтобы просматривать другие анкеты.");
            }

            var watched = DBContext.WatchedForms.Query()
                .Where(x => x.MainForm.Id == currentForm.Id)
                .ToArray();
            var watchedIds = watched.Select(x => x.Id).ToArray();

            var reportedPersons = watched
                .Where(x => x.Response == ResponseType.Reported)
                .Where(x => x.Created > DateTime.Now.AddDays(-7))
                .Select(x => x.Watched.Author.Id)
                .Distinct()
                .ToList();

            var availableForms = DBContext.Forms.Query()
                .Where(x => x.IsActive && x.Author.Id != person.Id)
                .ToArray()
                .Where(x => !watchedIds.Contains(x.Id))
                .Where(x => !reportedPersons.Contains(x.Author.Id))
                .ToArray();

            IEnumerable<Form> queue;
            if (currentForm.Type == FormType.Regular)
            {
                queue = availableForms
                    .OrderIf(() => currentForm.City != string.Empty, x => x.City == currentForm.City)
                    .OrderByDescending(x => x.Type == FormType.Regular);
            }
            else if (currentForm.Type == FormType.Help)
            {
                queue = availableForms
                    .OrderByDescending(x => x.Type == FormType.Help);
            }
            else if (currentForm.Type == FormType.Club)
            {
                queue = availableForms
                    .OrderIf(() => currentForm.City != string.Empty, x => x.City == currentForm.City)
                    .OrderByDescending(x => x.Type == FormType.Club);
            }
            else if (currentForm.Type == FormType.Frendship)
            {
                queue = availableForms
                    .OrderBy(x =>
                    {
                        var totalAccurate = 0;
                        if (x.Type != FormType.Frendship)
                        {
                            return 1000;
                        }
                        if (currentForm.SexOfPair != Sex.Any)
                        {
                            if (x.Sex == currentForm.SexOfPair && x.SexOfPair == currentForm.Sex)
                                totalAccurate += 0;
                            else if (x.Sex == currentForm.SexOfPair && x.SexOfPair == Sex.Any)
                                totalAccurate += 1;
                            else if (x.Sex != currentForm.Sex)
                                totalAccurate += 2;
                        }
                        else
                        {
                            if (x.SexOfPair == currentForm.Sex)
                                totalAccurate += 0;
                            else if (x.SexOfPair == Sex.Any)
                                totalAccurate += 1;
                            else if (x.Sex != currentForm.Sex)
                                totalAccurate += 2;
                        }

                        totalAccurate += x.GetAccurateByAge(currentForm.Age);
                        totalAccurate += currentForm.GetAccurateByAge(x.Age);

                        if (currentForm.City != string.Empty)
                        {
                            if (x.City == currentForm.City)
                                totalAccurate += 0;
                            else if (x.City != string.Empty)
                                totalAccurate += 5;
                            else
                                totalAccurate += 10;
                        }
                        return totalAccurate;
                    });
            }
            else
            {
                throw new NotImplementedException("Не возможно создать очередь для такого типа анкеты");
            }

            if (currentForm.Type == FormType.Frendship)
            {
                queue = queue.Where(x =>
                {
                    if (x.Type == FormType.Frendship)
                        return x.Sex == currentForm.SexOfPair && (x.SexOfPair == Sex.Any || x.SexOfPair == currentForm.Sex);
                    else
                        return true;
                });
            }
            return new Queue<Form>(queue.Take(50).ToArray());
        }
    }
}
