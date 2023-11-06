using BszScheduleFeed.Service;
using BszScheduleFeed.Util;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;

namespace BszScheduleFeedIsolated
{
    public class ScheduleRoutineRepetitive
    {
#if DEBUG
        private const string CRON = "*/5 * * * * *";
#else
        private const string CRON = "0 */5 * * * *";
#endif
        private readonly ILogger logger;
        private readonly IScheduleService scheduleService;
        private readonly ITelegramService telegramService;
        private readonly ITableStorageService storageService;

        public ScheduleRoutineRepetitive(ILoggerFactory loggerFactory, ITableStorageService storageService, IScheduleService scheduleService, ITelegramService telegramService)
        {
            logger = loggerFactory.CreateLogger<ScheduleRoutineRepetitive>();
            this.storageService = storageService;
            this.scheduleService = scheduleService;
            this.telegramService = telegramService;
        }

        [Function("ScheduleRoutineRepetitive")]
        public async Task RunAsync([TimerTrigger(CRON)] MyInfo myTimer)
        {
            logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            logger.LogInformation("Get LatestScheduleList with ScheduleService");
            var latestScheduleList = await scheduleService.GetScheduleListAsync();

            logger.LogInformation("Get SavedSchedulelist with storageService");
            var savedScheduleList = storageService.GetAsync();

            logger.LogInformation("Compare Lists");
            if (latestScheduleList.EqualsToList(savedScheduleList))
            {
                logger.LogInformation("No changes in Schedule");
            }
            else
            {
                logger.LogInformation("Change detected - end new Schedule Message");
                await storageService.PutAsync(latestScheduleList.ToJson());
                await telegramService.SendMessageAsync(latestScheduleList.ToReport());
            }
#if DEBUG
            logger.LogInformation($"[DEBUG] send Message: {latestScheduleList.ToJson()}");
            await telegramService.SendMessageAsync(latestScheduleList.ToReport());
#endif
        }
    }

    public class MyInfo
    {
        public MyScheduleStatus? ScheduleStatus { get; set; }

        public bool IsPastDue { get; set; }
    }

    public class MyScheduleStatus
    {
        public DateTime Last { get; set; }

        public DateTime Next { get; set; }

        public DateTime LastUpdated { get; set; }
    }
}
