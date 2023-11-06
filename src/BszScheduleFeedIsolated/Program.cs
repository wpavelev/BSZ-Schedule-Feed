using BszScheduleFeed.Configuration;
using BszScheduleFeed.Service;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using static BszScheduleFeed.Configuration.ServiceConfiguration;

IConfiguration? configuration = default;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureAppConfiguration((context, config) =>
    {
        config.AddJsonFile("appsettings.json");
        configuration = config.Build();
    })
    .ConfigureServices(service =>
    {
        if (configuration is not null)
        {
            service.Configure<TelegramServiceConfiguration>(configuration.GetSection("Telegram"));
            service.Configure<ScheduleServiceConfiguraiton>(configuration.GetSection("Schedule"));
            service.Configure<TableServiceConfiguration>(configuration.GetSection("TableStorage"));
        }

        service.AddHttpClient();
        service.AddLogging();
        service.AddTransient<IScheduleService, ScheduleService>();
        service.AddTransient<ITelegramService, TelegramService>();
        service.AddTransient<ITableStorageService, TableStorageService>();
        
    }).Build();

host.Run();