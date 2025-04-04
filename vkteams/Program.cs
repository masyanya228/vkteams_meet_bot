using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using vkteams;
using vkteams.Services;

public class Program
{
    private static void Main(string[] args)
    {
        //IoC - рабочий
        IServiceCollection services = new ServiceCollection();
        services.AddSingleton(typeof(LogService), new LogService());
        var serviceProvider = services.BuildServiceProvider();
        var logService = serviceProvider.GetService<LogService>();

        //App config - рабочий
        IConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.AddEnvironmentVariables();
        configurationBuilder.AddJsonFile("appsettings.json");
        var config = configurationBuilder.Build();
        var apiKey = config.GetValue("VKTeamsApiKey", string.Empty);

        //Host - нерабочий
        ConfigureHost(Host.CreateDefaultBuilder(args)).Build().Run();

        AppDomain.CurrentDomain.ProcessExit += new EventHandler((object sender, EventArgs e) => ConsoleCtrlCheck(sender, e, logService));

        new VkteamsService(logService, new VKTeamsAPI(logService, apiKey));
    }

    private static void ConsoleCtrlCheck(object sender, EventArgs e, LogService logService)
    {
        logService.Dispose();
        var res = DBContext.DB.Commit();
        DBContext.DB.Checkpoint();
        DBContext.DB.Dispose();
        Console.WriteLine(res);
    }

    public static IHostBuilder ConfigureHost(IHostBuilder host) => host
        .ConfigureAppConfiguration(builder =>
        {
            builder.AddEnvironmentVariables();
            builder.AddJsonFile("appsettings.json");
            var config = builder.Build();
            var apiKey = config.GetValue("VKTeamsApiKey", string.Empty);
        })
        .ConfigureServices((context, services) =>
        {
            var configuration = context.Configuration;
        });
}