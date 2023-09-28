using System.ComponentModel;

#pragma warning disable CS8618

namespace RobotAppLibraryV2.Modeles;

public class Position
{
    public string Id { get; set; }

    public string StrategyId { get; set; }
    public string Symbol { get; set; }

    public TypePosition TypePosition { get; set; }

    public double? Spread { get; set; }
    public decimal Profit { get; set; }
    public decimal OpenPrice { get; set; }
    public DateTime DateOpen { get; set; }
    public decimal ClosePrice { get; set; }
    public DateTime DateClose { get; set; }
    public string? ReasonClosed { get; set; }
    public decimal? StopLoss { get; set; }
    public decimal? TakeProfit { get; set; }
    public double Volume { get; set; }

    public decimal Pips => ClosePrice != 0 ? Math.Abs(OpenPrice - ClosePrice) : 0;
    public StatusPosition StatusPosition { get; set; }
    public string? Comment { get; set; }


    public Position SetId(string id)
    {
        Id = id;
        return this;
    }

    public Position SetSymbol(string symbol)
    {
        Symbol = symbol;
        return this;
    }

    public Position SetTypePosition(TypePosition typePosition)
    {
        TypePosition = typePosition;
        return this;
    }

    public Position SetSpread(double? spread)
    {
        Spread = spread;
        return this;
    }

    public Position SetDateOpen(DateTime dateTime)
    {
        DateOpen = dateTime;
        return this;
    }


    public Position SetProfit(decimal profit)
    {
        Profit = profit;
        return this;
    }

    public Position SetOpenPrice(decimal openPrice)
    {
        OpenPrice = openPrice;
        return this;
    }

    public Position SetClosePrice(decimal closePrice)
    {
        ClosePrice = closePrice;
        return this;
    }

    public Position SetDateClose(DateTime dateClose)
    {
        DateClose = dateClose;
        return this;
    }

    public Position SetReasonClosed(string? reasonClosed)
    {
        ReasonClosed = reasonClosed;
        return this;
    }

    public Position SetStopLoss(decimal? stopLoss)
    {
        StopLoss = stopLoss;
        return this;
    }

    public Position SetTakeProfit(decimal? takeProfit)
    {
        TakeProfit = takeProfit;
        return this;
    }

    public Position SetVolume(double volume)
    {
        Volume = volume;
        return this;
    }

    public Position SetStatusPosition(StatusPosition statusPosition)
    {
        StatusPosition = statusPosition;
        return this;
    }

    public Position SetComment(string comment)
    {
        Comment = comment;
        return this;
    }

    public Position SetStrategyId(string strategyId)
    {
        StrategyId = strategyId;
        return this;
    }

    public Position Clone()
    {
        return new Position
        {
            Id = Id,
            StrategyId = StrategyId,
            TypePosition = TypePosition,
            Spread = Spread,
            Profit = Profit,
            OpenPrice = OpenPrice,
            DateOpen = DateOpen,
            ClosePrice = ClosePrice,
            DateClose = DateClose,
            ReasonClosed = ReasonClosed,
            StopLoss = StopLoss,
            TakeProfit = TakeProfit,
            Volume = Volume,
            StatusPosition = StatusPosition,
            Comment = Comment,
            CustomComment = CustomComment,
            Symbol = Symbol
        };
    }


    #region XTB

    public long? Order { get; set; }
    public long? Order2 { get; set; }
    public long? PositionId { get; set; }
    public string CustomComment { get; set; }

    public Position SetOrder(long? order)
    {
        Order = order;
        return this;
    }

    public Position SetOrder2(long? order2)
    {
        Order2 = order2;
        return this;
    }

    public Position SetPositionId(long? positionId)
    {
        PositionId = positionId;
        return this;
    }

    public Position SetCustomComment(string customComment)
    {
        CustomComment = customComment;
        return this;
    }

    #endregion
}

public enum ReasonClosed
{
    Sl,
    Tp,
    Margin,
    Closed
}

public enum StatusPosition
{
    Open,
    Pending,
    Close,
    WaitClose,
    Rejected
}

public enum TypePosition
{
    [Description("Buy")] Buy = 0,
    [Description("Sell")] Sell = 1,
    BuyLimit = 2,
    SellLimit = 3,
    BuyStop = 4,
    SellStop = 5,
    Balance = 6,
    Credit = 7,
    [Description("None")] None = 8
}