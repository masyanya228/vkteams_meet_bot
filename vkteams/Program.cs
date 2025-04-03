using Newtonsoft.Json.Linq;

using vkteams;
using vkteams.Services;

public class Program
{
    private static void Main(string[] args)
    {
        LogService logService = new LogService();
        AppDomain.CurrentDomain.ProcessExit += new EventHandler((object sender, EventArgs e) => ConsoleCtrlCheck(sender, e, logService));

        new VkteamsService(logService, new VKTeamsAPI(logService, GetVKTeamsApiKey()));
    }

    private static void ConsoleCtrlCheck(object sender, EventArgs e, LogService logService)
    {
        logService.Dispose();
        var res = DBContext.DB.Commit();
        DBContext.DB.Checkpoint();
        DBContext.DB.Dispose();
        Console.WriteLine(res);
    }

    public static string GetVKTeamsApiKey()
    {
        using (StreamReader r = new StreamReader("appsettings.json"))
        {
            string json = r.ReadToEnd();
            var items = JObject.Parse(json);
            return items.SelectToken("VKTeamsApiKey").ToString();
        }
    }
}