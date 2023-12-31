using FluentAssertions;
using Moq;
using RobotAppLibraryV2.ApiHandler;
using RobotAppLibraryV2.CandleList;
using RobotAppLibraryV2.Modeles;
using RobotAppLibraryV2.Utils;
using Serilog;

namespace RobotAppLibraryV2.Tests.Candlelist;

public class CandleListTest
{
    private readonly Mock<IApiHandler> _apiHandlerMock = new();
    private readonly Mock<ILogger> _loggerMock = new();

    public CandleListTest()
    {
        _loggerMock.Setup(x => x.ForContext<CandleList.CandleList>())
            .Returns(_loggerMock.Object);
    }


    public static TradeHourRecord GetTradeHoursMock()
    {
        var tradeHoursRecord = new TradeHourRecord();
        foreach (var day in Enum.GetValues(typeof(DayOfWeek)))
            tradeHoursRecord.HoursRecords.Add(new TradeHourRecord.HoursRecordData
            {
                Day = (DayOfWeek)day,
                From = DateTime.UtcNow.Date.TimeOfDay,
                To = DateTime.UtcNow.Date.AddHours(23).AddMinutes(59).AddSeconds(59).TimeOfDay
            });

        return tradeHoursRecord;
    }

    #region Trading Hours tests in

    [Fact]
    public void Test_CurrentHoursRecord_Boucle()
    {
        // Arrange

        _apiHandlerMock.Setup(x => x.GetChartAsync(It.IsAny<string>(), It.IsAny<Timeframe>()))
            .ReturnsAsync(TestUtils.GenerateCandle(Timeframe.FifteenMinutes, 100));
        _apiHandlerMock.Setup(x => x.GetTradingHoursAsync(It.IsAny<string>()))
            .ReturnsAsync(GetTradeHoursMock);
        // Act
        var candleList = new CandleList.CandleList(_apiHandlerMock.Object, _loggerMock.Object, Timeframe.FifteenMinutes,
            "EURUSD");

        // Assert
        candleList.Count.Should().Be(100);
    }

    #endregion

    #region Init

    [Fact]
    public void Test_Init()
    {
        // Arrange

        _apiHandlerMock.Setup(x => x.GetChartAsync(It.IsAny<string>(), It.IsAny<Timeframe>()))
            .ReturnsAsync(TestUtils.GenerateCandle(Timeframe.FifteenMinutes, 100));
        _apiHandlerMock.Setup(x => x.GetTradingHoursAsync(It.IsAny<string>()))
            .ReturnsAsync(GetTradeHoursMock);
        // Act
        var candleList = new CandleList.CandleList(_apiHandlerMock.Object, _loggerMock.Object, Timeframe.FifteenMinutes,
            "EURUSD");

        // Assert

        candleList.Count.Should().Be(100);
    }


    [Fact]
    public void Test_Init_throw_exception()
    {
        // Arrange

        _apiHandlerMock.Setup(x => x.GetChartAsync(It.IsAny<string>(), It.IsAny<Timeframe>()))
            .ThrowsAsync(new Exception());

        // Act && assert
        var candleList = () => new CandleList.CandleList(_apiHandlerMock.Object, _loggerMock.Object,
            Timeframe.FifteenMinutes,
            "EURUSD");

        // Assert
        candleList.Should().Throw<CandleListException>();
    }


    [Theory]
    [InlineData(Timeframe.Monthly)]
    [InlineData(Timeframe.Weekly)]
    public void Test_Init_throw_exception_bad_timeframe(Timeframe timeframe)
    {
        // Arrange

        _apiHandlerMock.Setup(x => x.GetChartAsync(It.IsAny<string>(), It.IsAny<Timeframe>()))
            .ThrowsAsync(new Exception());

        // Act && assert
        var candleList = () => new CandleList.CandleList(_apiHandlerMock.Object, _loggerMock.Object,
            timeframe,
            "EURUSD");

        // Assert
        candleList.Should().Throw<CandleListException>();
    }

    #endregion

    #region OnTickEvent

