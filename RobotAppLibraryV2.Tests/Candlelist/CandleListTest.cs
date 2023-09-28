using FluentAssertions;
using Moq;
using RobotAppLibraryV2.ApiHandler.Interfaces;
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
                From = DateTime.Now.Date.TimeOfDay,
                To = DateTime.Now.Date.AddHours(23).AddMinutes(59).AddSeconds(59).TimeOfDay
            });

        return tradeHoursRecord;
    }

    #region Init

    [Fact]
    public void Test_Init()
    {
        // Arrange

        _apiHandlerMock.Setup(x => x.GetChartAsync(It.IsAny<string>(), It.IsAny<Timeframe>()))
            .ReturnsAsync(TestUtils.GenerateCandle(TimeSpan.FromMinutes(15), 100));
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
    public void Test_NewTick_UpdateLastCandle(Timeframe timeframe)
    {
        // Arrange
        var timeframeMinute = timeframe.GetMinuteFromTimeframe();
        var caller = false;
        var callerTick = false;
        _apiHandlerMock.Setup(x => x.GetChartAsync(It.IsAny<string>(), It.IsAny<Timeframe>()))
            .ReturnsAsync(TestUtils.GenerateCandle(TimeSpan.FromMinutes(timeframeMinute), 100));
        _apiHandlerMock.Setup(x => x.GetTradingHoursAsync(It.IsAny<string>()))
            .ReturnsAsync(GetTradeHoursMock);
        var candleList = new CandleList.CandleList(_apiHandlerMock.Object, _loggerMock.Object, timeframe, "EURUSD");

        // Act

        var lastDate = candleList.Last().Date.AddMinutes(timeframeMinute).AddSeconds(-10);
        var tick = new Tick(1, 1, lastDate, "EURUSD")
            .SetAskVolume(1)
            .SetBidVolume(1);


        candleList.OnCandleEvent += candle => caller = true;
        candleList.OnTickEvent += tick1 => callerTick = true;

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
    [InlineData(Timeframe.FourHour)]
    [InlineData(Timeframe.Daily)]
    public void Test_NewTick_UpdateLastCandle_If_last_is_0(Timeframe timeframe)
    {
        // Arrange
        var timeframeMinute = timeframe.GetMinuteFromTimeframe();
        var caller = false;
        var callerTick = false;
        var candleListData = TestUtils.GenerateCandle(TimeSpan.FromMinutes(timeframeMinute), 100);
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

        candleList.OnCandleEvent += candle => caller = true;
        candleList.OnTickEvent += tick1 => callerTick = true;

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
            .ReturnsAsync(TestUtils.GenerateCandle(TimeSpan.FromMinutes(15), 100));
        _apiHandlerMock.Setup(x => x.GetTradingHoursAsync(It.IsAny<string>()))
            .ReturnsAsync(GetTradeHoursMock);
        var candleList = new CandleList.CandleList(_apiHandlerMock.Object, _loggerMock.Object, Timeframe.FifteenMinutes,
            "EURUSD");

        // Act

        var lastDate = candleList.Last().Date.AddMinutes(3);
        var tick = new Tick(1, 1, lastDate, "test")
            .SetAskVolume(1)
            .SetBidVolume(1);

        candleList.OnCandleEvent += candle => caller = true;
        candleList.OnTickEvent += tick1 => callerTick = true;

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
    public void Test_NewTick_AddNewCandle(Timeframe timeframe)
    {
        // Arrange
        var timeframeMinute = timeframe.GetMinuteFromTimeframe();
        var caller = false;
        var callerTick = false;
        _apiHandlerMock.Setup(x => x.GetChartAsync(It.IsAny<string>(), It.IsAny<Timeframe>()))
            .ReturnsAsync(TestUtils.GenerateCandle(TimeSpan.FromMinutes(timeframeMinute), 100));

        _apiHandlerMock.Setup(x => x.GetTradingHoursAsync(It.IsAny<string>()))
            .ReturnsAsync(GetTradeHoursMock);

        var candleList = new CandleList.CandleList(_apiHandlerMock.Object, _loggerMock.Object, timeframe, "EURUSD");

        // Act

        var lastDate = candleList.Last().Date.AddMinutes(timeframeMinute);
        var tick = new Tick(1, 1, lastDate, "EURUSD")
            .SetAskVolume(1)
            .SetBidVolume(1);

        candleList.OnCandleEvent += candle => caller = true;
        candleList.OnTickEvent += tick1 => callerTick = true;

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
    public void Test_NewTick_AddNewCandle_at2000(Timeframe timeframe)
    {
        // Arrange
        var timeframeMinute = timeframe.GetMinuteFromTimeframe();
        var caller = false;
        var callerTick = false;
        _apiHandlerMock.Setup(x => x.GetChartAsync(It.IsAny<string>(), It.IsAny<Timeframe>()))
            .ReturnsAsync(TestUtils.GenerateCandle(TimeSpan.FromMinutes(timeframeMinute), 2000));
        _apiHandlerMock.Setup(x => x.GetTradingHoursAsync(It.IsAny<string>()))
            .ReturnsAsync(GetTradeHoursMock);
        var candleList = new CandleList.CandleList(_apiHandlerMock.Object, _loggerMock.Object, timeframe, "EURUSD");

        // Act

        var lastDate = candleList.Last().Date.AddMinutes(timeframeMinute);
        var tick = new Tick(1, 1, lastDate, "EURUSD")
            .SetAskVolume(1)
            .SetBidVolume(1);

        candleList.OnCandleEvent += candle => caller = true;
        candleList.OnTickEvent += tick1 => callerTick = true;

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
    public void Test_NewTick_CorrectHistory(Timeframe timeframe)
    {
        // Arrange
        var timeframeMinute = timeframe.GetMinuteFromTimeframe();
        var caller = false;
        var callerTick = false;
        var candleListData = TestUtils.GenerateCandle(TimeSpan.FromMinutes(timeframeMinute), 100);
        _apiHandlerMock.Setup(x => x.GetChartAsync(It.IsAny<string>(), It.IsAny<Timeframe>()))
            .ReturnsAsync(candleListData);
        _apiHandlerMock.Setup(x => x.GetTradingHoursAsync(It.IsAny<string>()))
            .ReturnsAsync(GetTradeHoursMock);
        var candleList = new CandleList.CandleList(_apiHandlerMock.Object, _loggerMock.Object, timeframe, "EURUSD");

        var candleListCorrectHistory =
            TestUtils.GenerateCandle(TimeSpan.FromMinutes(timeframeMinute), 2, candleListData.Last().Date);

        _apiHandlerMock.Setup(x =>
                x.GetChartByDateAsync(It.IsAny<string>(), It.IsAny<Timeframe>(), It.IsAny<DateTime>(),
                    It.IsAny<DateTime>()))
            .ReturnsAsync(candleListCorrectHistory);

        // Act

        var lastDate = candleList.Last().Date.AddMinutes(timeframeMinute * 2);
        var tick = new Tick(2, 2, lastDate, "EURUSD")
            .SetAskVolume(1)
            .SetBidVolume(1);

        candleList.OnCandleEvent += candle => caller = true;
        candleList.OnTickEvent += tick1 => callerTick = true;

        _apiHandlerMock.Raise(x => x.TickEvent += null, this, tick);

        // Assert
        caller.Should().BeFalse();
        callerTick.Should().BeFalse();
        candleList.Count.Should().Be(101);
    }


    [Theory]
    [InlineData(Timeframe.OneMinute)]
    [InlineData(Timeframe.FiveMinutes)]
    [InlineData(Timeframe.FifteenMinutes)]
    [InlineData(Timeframe.ThirtyMinutes)]
    [InlineData(Timeframe.OneHour)]
    [InlineData(Timeframe.FourHour)]
    [InlineData(Timeframe.Daily)]
    public void Test_NewTick_CorrectHistory_more_candle(Timeframe timeframe)
    {
        // Arrange
        var timeframeMinute = timeframe.GetMinuteFromTimeframe();
        var caller = false;
        var callerTick = false;
        var candleListData = TestUtils.GenerateCandle(TimeSpan.FromMinutes(timeframeMinute), 100);
        _apiHandlerMock.Setup(x => x.GetChartAsync(It.IsAny<string>(), It.IsAny<Timeframe>()))
            .ReturnsAsync(candleListData);
        _apiHandlerMock.Setup(x => x.GetTradingHoursAsync(It.IsAny<string>()))
            .ReturnsAsync(GetTradeHoursMock);
        var candleList = new CandleList.CandleList(_apiHandlerMock.Object, _loggerMock.Object, timeframe, "EURUSD");

        var candleListCorrectHistory =
            TestUtils.GenerateCandle(TimeSpan.FromMinutes(timeframeMinute), 10, candleListData.Last().Date);

        _apiHandlerMock.Setup(x =>
                x.GetChartByDateAsync(It.IsAny<string>(), It.IsAny<Timeframe>(), It.IsAny<DateTime>(),
                    It.IsAny<DateTime>()))
            .ReturnsAsync(candleListCorrectHistory);

        // Act

        var lastDate = candleList.Last().Date.AddMinutes(timeframeMinute * 2);
        var tick = new Tick(2, 2, lastDate, "EURUSD")
            .SetAskVolume(1)
            .SetBidVolume(1);

        candleList.OnCandleEvent += candle => caller = true;
        candleList.OnTickEvent += tick1 => callerTick = true;

        _apiHandlerMock.Raise(x => x.TickEvent += null, this, tick);

        // Assert
        caller.Should().BeFalse();
        callerTick.Should().BeFalse();
        candleList.Count.Should().Be(109);
    }


    [Theory]
    [InlineData(Timeframe.OneMinute)]
    [InlineData(Timeframe.FiveMinutes)]
    [InlineData(Timeframe.FifteenMinutes)]
    [InlineData(Timeframe.ThirtyMinutes)]
    [InlineData(Timeframe.OneHour)]
    [InlineData(Timeframe.FourHour)]
    [InlineData(Timeframe.Daily)]
    public void Test_NewTick_CorrectHistory_noCandleFetched(Timeframe timeframe)
    {
        // Arrange
        var timeframeMinute = timeframe.GetMinuteFromTimeframe();
        var caller = false;
        var callerTick = false;
        var candleListData = TestUtils.GenerateCandle(TimeSpan.FromMinutes(timeframeMinute), 100);
        _apiHandlerMock.Setup(x => x.GetChartAsync(It.IsAny<string>(), It.IsAny<Timeframe>()))
            .ReturnsAsync(candleListData);
        _apiHandlerMock.Setup(x => x.GetTradingHoursAsync(It.IsAny<string>()))
            .ReturnsAsync(GetTradeHoursMock);
        var candleList = new CandleList.CandleList(_apiHandlerMock.Object, _loggerMock.Object, timeframe, "EURUSD");

        _apiHandlerMock.Setup(x =>
                x.GetChartByDateAsync(It.IsAny<string>(), It.IsAny<Timeframe>(), It.IsAny<DateTime>(),
                    It.IsAny<DateTime>()))
            .ReturnsAsync(new List<Candle>());

        // Act

        var lastDate = candleList.Last().Date.AddMinutes(timeframeMinute * 2);
        var tick = new Tick(2, 2, lastDate, "EURUSD")
            .SetAskVolume(1)
            .SetBidVolume(1);

        candleList.OnCandleEvent += candle => caller = true;
        candleList.OnTickEvent += tick1 => callerTick = true;

        _apiHandlerMock.Raise(x => x.TickEvent += null, this, tick);

        // Assert
        caller.Should().BeFalse();
        callerTick.Should().BeFalse();
        candleList.Count.Should().Be(100);
    }

    #endregion

    #region Trading Hours tests in

    [Fact]
    public void Test_CurrentHoursRecord_Boucle()
    {
        // Arrange

        _apiHandlerMock.Setup(x => x.GetChartAsync(It.IsAny<string>(), It.IsAny<Timeframe>()))
            .ReturnsAsync(TestUtils.GenerateCandle(TimeSpan.FromMinutes(15), 100));
        _apiHandlerMock.Setup(x => x.GetTradingHoursAsync(It.IsAny<string>()))
            .ReturnsAsync(GetTradeHoursMock);
        // Act
        var candleList = new CandleList.CandleList(_apiHandlerMock.Object, _loggerMock.Object, Timeframe.FifteenMinutes,
            "EURUSD");

        // Assert
        candleList.Count.Should().Be(100);
    }

    [Fact]
    public void Test_CurrentHoursRecord_NoBoucle_NoDepassed()
    {
        // Arrange

        var candles = TestUtils.GenerateCandle(TimeSpan.FromMinutes(15), 100);
        candles.Last().SetDate(DateTime.Now);
        _apiHandlerMock.Setup(x => x.GetChartAsync(It.IsAny<string>(), It.IsAny<Timeframe>()))
            .ReturnsAsync(candles);

        var tradingHour = new TradeHourRecord();

        tradingHour.HoursRecords.Add(new TradeHourRecord.HoursRecordData
        {
            Day = DateTime.Now.DayOfWeek,
            From = DateTime.Now.AddHours(-1).TimeOfDay,
            To = DateTime.Now.AddHours(1).TimeOfDay
        });

        _apiHandlerMock.Setup(x => x.GetTradingHoursAsync(It.IsAny<string>()))
            .ReturnsAsync(tradingHour);
        // Act
        var candleList = new CandleList.CandleList(_apiHandlerMock.Object, _loggerMock.Object, Timeframe.FifteenMinutes,
            "EURUSD");

        // Assert
        candleList.Count.Should().Be(100);
    }

    [Fact]
    public void Test_CurrentHoursRecord_NoBoucle_inferior_from()
    {
        // Arrange
        var now = DateTime.Now;
        var todayWithTimeZeroed = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0);
        var candles = TestUtils.GenerateCandle(TimeSpan.FromMinutes(15), 100);
        candles.Last().SetDate(now.AddMinutes(-15));
        _apiHandlerMock.Setup(x => x.GetChartAsync(It.IsAny<string>(), It.IsAny<Timeframe>()))
            .ReturnsAsync(candles);

        var tradingHour = new TradeHourRecord();

        tradingHour.HoursRecords.Add(new TradeHourRecord.HoursRecordData
        {
            Day = DateTime.Now.DayOfWeek,
            From = todayWithTimeZeroed.AddHours(1).TimeOfDay,
            To = todayWithTimeZeroed.AddHours(2).TimeOfDay
        });

        _apiHandlerMock.Setup(x => x.GetTradingHoursAsync(It.IsAny<string>()))
            .ReturnsAsync(tradingHour);
        // Act
        var candleList = new CandleList.CandleList(_apiHandlerMock.Object, _loggerMock.Object, Timeframe.FifteenMinutes,
            "EURUSD");

        // Assert
        candleList.Count.Should().Be(101);
        candleList.Last().Date.Hour.Should().Be(tradingHour.HoursRecords[0].From.Hours);
        candleList.Last().Date.DayOfWeek.Should().Be(tradingHour.HoursRecords[0].Day);
        candleList.Last().Open.Should().Be(0);
        candleList.Last().High.Should().Be(0);
        candleList.Last().Low.Should().Be(0);
        candleList.Last().Close.Should().Be(0);
    }

    [Fact]
    public void Test_CurrentHoursRecord_NoBoucle_inferior_from_2()
    {
        // Arrange
        var now = DateTime.Now;
        var todayWithTimeZeroed = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0);
        var candles = TestUtils.GenerateCandle(TimeSpan.FromMinutes(15), 100);
        candles.Last().SetDate(now.AddMinutes(-15));
        _apiHandlerMock.Setup(x => x.GetChartAsync(It.IsAny<string>(), It.IsAny<Timeframe>()))
            .ReturnsAsync(candles);

        var tradingHour = new TradeHourRecord();

        tradingHour.HoursRecords.Add(new TradeHourRecord.HoursRecordData
        {
            Day = DateTime.Now.DayOfWeek,
            From = todayWithTimeZeroed.AddHours(1).AddMinutes(10).TimeOfDay,
            To = todayWithTimeZeroed.AddHours(2).TimeOfDay
        });

        _apiHandlerMock.Setup(x => x.GetTradingHoursAsync(It.IsAny<string>()))
            .ReturnsAsync(tradingHour);
        // Act
        var candleList = new CandleList.CandleList(_apiHandlerMock.Object, _loggerMock.Object, Timeframe.FifteenMinutes,
            "EURUSD");

        // Assert
        candleList.Count.Should().Be(101);
        candleList.Last().Date.Hour.Should().Be(tradingHour.HoursRecords[0].From.Hours);
        candleList.Last().Date.DayOfWeek.Should().Be(tradingHour.HoursRecords[0].Day);
        candleList.Last().Open.Should().Be(0);
        candleList.Last().High.Should().Be(0);
        candleList.Last().Low.Should().Be(0);
        candleList.Last().Close.Should().Be(0);
    }

    [Fact]
    public void Test_CurrentHoursRecord_NoBoucle_inferior_from_and_newTick()
    {
        // Arrange
        var now = DateTime.Now;
        var todayWithTimeZeroed = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0);
        var candles = TestUtils.GenerateCandle(TimeSpan.FromMinutes(15), 100);
        candles.Last().SetDate(now.AddMinutes(-15));
        _apiHandlerMock.Setup(x => x.GetChartAsync(It.IsAny<string>(), It.IsAny<Timeframe>()))
            .ReturnsAsync(candles);

        var tradingHour = new TradeHourRecord();

        tradingHour.HoursRecords.Add(new TradeHourRecord.HoursRecordData
        {
            Day = DateTime.Now.DayOfWeek,
            From = todayWithTimeZeroed.AddHours(1).TimeOfDay,
            To = todayWithTimeZeroed.AddHours(2).TimeOfDay
        });

        _apiHandlerMock.Setup(x => x.GetTradingHoursAsync(It.IsAny<string>()))
            .ReturnsAsync(tradingHour);
        var candleList = new CandleList.CandleList(_apiHandlerMock.Object, _loggerMock.Object, Timeframe.FifteenMinutes,
            "EURUSD");
        // Act
        var tick = new Tick(1, 1, DateTime.Now, "EURUSD");

        _apiHandlerMock.Raise(x => x.TickEvent += null, this, tick);


        // Assert
        candleList.Count.Should().Be(101);
        candleList.Last().Date.Hour.Should().Be(tradingHour.HoursRecords[0].From.Hours);
        candleList.Last().Date.DayOfWeek.Should().Be(tradingHour.HoursRecords[0].Day);
        candleList.Last().Open.Should().Be(1);
        candleList.Last().High.Should().Be(1);
        candleList.Last().Low.Should().Be(1);
        candleList.Last().Close.Should().Be(1);
    }


    [Fact]
    public void Test_CurrentHoursRecord_NoBoucle_superior_to()
    {
        // Arrange
        var now = DateTime.Now;
        var todayWithTimeZeroed = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0);
        var candles = TestUtils.GenerateCandle(TimeSpan.FromMinutes(15), 100);
        candles.Last().SetDate(todayWithTimeZeroed.AddHours(1).AddMinutes(-15));
        _apiHandlerMock.Setup(x => x.GetChartAsync(It.IsAny<string>(), It.IsAny<Timeframe>()))
            .ReturnsAsync(candles);

        var tradingHour = new TradeHourRecord();

        tradingHour.HoursRecords.Add(
            new TradeHourRecord.HoursRecordData
            {
                Day = DateTime.Now.DayOfWeek,
                From = todayWithTimeZeroed.AddMinutes(30).TimeOfDay,
                To = todayWithTimeZeroed.AddHours(1).TimeOfDay
            });
        tradingHour.HoursRecords.Add(
            new TradeHourRecord.HoursRecordData
            {
                Day = DateTime.Now.AddDays(1).DayOfWeek,
                From = todayWithTimeZeroed.AddMinutes(30).TimeOfDay,
                To = todayWithTimeZeroed.AddHours(1).TimeOfDay
            });

        _apiHandlerMock.Setup(x => x.GetTradingHoursAsync(It.IsAny<string>()))
            .ReturnsAsync(tradingHour);
        // Act
        var candleList = new CandleList.CandleList(_apiHandlerMock.Object, _loggerMock.Object, Timeframe.FifteenMinutes,
            "EURUSD");

        // Assert
        candleList.Count.Should().Be(101);
        candleList.Last().Date.Hour.Should().Be(tradingHour.HoursRecords[1].From.Hours);
        candleList.Last().Date.DayOfWeek.Should().Be(tradingHour.HoursRecords[1].Day);
        candleList.Last().Open.Should().Be(0);
        candleList.Last().High.Should().Be(0);
        candleList.Last().Low.Should().Be(0);
        candleList.Last().Close.Should().Be(0);
    }

    [Fact]
    public void Test_CurrentHoursRecord_NoBoucle_superior_to_date_ecart()
    {
        // Arrange
        var now = DateTime.Now;
        var todayWithTimeZeroed = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0);
        var candles = TestUtils.GenerateCandle(TimeSpan.FromMinutes(15), 100);
        candles.Last().SetDate(todayWithTimeZeroed.AddHours(1).AddMinutes(-15));
        _apiHandlerMock.Setup(x => x.GetChartAsync(It.IsAny<string>(), It.IsAny<Timeframe>()))
            .ReturnsAsync(candles);

        var tradingHour = new TradeHourRecord();

        tradingHour.HoursRecords.Add(
            new TradeHourRecord.HoursRecordData
            {
                Day = DateTime.Now.DayOfWeek,
                From = todayWithTimeZeroed.AddMinutes(30).TimeOfDay,
                To = todayWithTimeZeroed.AddHours(1).TimeOfDay
            });
        tradingHour.HoursRecords.Add(
            new TradeHourRecord.HoursRecordData
            {
                Day = DateTime.Now.AddDays(4).DayOfWeek,
                From = todayWithTimeZeroed.AddMinutes(30).TimeOfDay,
                To = todayWithTimeZeroed.AddHours(1).TimeOfDay
            });

        _apiHandlerMock.Setup(x => x.GetTradingHoursAsync(It.IsAny<string>()))
            .ReturnsAsync(tradingHour);
        // Act
        var candleList = new CandleList.CandleList(_apiHandlerMock.Object, _loggerMock.Object, Timeframe.FifteenMinutes,
            "EURUSD");

        // Assert
        candleList.Count.Should().Be(101);
        candleList.Last().Date.Hour.Should().Be(tradingHour.HoursRecords[1].From.Hours);
        candleList.Last().Date.DayOfWeek.Should().Be(tradingHour.HoursRecords[1].Day);
        candleList.Last().Open.Should().Be(0);
        candleList.Last().High.Should().Be(0);
        candleList.Last().Low.Should().Be(0);
        candleList.Last().Close.Should().Be(0);
    }

    [Fact]
    public void Test_CurrentHoursRecord_NoBoucle_superior_to_2()
    {
        // Arrange
        var now = DateTime.Now;
        var todayWithTimeZeroed = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0);
        var candles = TestUtils.GenerateCandle(TimeSpan.FromMinutes(15), 100);
        candles.Last().SetDate(todayWithTimeZeroed.AddHours(1).AddMinutes(-15));
        _apiHandlerMock.Setup(x => x.GetChartAsync(It.IsAny<string>(), It.IsAny<Timeframe>()))
            .ReturnsAsync(candles);

        var tradingHour = new TradeHourRecord();

        tradingHour.HoursRecords.Add(
            new TradeHourRecord.HoursRecordData
            {
                Day = DateTime.Now.DayOfWeek,
                From = todayWithTimeZeroed.AddMinutes(30).TimeOfDay,
                To = todayWithTimeZeroed.AddHours(1).TimeOfDay
            });
        tradingHour.HoursRecords.Add(
            new TradeHourRecord.HoursRecordData
            {
                Day = DateTime.Now.AddDays(1).DayOfWeek,
                From = todayWithTimeZeroed.AddMinutes(30).TimeOfDay,
                To = todayWithTimeZeroed.AddHours(1).TimeOfDay
            });

        _apiHandlerMock.Setup(x => x.GetTradingHoursAsync(It.IsAny<string>()))
            .ReturnsAsync(tradingHour);
        // Act
        var candleList = new CandleList.CandleList(_apiHandlerMock.Object, _loggerMock.Object, Timeframe.FifteenMinutes,
            "EURUSD");

        // Assert
        candleList.Count.Should().Be(101);
        candleList.Last().Date.Hour.Should().Be(tradingHour.HoursRecords[1].From.Hours);
        candleList.Last().Date.DayOfWeek.Should().Be(tradingHour.HoursRecords[1].Day);
        candleList.Last().Open.Should().Be(0);
        candleList.Last().High.Should().Be(0);
        candleList.Last().Low.Should().Be(0);
        candleList.Last().Close.Should().Be(0);
    }

    [Fact]
    public void Test_CurrentHoursRecord_NoBoucle_superior_to_and_newTick()
    {
        // Arrange
        var now = DateTime.Now;
        var todayWithTimeZeroed = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0);
        var candles = TestUtils.GenerateCandle(TimeSpan.FromMinutes(15), 100);
        candles.Last().SetDate(todayWithTimeZeroed.AddHours(1).AddMinutes(-15));
        _apiHandlerMock.Setup(x => x.GetChartAsync(It.IsAny<string>(), It.IsAny<Timeframe>()))
            .ReturnsAsync(candles);

        var tradingHour = new TradeHourRecord();

        tradingHour.HoursRecords.Add(
            new TradeHourRecord.HoursRecordData
            {
                Day = DateTime.Now.DayOfWeek,
                From = todayWithTimeZeroed.AddMinutes(30).TimeOfDay,
                To = todayWithTimeZeroed.AddHours(1).TimeOfDay
            });
        tradingHour.HoursRecords.Add(
            new TradeHourRecord.HoursRecordData
            {
                Day = DateTime.Now.AddDays(1).DayOfWeek,
                From = todayWithTimeZeroed.AddMinutes(30).TimeOfDay,
                To = todayWithTimeZeroed.AddHours(1).TimeOfDay
            });

        _apiHandlerMock.Setup(x => x.GetTradingHoursAsync(It.IsAny<string>()))
            .ReturnsAsync(tradingHour);
        var candleList = new CandleList.CandleList(_apiHandlerMock.Object, _loggerMock.Object, Timeframe.FifteenMinutes,
            "EURUSD");
        // Act

        var tick = new Tick(1, 1, DateTime.Now, "EURUSD");

        _apiHandlerMock.Raise(x => x.TickEvent += null, this, tick);


        // Assert
        candleList.Count.Should().Be(101);
        candleList.Last().Date.Hour.Should().Be(tradingHour.HoursRecords[1].From.Hours);
        candleList.Last().Date.DayOfWeek.Should().Be(tradingHour.HoursRecords[1].Day);
        candleList.Last().Open.Should().Be(1);
        candleList.Last().High.Should().Be(1);
        candleList.Last().Low.Should().Be(1);
        candleList.Last().Close.Should().Be(1);
    }


    [Fact]
    public void Test_CurrentHoursRecord_null()
    {
        // Arrange
        var now = DateTime.Now;
        var todayWithTimeZeroed = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0);
        var candles = TestUtils.GenerateCandle(TimeSpan.FromMinutes(15), 100);
        candles.Last().SetDate(todayWithTimeZeroed.AddHours(1).AddMinutes(-15));
        _apiHandlerMock.Setup(x => x.GetChartAsync(It.IsAny<string>(), It.IsAny<Timeframe>()))
            .ReturnsAsync(candles);

        var tradingHour = new TradeHourRecord();


        tradingHour.HoursRecords.Add(
            new TradeHourRecord.HoursRecordData
            {
                Day = DateTime.Now.AddDays(1).DayOfWeek,
                From = todayWithTimeZeroed.AddMinutes(30).TimeOfDay,
                To = todayWithTimeZeroed.AddHours(1).TimeOfDay
            });

        _apiHandlerMock.Setup(x => x.GetTradingHoursAsync(It.IsAny<string>()))
            .ReturnsAsync(tradingHour);
        // Act
        var candleList = new CandleList.CandleList(_apiHandlerMock.Object, _loggerMock.Object, Timeframe.FifteenMinutes,
            "EURUSD");

        // Assert
        candleList.Count.Should().Be(101);
        candleList.Last().Date.Hour.Should().Be(tradingHour.HoursRecords[0].From.Hours);
        candleList.Last().Date.DayOfWeek.Should().Be(tradingHour.HoursRecords[0].Day);
        candleList.Last().Open.Should().Be(0);
        candleList.Last().High.Should().Be(0);
        candleList.Last().Low.Should().Be(0);
        candleList.Last().Close.Should().Be(0);
    }

    [Fact]
    public void Test_CurrentHoursRecord_null_date_ecart()
    {
        // Arrange
        var now = DateTime.Now;
        var todayWithTimeZeroed = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0);
        var candles = TestUtils.GenerateCandle(TimeSpan.FromMinutes(15), 100);
        candles.Last().SetDate(todayWithTimeZeroed.AddHours(1).AddMinutes(-15));
        _apiHandlerMock.Setup(x => x.GetChartAsync(It.IsAny<string>(), It.IsAny<Timeframe>()))
            .ReturnsAsync(candles);

        var tradingHour = new TradeHourRecord();


        tradingHour.HoursRecords.Add(
            new TradeHourRecord.HoursRecordData
            {
                Day = DateTime.Now.AddDays(4).DayOfWeek,
                From = todayWithTimeZeroed.AddMinutes(30).TimeOfDay,
                To = todayWithTimeZeroed.AddHours(1).TimeOfDay
            });

        _apiHandlerMock.Setup(x => x.GetTradingHoursAsync(It.IsAny<string>()))
            .ReturnsAsync(tradingHour);
        // Act
        var candleList = new CandleList.CandleList(_apiHandlerMock.Object, _loggerMock.Object, Timeframe.FifteenMinutes,
            "EURUSD");

        // Assert
        candleList.Count.Should().Be(101);
        candleList.Last().Date.Hour.Should().Be(tradingHour.HoursRecords[0].From.Hours);
        candleList.Last().Date.DayOfWeek.Should().Be(tradingHour.HoursRecords[0].Day);
        candleList.Last().Open.Should().Be(0);
        candleList.Last().High.Should().Be(0);
        candleList.Last().Low.Should().Be(0);
        candleList.Last().Close.Should().Be(0);
    }

    [Fact]
    public void Test_CurrentHoursRecord_null_2()
    {
        // Arrange
        var now = DateTime.Now;
        var todayWithTimeZeroed = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0);
        var candles = TestUtils.GenerateCandle(TimeSpan.FromMinutes(15), 100);
        candles.Last().SetDate(todayWithTimeZeroed.AddHours(1).AddMinutes(-15));
        _apiHandlerMock.Setup(x => x.GetChartAsync(It.IsAny<string>(), It.IsAny<Timeframe>()))
            .ReturnsAsync(candles);

        var tradingHour = new TradeHourRecord();


        tradingHour.HoursRecords.Add(
            new TradeHourRecord.HoursRecordData
            {
                Day = DateTime.Now.AddDays(1).DayOfWeek,
                From = todayWithTimeZeroed.AddMinutes(30).TimeOfDay,
                To = todayWithTimeZeroed.AddHours(1).TimeOfDay
            });

        _apiHandlerMock.Setup(x => x.GetTradingHoursAsync(It.IsAny<string>()))
            .ReturnsAsync(tradingHour);
        // Act
        var candleList = new CandleList.CandleList(_apiHandlerMock.Object, _loggerMock.Object, Timeframe.FifteenMinutes,
            "EURUSD");

        // Assert
        candleList.Count.Should().Be(101);
        candleList.Last().Date.Hour.Should().Be(tradingHour.HoursRecords[0].From.Hours);
        candleList.Last().Date.DayOfWeek.Should().Be(tradingHour.HoursRecords[0].Day);
        candleList.Last().Open.Should().Be(0);
        candleList.Last().High.Should().Be(0);
        candleList.Last().Low.Should().Be(0);
        candleList.Last().Close.Should().Be(0);
    }

    [Fact]
    public void Test_CurrentHoursRecord_NoBoucle_null_newTick()
    {
        // Arrange
        var now = DateTime.Now;
        var todayWithTimeZeroed = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0);
        var candles = TestUtils.GenerateCandle(TimeSpan.FromMinutes(15), 100);
        candles.Last().SetDate(todayWithTimeZeroed.AddHours(1).AddMinutes(-15));
        _apiHandlerMock.Setup(x => x.GetChartAsync(It.IsAny<string>(), It.IsAny<Timeframe>()))
            .ReturnsAsync(candles);

        var tradingHour = new TradeHourRecord();


        tradingHour.HoursRecords.Add(
            new TradeHourRecord.HoursRecordData
            {
                Day = DateTime.Now.AddDays(1).DayOfWeek,
                From = todayWithTimeZeroed.AddMinutes(30).TimeOfDay,
                To = todayWithTimeZeroed.AddHours(1).TimeOfDay
            });

        _apiHandlerMock.Setup(x => x.GetTradingHoursAsync(It.IsAny<string>()))
            .ReturnsAsync(tradingHour);
        var candleList = new CandleList.CandleList(_apiHandlerMock.Object, _loggerMock.Object, Timeframe.FifteenMinutes,
            "EURUSD");
        // Act

        var tick = new Tick(1, 1, DateTime.Now, "EURUSD");

        _apiHandlerMock.Raise(x => x.TickEvent += null, this, tick);


        // Assert
        candleList.Count.Should().Be(101);
        candleList.Last().Date.Hour.Should().Be(tradingHour.HoursRecords[0].From.Hours);
        candleList.Last().Date.DayOfWeek.Should().Be(tradingHour.HoursRecords[0].Day);
        candleList.Last().Open.Should().Be(1);
        candleList.Last().High.Should().Be(1);
        candleList.Last().Low.Should().Be(1);
        candleList.Last().Close.Should().Be(1);
    }

    #endregion

    // Only partial tests possible

    #region Timer

    #endregion
}