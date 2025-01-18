using vkteams;
using vkteams.Services;
using vkteams.Tests;

public class Program
{
    private static void Main(string[] args)
    {
        LogService logService = new LogService();
        AppDomain.CurrentDomain.ProcessExit += new EventHandler(ConsoleCtrlCheck);
        
        var formFactory = new TestFormFactory();
        //formFactory.DeleteAllFormData();
        //formFactory.GenerateTest();

        new VkteamsService(logService, new VKTeamsAPI(logService, "001.3196337833.1460183339:1011824590"));//todo - вынести токен в config.json

        void ConsoleCtrlCheck(object sender, EventArgs e)
        {
            logService.Dispose();
            var res = DBContext.DB.Commit();
            DBContext.DB.Checkpoint();
            DBContext.DB.Dispose();
            Console.WriteLine(res);
            Thread.Sleep(500);//todo - почитать доку базы данных, по идее эта задержка не нужно, ведь Commit(), Checkpoint() и Dispose() - синхронные
        }
    }
}