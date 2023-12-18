using FluentAssertions;
using Moq;
using RobotAppLibraryV2.ApiHandler.Interfaces;
using RobotAppLibraryV2.CandleList;
using RobotAppLibraryV2.Factory;
using RobotAppLibraryV2.Modeles;
using RobotAppLibraryV2.MoneyManagement;
using RobotAppLibraryV2.Positions;
using RobotAppLibraryV2.Result;
using RobotAppLibraryV2.Strategy;
using RobotAppLibraryV2.Utils;
using Serilog;
using Skender.Stock.Indicators;

namespace RobotAppLibraryV2.Tests.Strategy.ImplementationTests.Indicator;

public class FakeStrategyTestContextIndicatorTest
{
    private readonly Mock<IApiHandler> _apiHandlerMock = new();
    private readonly Mock<ILogger> _loggerMock = new();
    private readonly Mock<ICandleList> candleListMock = new();

    private readonly List<Candle> fakeHistory = TestUtils.GenerateCandle(TimeSpan.FromMinutes(5), 500);
    private readonly FakeStrategyContextIndicator fakeStrategyContextIndicator = new();
    private readonly Mock<IMoneyManagement> moneyManagementMock = new();
    private readonly Mock<IPositionHandler> positionHandlerMock = new();

    private readonly StrategyBase strategyBase;

    private readonly Mock<StrategyImplementationBase> strategyImplementationBaseMock = new();

    private readonly Mock<IStrategyResult> strategyResultMock = new();
    private readonly Mock<IStrategyServiceFactory> strategyServiceFactoryMock = new();

    public FakeStrategyTestContextIndicatorTest()
    {
        _loggerMock.Setup(x => x.ForContext<StrategyBase>())
            .Returns(_loggerMock.Object);
        _loggerMock.Setup(x => x.ForContext(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<bool>()))
            .Returns(_loggerMock.Object);

        _apiHandlerMock.Setup(x => x.PingAsync());

        strategyServiceFactoryMock.Setup(x =>
                x.GetMoneyManagement(It.IsAny<IApiHandler>(), It.IsAny<ILogger>(), It.IsAny<string>(),
                    It.IsAny<string>()))
            .Returns(moneyManagementMock.Object);

        strategyServiceFactoryMock.Setup(x =>
                x.GetHistory(It.IsAny<ILogger>(), It.IsAny<IApiHandler>(), It.IsAny<string>(), It.IsAny<Timeframe>()))
            .Returns(candleListMock.Object);

        strategyServiceFactoryMock.Setup(x => x.GetStrategyResultService(It.IsAny<IApiHandler>(), It.IsAny<string>()))
            .Returns(strategyResultMock.Object);

        strategyServiceFactoryMock.Setup(x =>
                x.GetPositionHandler(It.IsAny<ILogger>(), It.IsAny<IApiHandler>(), It.IsAny<string>(),
                    It.IsAny<string>()))
            .Returns(positionHandlerMock.Object);


        for (var i = 0; i < fakeHistory.Count; i++)
        {
            var capture = i;
            candleListMock.Setup(m => m[capture]).Returns(fakeHistory[capture]);
        }

        candleListMock.SetupGet(cl => cl.Count).Returns(fakeHistory.Count);

        candleListMock.SetupGet(x => x.LastPrice).Returns(new Tick());

        var aggregatedList = fakeHistory.Aggregate(Timeframe.OneHour.ToPeriodSize()).AsEnumerable().Select(x =>
            new Candle()
                .SetOpen(x.Open)
                .SetHigh(x.High)
                .SetLow(x.Low)
                .SetClose(x.Close)
                .SetDate(x.Date)).ToList();

        candleListMock.Setup(x => x.Aggregate(It.IsAny<Timeframe>()))
            .Returns(aggregatedList);

        strategyBase = new StrategyBase(fakeStrategyContextIndicator, "EURUSD",
            Timeframe.FifteenMinutes, Timeframe.OneHour, _apiHandlerMock.Object, _loggerMock.Object,
            strategyServiceFactoryMock.Object);
    }


    [Fact]
    public void Test_Count_Indicator()
    {
        //  Act and Act
        candleListMock.Raise(x => x.OnTickEvent += null, new Tick());

        // Assert 
        fakeStrategyContextIndicator.SarIndicator.Count.Should().Be(500);
        fakeStrategyContextIndicator.SarIndicator2.Count.Should().Be(43);
    }

    [Fact]
    public void Test_Count_Indicator_New_Candle()
    {
        // Arrange
        fakeHistory.Add(new Candle());
        candleListMock.Setup(m => m[fakeHistory.Count - 1]).Returns(fakeHistory[^1]);
        candleListMock.SetupGet(cl => cl.Count).Returns(fakeHistory.Count);

        candleListMock.SetupGet(x => x.LastPrice).Returns(new Tick());

        // Act
        candleListMock.Raise(x => x.OnCandleEvent += null, new Candle());

        // Assert 
        fakeStrategyContextIndicator.SarIndicator.Count.Should().Be(501);
        fakeStrategyContextIndicator.SarIndicator2.Count.Should().Be(43);
    }
}