    [Theory]
    [InlineData(Timeframe.OneMinute)]
    [InlineData(Timeframe.FiveMinutes)]
    [InlineData(Timeframe.FifteenMinutes)]
    [InlineData(Timeframe.ThirtyMinutes)]
    [InlineData(Timeframe.OneHour)]
    [InlineData(Timeframe.FourHour)]
    [InlineData(Timeframe.Daily)]
    [InlineData(Timeframe.Monthly)]
    [InlineData(Timeframe.Weekly)]
    public void Test_NewTick_UpdateLastCandle(Timeframe timeframe)
    {
        // Arrange
        var timeframeMinute = timeframe.GetMinuteFromTimeframe();
        var caller = false;
        var callerTick = false;
        _apiHandlerMock.Setup(x => x.GetChartAsync(It.IsAny<string>(), It.IsAny<Timeframe>()))
            .ReturnsAsync(TestUtils.GenerateCandle(timeframe, 100));
        _apiHandlerMock.Setup(x => x.GetTradingHoursAsync(It.IsAny<string>()))
            .ReturnsAsync(GetTradeHoursMock);
        var candleList = new CandleList.CandleList(_apiHandlerMock.Object, _loggerMock.Object, timeframe, "EURUSD");

        // Act

        var lastDate = candleList.Last().Date.AddMinutes(timeframeMinute).AddSeconds(-10);
        var tick = new Tick(1, 1, lastDate, "EURUSD")
            .SetAskVolume(1)
            .SetBidVolume(1);


        candleList.OnCandleEvent += candle =>
        {
            caller = true;
            return Task.CompletedTask;
        };
        candleList.OnTickEvent += tick1 =>
        {
            callerTick = true;
            return Task.CompletedTask;
        };

        _apiHandlerMock.Raise(x => x.TickEvent += null, this, tick);

        // Assert
        caller.Should().BeFalse();
        callerTick.Should().BeTrue();
        candleList.Last().Ticks.Count.Should().Be(1);
        candleList.Last().Close.Should().Be(1);
        candleList.Last().AskVolume.Should().Be(1);
        candleList.Last().BidVolume.Should().Be(1);
        candleList.Last().Volume.Should().Be(2);
    }

    [Theory]
    [InlineData(Timeframe.OneMinute)]
    [InlineData(Timeframe.FiveMinutes)]
    [InlineData(Timeframe.FifteenMinutes)]
    [InlineData(Timeframe.ThirtyMinutes)]
    [InlineData(Timeframe.OneHour)]
    [InlineData(Timeframe.Daily)]
    [InlineData(Timeframe.Monthly)]
    [InlineData(Timeframe.Weekly)]
    [InlineData(Timeframe.FourHour)]
    public void Test_NewTick_UpdateLastCandle_If_last_is_0(Timeframe timeframe)
    {
        // Arrange
        var timeframeMinute = timeframe.GetMinuteFromTimeframe();
        var caller = false;
        var callerTick = false;
        var candleListData = TestUtils.GenerateCandle(timeframe, 100);
        candleListData.Last()
            .SetOpen(0)
            .SetHigh(0)
            .SetLow(0)
            .SetClose(0);
        _apiHandlerMock.Setup(x => x.GetChartAsync(It.IsAny<string>(), It.IsAny<Timeframe>()))
            .ReturnsAsync(candleListData);
        _apiHandlerMock.Setup(x => x.GetTradingHoursAsync(It.IsAny<string>()))
            .ReturnsAsync(GetTradeHoursMock);
        var candleList = new CandleList.CandleList(_apiHandlerMock.Object, _loggerMock.Object, timeframe, "EURUSD");

        // Act

        var lastDate = candleList.Last().Date.AddMinutes(timeframeMinute).AddSeconds(-10);
        var tick = new Tick(1, 10, lastDate, "EURUSD")
            .SetAskVolume(1)
            .SetBidVolume(1);

        candleList.OnCandleEvent += candle =>
        {
            caller = true;
            return Task.CompletedTask;
        };
        candleList.OnTickEvent += tick1 =>
        {
            callerTick = true;
            return Task.CompletedTask;
        };
        _apiHandlerMock.Raise(x => x.TickEvent += null, this, tick);

        // Assert
        caller.Should().BeFalse();
        callerTick.Should().BeTrue();
        candleList.Last().Ticks.Count.Should().Be(1);
        candleList.Last().AskVolume.Should().Be(1);
        candleList.Last().BidVolume.Should().Be(1);
        candleList.Last().Volume.Should().Be(2);
        candleList.Last().Open.Should().Be(10);
        candleList.Last().High.Should().Be(10);
        candleList.Last().Low.Should().Be(10);
        candleList.Last().Close.Should().Be(10);
    }

    [Fact]
    public void Test_NewTick_NoTrigger_BadSymbol()
    {
        // Arrange
        var caller = false;
        var callerTick = false;
        _apiHandlerMock.Setup(x => x.GetChartAsync(It.IsAny<string>(), It.IsAny<Timeframe>()))
            .ReturnsAsync(TestUtils.GenerateCandle(Timeframe.FifteenMinutes, 100));
        _apiHandlerMock.Setup(x => x.GetTradingHoursAsync(It.IsAny<string>()))
            .ReturnsAsync(GetTradeHoursMock);
        var candleList = new CandleList.CandleList(_apiHandlerMock.Object, _loggerMock.Object, Timeframe.FifteenMinutes,
            "EURUSD");

        // Act

        var lastDate = candleList.Last().Date.AddMinutes(3);
        var tick = new Tick(1, 1, lastDate, "test")
            .SetAskVolume(1)
            .SetBidVolume(1);

        candleList.OnCandleEvent += candle =>
        {
            caller = true;
            return Task.CompletedTask;
        };
        candleList.OnTickEvent += tick1 =>
        {
            callerTick = true;
            return Task.CompletedTask;
        };

        _apiHandlerMock.Raise(x => x.TickEvent += null, this, tick);

        // Assert
        caller.Should().BeFalse();
        callerTick.Should().BeFalse();
    }


