using Azure;
using Azure.Data.Tables;
using System;

namespace BszScheduleFeed.Service.Model;

internal class ScheduleTable : ITableEntity
{
    public string PartitionKey { get; set; }
    public string RowKey { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
    public string Data { get; set; }
}