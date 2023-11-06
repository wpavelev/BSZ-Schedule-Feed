using Azure.Data.Tables;
using BszScheduleFeed.Model;
using BszScheduleFeed.Service.Model;
using BszScheduleFeed.Configuration;

using JsonSerializer = System.Text.Json.JsonSerializer;
using static BszScheduleFeed.Configuration.ServiceConfiguration;
using Microsoft.Extensions.Options;

namespace BszScheduleFeed.Service;

public class TableStorageService : ITableStorageService
{
    private TableClient tableClient;

    public TableStorageService(IOptions<TableServiceConfiguration> options)
    {
#if DEBUG
        tableClient = new TableClient(
            options.Value.ConnectionString,
            options.Value.DebugTableName);
#else
        tableClient = new TableClient(
            options.Value.ConnectionString, 
            options.Value.TableName);
#endif
    }

    public List<Schedule> GetAsync()
    {
        var partitionKey = DateTime.Now.Year.ToString();
        var result = tableClient.Query<ScheduleTable>(x => x.PartitionKey.Equals(partitionKey)).OrderByDescending(x => x.Timestamp).FirstOrDefault();

        if (result != default)
        {
#if DEBUG
            var json = JsonSerializer.Deserialize<List<Schedule>>(result.Data);
            Console.WriteLine($"result.Data:\n Partitionkey: {result.PartitionKey}, Guid: {result.RowKey}, Elements: {json.Count}");
#endif
            return JsonSerializer.Deserialize<List<Schedule>>(result.Data);
        }
        return new List<Schedule>();
    }

    public async Task PutAsync(string scheduleJson)
    {
        var id = Guid.NewGuid().ToString();
        var partition = DateTime.Now.Year.ToString();

        await tableClient.AddEntityAsync(new ScheduleTable
        {
            Data = scheduleJson,
            PartitionKey = partition,
            RowKey = id,
        });
    }
}