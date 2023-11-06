
namespace BszScheduleFeed.Model;

public record Schedule
{
    public string? DayOfWeek { get; set; }
    public string? Date { get; set; }
    public string? Class { get; set; }
    public string? RoomDefault { get; set; }
    public string? RoomNew { get; set; }
    public string? Pos { get; set; }
    public string? Subject { get; set; }
    public string? TeacherDefault { get; set; }
    public string? TeacherNew { get; set; }
    public string? Message { get; set; }
}