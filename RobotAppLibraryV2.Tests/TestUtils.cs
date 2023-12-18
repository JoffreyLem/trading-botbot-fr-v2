using System.Reflection;
using RobotAppLibraryV2.Modeles;

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

    public static List<Candle> GenerateCandle(TimeSpan interval, int nombre = 1000, DateTime? start = null)
    {
        var random = new Random();
        var candles = new List<Candle>();
        var now = DateTime.UtcNow;
        var dateDebut = start ?? new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0);

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

            if (start is not null)
                dateDebut += interval;
            else
                dateDebut -= interval;

            candles.Add(candle);
        }

        return candles.Distinct().OrderBy(candle => candle.Date).ToList();
    }
}