using BszScheduleFeed.Model;
using Org.BouncyCastle.Asn1.X509;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace BszScheduleFeed.Util
{
    public static class ScheduleExtensions
    {
        public static string ToJson(this IList<Schedule> schedules)
        {
            var options = new JsonSerializerOptions()
            {
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
                WriteIndented = true
            };

            return JsonSerializer.Serialize(schedules, options);
        }
            
        public static string ToReport(this IList<Schedule> schedules)
        {
            string output = "";
            var schoolClasses = schedules.GroupBy(schedule => schedule.Class);

            foreach (var schoolClass in schoolClasses)
            {
                output += schoolClass.Key + " - ";

                var dates = schoolClass.GroupBy(schedule => schedule.Date);

                foreach (var date in dates)
                {
                    output += date.Key + ", " + date.First().DayOfWeek + "\n";
                    foreach (var groupItem in date)
                    {
                        var room = groupItem.RoomDefault;
                        if (!String.IsNullOrEmpty(groupItem.RoomNew))
                        {
                            room = $"{groupItem.RoomNew} statt {groupItem.RoomDefault}";
                        }

                        var teacher = groupItem.TeacherDefault;
                        if (!String.IsNullOrEmpty (groupItem.TeacherNew))
                        {
                            teacher = $"{groupItem.TeacherNew} statt {groupItem.TeacherDefault}";
                        }
                        string scheduleString = $"{groupItem.Pos} {groupItem.Subject} {groupItem.Message} ({room}, {teacher})";
                        output += scheduleString + "\n";
                    }
                }
               
                output += "\n";
            }
            return output;
        }

        public static bool EqualsToList(this IList<Schedule> latestScheduleList, IList<Schedule> savedScheduleList)
        {
            if (latestScheduleList is null || savedScheduleList is null)
            {
                return false;
            }
            if (latestScheduleList.Count != savedScheduleList.Count)
            {
                return false;
            }
            else
            {
                for (int i = 0; i < latestScheduleList.Count; i++)
                {
                    Schedule schedule1 = latestScheduleList[i];
                    Schedule schedule2 = savedScheduleList[i];
                    var AreSchedulesSame = schedule1.Equals(schedule2);
                    if (!AreSchedulesSame)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

    }
}



