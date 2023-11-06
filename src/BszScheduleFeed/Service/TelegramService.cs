using Azure.Data.Tables;
using BszScheduleFeed.Configuration;
using Microsoft.Extensions.Options;
using System.Reflection.Metadata.Ecma335;
using Telegram.Bot;
using static BszScheduleFeed.Configuration.ServiceConfiguration;

namespace BszScheduleFeed.Service;
public class TelegramService : ITelegramService
{
    private TelegramBotClient client;
    private string debugChatId;
    private string channelId;
    private string chatId;

    public TelegramService(IOptions<TelegramServiceConfiguration> options)
    {
        client = new TelegramBotClient(options.Value.Token);
        debugChatId = options.Value.DebugChatId;
        channelId = options.Value.ChannelId;

#if DEBUG
        chatId = debugChatId;
#else
        chatId = channelId;
       
#endif
    }

    public async Task<string> SendMessageAsync(string message)
    {
        if (message is null)
        {
            return String.Empty;
        }
#if DEBUG
        chatId = debugChatId;
#endif

        return (await client.SendTextMessageAsync(chatId, message)).Text ?? String.Empty;
    }
}