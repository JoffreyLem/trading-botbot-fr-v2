using RobotAppLibraryV2.Modeles;
using RobotAppLibraryV2.Utils;

namespace RobotAppLibraryV2.BackTest;

public class CandleHelper
{
    private static readonly Random random = new();

    // For backtest purpose only
    public static List<Tick> DecomposeCandlestick(Candle candle, Timeframe timeframe, decimal askBidSpread,
        SymbolInfo symbolInfo)
    {
        var symbol = symbolInfo.Symbol;
        var ticks = new List<Tick>();
        var timeframeminutes = timeframe.GetMinuteFromTimeframe();
        var endTime = candle.Date.AddMinutes(timeframeminutes).AddSeconds(-1);
        var quarterDuration = new TimeSpan((endTime - candle.Date).Ticks / 4);

        var tickSize = symbolInfo.Symbol.Contains("JPY") ? 0.01m : 0.0001m;

        // Calculer le spread en fonction du type de symbole
        var spread = symbolInfo.Category == Category.Forex ? askBidSpread * tickSize : askBidSpread;

        ticks.Add(new Tick { Date = candle.Date, Bid = candle.Open, Ask = candle.Open + spread, Symbol = symbol });
        ticks.Add(new Tick
        {
            Date = candle.Date.AddTicks(quarterDuration.Ticks), Bid = candle.High, Ask = candle.High + spread,
            Symbol = symbol
        });
        ticks.Add(new Tick
        {
            Date = candle.Date.AddTicks(quarterDuration.Ticks * 2), Bid = candle.Low, Ask = candle.Low + spread,
            Symbol = symbol
        });
        ticks.Add(new Tick { Date = endTime, Bid = candle.Close, Ask = candle.Close + spread, Symbol = symbol });

        return ticks;
    }
}