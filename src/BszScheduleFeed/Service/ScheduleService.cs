using BszScheduleFeed.Model;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using static BszScheduleFeed.Configuration.ServiceConfiguration;

namespace BszScheduleFeed.Service;

public class ScheduleService : IScheduleService
{
    private readonly string[] lineToSkip = { "A_", "B_", "C_", "Mo", "Di", "Mi", "Do", "Fr", "VLehrer" };
    private readonly ScheduleServiceConfiguraiton options;

    public ScheduleService(IOptions<ScheduleServiceConfiguraiton> options)
    {
        this.options = options.Value;
    }

    public async Task<List<Schedule>> GetScheduleListAsync()
    {
        return GetScheduleListFromPdf(await DownloadPlan())!;
    }

    private async Task<Stream> DownloadPlan()
    {
        using HttpClient httpClient = new HttpClient();
        string credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{options.Username}:{options.Password}"));
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);

        HttpResponseMessage response = await httpClient.GetAsync(options.URL);

        if (response.IsSuccessStatusCode)
        {
            Stream stream = await response.Content.ReadAsStreamAsync();
            return stream;
        }
        else
        {
            return Stream.Null;
        }
    }

    private List<Schedule>? GetScheduleListFromPdf(Stream stream)
    {
        if (stream == Stream.Null)
        {
            return new List<Schedule>();
        }
        PdfDocument pdfDoc = new PdfDocument(new PdfReader(stream));
        List<Schedule> scheduleList = new();
        for (int i = 1; i <= pdfDoc.GetNumberOfPages(); i++)
        {
            PdfPage page;
            try
            {
                page = pdfDoc.GetPage(i);
            }
            catch (Exception m)
            {
                throw new Exception(m.Message);
            }

            scheduleList.AddRange(readPdfPage(page));
        }
        pdfDoc.Close();
        if (scheduleList == null) return new List<Schedule>();

        return scheduleList;
    }

    private List<Schedule> readPdfPage(PdfPage page)
    {
        var strategy = new SimpleTextExtractionStrategy();
        string text = PdfTextExtractor.GetTextFromPage(page, strategy);
        List<Schedule> scheduleList = new();

        var textLines = text.Split("\n");
        Array.Reverse(textLines);
        string lastDate = String.Empty;
        string lastDayofWeek = String.Empty;

        for (int i = 0; i < textLines.Length; i++)
        {
            Schedule schedule = new();

            var textLine = textLines[i];
            if (lineToSkip.Any(x => textLine.StartsWith(x)))
            {
                continue;
            }

            var lineTabs = textLine.Split("...");
            Array.Reverse(lineTabs);
            string lastLineTab = "";

            for (int j = 0; j < lineTabs.Length; j++)
            {
                var lineTab = lineTabs[j].Trim().Trim('.').Trim();

                if (string.IsNullOrWhiteSpace(lineTab) || lineTab.Equals(lastLineTab))
                {
                    continue;
                }

                try
                {
                    schedule = TextToScheduleInformation(schedule, lineTab);

                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }

                if (!String.IsNullOrEmpty(schedule.Date))
                {
                    lastDate = schedule.Date;
                    lastDayofWeek = schedule.DayOfWeek;
                }
                lastLineTab = lineTab;
            }

            if (String.IsNullOrEmpty(schedule.Date))
            {
                schedule.Date = lastDate;
                schedule.DayOfWeek = lastDayofWeek;
            }
            scheduleList.Add(schedule);
        }
        return scheduleList;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="schedule"></param>
    /// <param name="lineTab"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    private Schedule TextToScheduleInformation(Schedule schedule, string lineTab)
    {

        return lineTab switch
        {
            var tabDate when IsDate(tabDate) => schedule with { Date = tabDate },

            var tabDayOfWeek when IsDayOfWeek(tabDayOfWeek) => schedule with { DayOfWeek = tabDayOfWeek },

            var tabPos when IsPos(tabPos) => schedule with { Pos = tabPos },

            var tabClass when IsClass(tabClass) => schedule with { Class = tabClass },

            var tabRoom when IsRoom(tabRoom) =>
                (!tabRoom.Contains('+')) ?
                    schedule with { RoomDefault = tabRoom } :
                    schedule with { RoomDefault = tabRoom.Substring(tabRoom.IndexOf("(") + 1, 4), RoomNew = tabRoom.Substring(tabRoom.IndexOf("+") + 1, 4) },

            var tabSubject when IsSubject(tabSubject) => schedule with { Subject = tabSubject },

            var tabMessage when IsMessage(tabMessage) => schedule with { Message = tabMessage },

            var tabTeacher when IsTeacher(tabTeacher) =>
                (!tabTeacher.Contains('+')) ?
                    schedule with
                    {
                        TeacherDefault = tabTeacher[1..^1],
                        TeacherNew = null
                    } :
                    schedule with
                    {
                        TeacherDefault = tabTeacher.Substring(tabTeacher.IndexOf("(") + 1, tabTeacher.LastIndexOf(")") - tabTeacher.IndexOf("(") - 1),
                        TeacherNew = tabTeacher.Substring(tabTeacher.IndexOf("+") + 1, tabTeacher.IndexOf("(") - 1 - tabTeacher.IndexOf("+"))
                    },

            _ => throw new ArgumentException($"Invalid input value: {lineTab}", nameof(lineTab))
        };

    }
    private bool IsTeacher(string lineTab)
    {
        Regex teacherRegex = new Regex(@"(^\(.*\)$)|(\+(.*) )");
        var result = teacherRegex.IsMatch(lineTab);
        return result;

    }
    private bool IsDate(string lineTab)
    {
        Regex dateRegex = new Regex("[0-9]{1,2}\\.[0-9]{1,2}\\.[0-9]{4}$");
        return dateRegex.IsMatch(lineTab);
    }

    private bool IsDayOfWeek(string lineTab)
    {
        Regex dayOfWeekRegex = new Regex("^(Mo|Di|Mi|Do|Fr)");
        return dayOfWeekRegex.IsMatch(lineTab);
    }

    private bool IsClass(string lineTab)
    {
        Regex classRegex = new Regex("(A_|B_|C_)[A-Z]{2} [0-9][0-9]\\/[0-9]");
        return classRegex.IsMatch(lineTab);
    }

    private bool IsRoom(string lineTab)
    {
        Regex roomRegex = new Regex("[A-Z][0-9]{3}");
        return roomRegex.IsMatch(lineTab);
    }

    private bool IsPos(string lineTab)
    {
        Regex posRegex = new Regex("^[0-9]");
        return posRegex.IsMatch(lineTab);
    }

    private bool IsSubject(string lineTab)
    {
        string[] subjects = { "DEU", "EN", "SP", "WI", "IT-LF", "GK", "BK", "FP", "FBP" };
        return subjects.Any(x => lineTab.StartsWith(x, StringComparison.InvariantCulture));
    }

    private bool IsMessage(string lineTab)
    {
        Regex MessageRegex = new Regex("^(statt |fällt aus|Aufgaben|Raumänderung|ganze Klasse)");
        return MessageRegex.IsMatch(lineTab);
    }
}