    [Theory]
    [InlineData(Timeframe.OneMinute)]
    [InlineData(Timeframe.FiveMinutes)]
    [InlineData(Timeframe.FifteenMinutes)]
    [InlineData(Timeframe.ThirtyMinutes)]
    [InlineData(Timeframe.OneHour)]
    [InlineData(Timeframe.FourHour)]
    [InlineData(Timeframe.Daily)]
    [InlineData(Timeframe.Monthly)]
    [InlineData(Timeframe.Weekly)]
    public void Test_NewTick_AddNewCandle(Timeframe timeframe)
    {
        // Arrange
        var timeframeMinute = timeframe.GetMinuteFromTimeframe();
        var caller = false;
        var callerTick = false;
        _apiHandlerMock.Setup(x => x.GetChartAsync(It.IsAny<string>(), It.IsAny<Timeframe>()))
            .ReturnsAsync(TestUtils.GenerateCandle(timeframe, 100));

        _apiHandlerMock.Setup(x => x.GetTradingHoursAsync(It.IsAny<string>()))
            .ReturnsAsync(GetTradeHoursMock);

        var candleList = new CandleList.CandleList(_apiHandlerMock.Object, _loggerMock.Object, timeframe, "EURUSD");

        // Act

        var lastDate = new DateTime();

        if (timeframe == Timeframe.Monthly)
            lastDate = candleList.Last().Date.AddMonths(1);
        else
            lastDate = candleList.Last().Date.AddMinutes(timeframeMinute);
        var tick = new Tick(1, 1, lastDate, "EURUSD")
            .SetAskVolume(1)
            .SetBidVolume(1);

        candleList.OnCandleEvent += candle =>
        {
            caller = true;
            return Task.CompletedTask;
        };
        candleList.OnTickEvent += tick1 =>
        {
            callerTick = true;
            return Task.CompletedTask;
        };

        _apiHandlerMock.Raise(x => x.TickEvent += null, this, tick);

        // Assert
        caller.Should().BeTrue();
        callerTick.Should().BeFalse();
        candleList.Count.Should().Be(101);
        candleList.Last().Ticks.Count.Should().Be(1);
        candleList.Last().Close.Should().Be(1);
        candleList.Last().AskVolume.Should().Be(1);
        candleList.Last().BidVolume.Should().Be(1);
        candleList.Last().Volume.Should().Be(2);
    }

    [Theory]
    [InlineData(Timeframe.OneMinute)]
    [InlineData(Timeframe.FiveMinutes)]
    [InlineData(Timeframe.FifteenMinutes)]
    [InlineData(Timeframe.ThirtyMinutes)]
    [InlineData(Timeframe.OneHour)]
    [InlineData(Timeframe.FourHour)]
    [InlineData(Timeframe.Daily)]
    [InlineData(Timeframe.Monthly)]
    [InlineData(Timeframe.Weekly)]
    public void Test_NewTick_AddNewCandle_at2000(Timeframe timeframe)
    {
        // Arrange
        var timeframeMinute = timeframe.GetMinuteFromTimeframe();
        var caller = false;
        var callerTick = false;
        _apiHandlerMock.Setup(x => x.GetChartAsync(It.IsAny<string>(), It.IsAny<Timeframe>()))
            .ReturnsAsync(TestUtils.GenerateCandle(timeframe, 2000));
        _apiHandlerMock.Setup(x => x.GetTradingHoursAsync(It.IsAny<string>()))
            .ReturnsAsync(GetTradeHoursMock);
        var candleList = new CandleList.CandleList(_apiHandlerMock.Object, _loggerMock.Object, timeframe, "EURUSD");

        // Act

        var lastDate = new DateTime();

        if (timeframe == Timeframe.Monthly)
            lastDate = candleList.Last().Date.AddMonths(1);
        else
            lastDate = candleList.Last().Date.AddMinutes(timeframeMinute);


        var tick = new Tick(1, 1, lastDate, "EURUSD")
            .SetAskVolume(1)
            .SetBidVolume(1);

        candleList.OnCandleEvent += candle =>
        {
            caller = true;
            return Task.CompletedTask;
        };
        candleList.OnTickEvent += tick1 =>
        {
            callerTick = true;
            return Task.CompletedTask;
        };

        _apiHandlerMock.Raise(x => x.TickEvent += null, this, tick);

        // Assert
        caller.Should().BeTrue();
        callerTick.Should().BeFalse();
        candleList.Count.Should().Be(2000);
        candleList.Last().Ticks.Count.Should().Be(1);
        candleList.Last().Close.Should().Be(1);
        candleList.Last().AskVolume.Should().Be(1);
        candleList.Last().BidVolume.Should().Be(1);
        candleList.Last().Volume.Should().Be(2);
    }


