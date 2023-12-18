namespace RobotAppLibraryV2.Modeles;

public struct Tick
{
    public Tick()
    {
        Ask = 0;
        Bid = 0;
        Date = new DateTime();
        Symbol = "";
        AskVolume = 0;
        BidVolume = 0;
    }


    public Tick(decimal? ask, decimal? bid, DateTime date, string symbol)
    {
        Ask = ask;
        Bid = bid;
        Date = date;
        Symbol = symbol;
        AskVolume = 0;
        BidVolume = 0;
    }


    public decimal? Ask { get; set; }
    public decimal? AskVolume { get; set; }
    public decimal? Bid { get; set; }
    public decimal? BidVolume { get; set; }
    public DateTime Date { get; set; }
    public decimal? Spread => Ask - Bid;
    public string Symbol { get; set; }

    public Tick SetAsk(decimal? ask)
    {
        Ask = ask;
        return this;
    }

    public Tick SetBid(decimal? bid)
    {
        Bid = bid;
        return this;
    }

    public Tick SetSymbol(string symbol)
    {
        Symbol = symbol;
        return this;
    }

    public Tick SetDate(DateTime date)
    {
        Date = date;
        return this;
    }

    public Tick SetBidVolume(decimal? bidVolume)
    {
        BidVolume = bidVolume;
        return this;
    }

    public Tick SetAskVolume(decimal? askVolume)
    {
        AskVolume = askVolume;
        return this;
    }


    public override string ToString()
    {
        return
            $"Ask : {Ask}  Bid : {Bid}  Date : {Date}  AskVolume : {AskVolume}  BidVolume : {BidVolume}  Spread : {Spread}";
    }
}