using Skender.Stock.Indicators;

namespace RobotAppLibraryV2.Indicators.Custom.CandleCode;

public static class CandleCode
{
    public static IEnumerable<CandleCodeResult> GetCandleCode<TQuote>(this IEnumerable<TQuote> quotes)
        where TQuote : CandleProperties
    {
        var quotesList = quotes.ToSortedCollection();

        var results = new List<CandleCodeResult>(quotesList.Count);

        for (var i = quotesList.Count - 1; i >= 0; i--)
        {
            var quote = quotesList[i];

            var colorCode = ColorCode(quote);
            var topMecheCode = TopMecheCode(quote);
            var botMecheCode = BotMecheCode(quote);
            var bodyCode = BodyCode(quote);

            var result = string.Join(".", colorCode, topMecheCode, botMecheCode, bodyCode);

            results.Add(new CandleCodeResult
            {
                Code = result,
                Date = quote.Date
            });
        }

        return results;
    }

    public static string ColorCode(CandleProperties candle)
    {
        if (candle.IsBullish) return CandleCodeTable.BUY;

        return CandleCodeTable.SELL;
    }

    public static string TopMecheCode(CandleProperties candle)
    {
        if (candle.UpperWick > candle.LowerWick) return CandleCodeTable.MTSMB;

        if (candle.UpperWick == candle.LowerWick) return CandleCodeTable.MTEMB;

        return CandleCodeTable.MTIMB;
    }


    public static string BotMecheCode(CandleProperties candle)
    {
        if (candle.LowerWick > candle.UpperWick) return CandleCodeTable.MBSMT;

        if (candle.LowerWick == candle.UpperWick) return CandleCodeTable.MBEMT;

        return CandleCodeTable.MBIMT;
    }

    public static string BodyCode(CandleProperties candle)
    {
        if (candle.Body == 0) return CandleCodeTable.BN;

        var v1 = BodyTopMecheCode(candle);
        var v2 = BodyBotMecheCode(candle);

        return string.Join(".", v1, v2);
    }

    public static string BodyTopMecheCode(CandleProperties candle)
    {
        if (candle.Body > candle.UpperWick) return CandleCodeTable.BSMT;

        if (candle.Body == candle.UpperWick) return CandleCodeTable.BEMT;

        return CandleCodeTable.BIMT;
    }

    public static string BodyBotMecheCode(CandleProperties candle)
    {
        if (candle.Body > candle.LowerWick) return CandleCodeTable.BSMB;

        if (candle.Body == candle.LowerWick) return CandleCodeTable.BEMB;

        return CandleCodeTable.BIMB;
    }
}