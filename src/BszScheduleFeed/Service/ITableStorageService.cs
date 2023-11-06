using BszScheduleFeed.Model;

namespace BszScheduleFeed.Service
{
    public interface ITableStorageService
    {
        List<Schedule> GetAsync();
        Task PutAsync(string scheduleJson);
    }
}