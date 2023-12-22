using System.Reflection;
using RobotAppLibraryV2.Modeles;
using RobotAppLibraryV2.Utils;

namespace RobotAppLibraryV2.Tests;

public static class TestUtils
{
    public static string FileReadContent(string ressourcePath, string sampleFile)
    {
        var asm = Assembly.GetExecutingAssembly();
        var ressource = $"{ressourcePath}.{sampleFile}";
        using var stream = asm.GetManifestResourceStream(ressource);
        if (stream == null) return string.Empty;

        var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    public static List<Candle> GenerateCandle(Timeframe timeframe, int nombre = 1000, DateTime? start = null)
    {
        var interval = TimeSpan.FromMinutes(timeframe.GetMinuteFromTimeframe());
        var random = new Random();
        var candles = new List<Candle>();
        var dateDebut = start ?? new DateTime(2021, 1, 1, 0, 0, 0);

        if (timeframe == Timeframe.Weekly)
        {
            int daysUntilMonday = (int)DayOfWeek.Monday - (int)dateDebut.DayOfWeek;
            if (daysUntilMonday > 0)
            {
      
                daysUntilMonday -= 7;
            }
            
            dateDebut = dateDebut.AddDays(daysUntilMonday);
        }

        if (timeframe == Timeframe.Daily || timeframe < Timeframe.Daily)
        {
            dateDebut = dateDebut.AddDays(1);
        }
        if (timeframe < Timeframe.FourHour)
        {
            dateDebut = dateDebut.AddHours(1);
        }

        for (var i = 0; i < nombre; i++)
        {
            var open = (decimal)random.NextDouble() * 100;
            var close = (decimal)random.NextDouble() * 100;
            var candle = new Candle()
                .SetOpen(open)
                .SetClose(close)
                .SetHigh(Math.Max(open, close) + (decimal)(random.NextDouble() * 100))
                .SetLow(Math.Max(open, close) + (decimal)(random.NextDouble() * 100))
                .SetDate(dateDebut);

            candles.Add(candle);

            do
            {
                if (start is not null)
                    dateDebut += interval;
                else
                    dateDebut -= interval;
            }
            while (dateDebut.DayOfWeek == DayOfWeek.Saturday || dateDebut.DayOfWeek == DayOfWeek.Sunday);
        }

        return candles.Distinct().OrderBy(candle => candle.Date).ToList();
    }

}