    [Theory]
    [InlineData(Timeframe.OneMinute)]
    [InlineData(Timeframe.FiveMinutes)]
    [InlineData(Timeframe.FifteenMinutes)]
    [InlineData(Timeframe.ThirtyMinutes)]
    [InlineData(Timeframe.OneHour)]
    [InlineData(Timeframe.FourHour)]
    [InlineData(Timeframe.Daily)]
    [InlineData(Timeframe.Monthly)]
    [InlineData(Timeframe.Weekly)]
    public void Test_NewTick_AddNewTick_count_is_0(Timeframe timeframe)
    {
        // Arrange
        var timeframeMinute = timeframe.GetMinuteFromTimeframe();
        var caller = false;
        var callerTick = false;
        _apiHandlerMock.Setup(x => x.GetChartAsync(It.IsAny<string>(), It.IsAny<Timeframe>()))
            .ReturnsAsync(new List<Candle>());
        _apiHandlerMock.Setup(x => x.GetTradingHoursAsync(It.IsAny<string>()))
            .ReturnsAsync(GetTradeHoursMock);
        var candleList = new CandleList.CandleList(_apiHandlerMock.Object, _loggerMock.Object, timeframe, "EURUSD");

        // Act

        var tick = new Tick(1, 1, new DateTime(), "EURUSD")
            .SetAskVolume(1)
            .SetBidVolume(1);

        candleList.OnCandleEvent += candle =>
        {
            caller = true;
            return Task.CompletedTask;
        };
        candleList.OnTickEvent += tick1 =>
        {
            callerTick = true;
            return Task.CompletedTask;
        };

        _apiHandlerMock.Raise(x => x.TickEvent += null, this, tick);

        // Assert
        caller.Should().BeTrue();
        callerTick.Should().BeFalse();
        candleList.Count.Should().Be(1);
    }


    [Theory]
    [InlineData(Timeframe.OneMinute)]
    [InlineData(Timeframe.FiveMinutes)]
    [InlineData(Timeframe.FifteenMinutes)]
    [InlineData(Timeframe.ThirtyMinutes)]
    [InlineData(Timeframe.OneHour)]
    [InlineData(Timeframe.FourHour)]
    [InlineData(Timeframe.Daily)]
    [InlineData(Timeframe.Monthly)]
    [InlineData(Timeframe.Weekly)]
    public void Test_NewTick_Tick_on_new_candle(Timeframe timeframe)
    {
        // Arrange
        var timeframeMinute = timeframe.GetMinuteFromTimeframe();
        var caller = false;
        var callerTick = false;
        _apiHandlerMock.Setup(x => x.GetChartAsync(It.IsAny<string>(), It.IsAny<Timeframe>()))
            .ReturnsAsync(TestUtils.GenerateCandle(timeframe, 100));
        _apiHandlerMock.Setup(x => x.GetTradingHoursAsync(It.IsAny<string>()))
            .ReturnsAsync(GetTradeHoursMock);
        var candleList = new CandleList.CandleList(_apiHandlerMock.Object, _loggerMock.Object, timeframe, "EURUSD");

        // Act

        var lastDate = new DateTime();

        if (timeframe == Timeframe.Monthly)
            lastDate = candleList.Last().Date.AddMonths(1);
        else
            lastDate = candleList.Last().Date.AddMinutes(timeframeMinute);
        var tick = new Tick(1, 1, lastDate, "EURUSD")
            .SetAskVolume(1)
            .SetBidVolume(1);

        candleList.OnCandleEvent += candle =>
        {
            caller = true;
            return Task.CompletedTask;
        };
        candleList.OnTickEvent += tick1 =>
        {
            callerTick = true;
            return Task.CompletedTask;
        };

        _apiHandlerMock.Raise(x => x.TickEvent += null, this, tick);

        // Assert
        caller.Should().BeTrue();
        callerTick.Should().BeFalse();
        candleList.Last().Date.Should().Be(lastDate);
        candleList.Last().Ticks.Last().Should().Be(tick);
    }

    #endregion
}