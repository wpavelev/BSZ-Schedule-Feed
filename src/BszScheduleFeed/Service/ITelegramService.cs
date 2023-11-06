namespace BszScheduleFeed.Service
{
    public interface ITelegramService
    {
        Task<string> SendMessageAsync(string message);

    }
}