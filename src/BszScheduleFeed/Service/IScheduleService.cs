using BszScheduleFeed.Model;

namespace BszScheduleFeed.Service
{
    public interface IScheduleService
    {
        Task<List<Schedule>> GetScheduleListAsync();
    }
}