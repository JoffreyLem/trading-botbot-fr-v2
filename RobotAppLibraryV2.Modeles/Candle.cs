using Skender.Stock.Indicators;

namespace RobotAppLibraryV2.Modeles;

public class Candle : CandleProperties
{
    public List<Tick> Ticks { get; set; } = new();
    public decimal BidVolume { get; set; }
    public decimal AskVolume { get; set; }

    public Candle SetOpen(decimal open)
    {
        Open = open;
        return this;
    }

    public Candle SetHigh(decimal high)
    {
        High = high;
        return this;
    }

    public Candle SetLow(decimal low)
    {
        Low = low;
        return this;
    }

    public Candle SetClose(decimal close)
    {
        Close = close;
        return this;
    }

    public Candle SetDate(DateTime date)
    {
        Date = date;
        return this;
    }

    public Candle SetVolume(decimal volume)
    {
        Volume = volume;
        return this;
    }

    public Candle SetAskVolume(decimal askVolume)
    {
        AskVolume = askVolume;
        return this;
    }

    public Candle SetBidVolume(decimal bidVolume)
    {
        BidVolume = bidVolume;
        return this;
    }

    public override string ToString()
    {
        return $"Date:{Date} " + $"Open:{Open} " + $"High:{High} " + $"Low:{Low} " +
               $"Close:{Close} Volume:{Volume} BidVolume:{BidVolume} AskVolume:{AskVolume} ";
    }
}