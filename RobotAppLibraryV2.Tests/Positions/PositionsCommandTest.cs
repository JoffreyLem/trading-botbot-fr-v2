using FluentAssertions;
using Moq;
using RobotAppLibraryV2.ApiHandler.Interfaces;
using RobotAppLibraryV2.Modeles;
using RobotAppLibraryV2.Positions;
using Serilog;

namespace RobotAppLibraryV2.Tests.Positions;

public class PositionsCommandTest
{
    private readonly Mock<IApiHandler> _apiHandlerMock = new();
    private readonly Mock<ILogger> _logger = new();
    private readonly PositionHandler _positionHandler;
    private readonly Tick tickRef = new() { Bid = (decimal?)1.11247, Ask = (decimal?)1.112450 };

    public PositionsCommandTest()
    {
        _logger.Setup(x => x.ForContext<PositionHandler>())
            .Returns(_logger.Object);

        var symbolInfo = new SymbolInfo()
                .WithLeverage(10)
                .WithTickSize(0.00001)
                .WithTickSize2(0.0001)
                .WithCurrency1("EUR")
                .WithCurrency2("USD")
                .WithCategory(Category.Forex)
                .WithSymbol("EURUSD")
            ;

        _apiHandlerMock.Setup(x => x.GetSymbolInformationAsync(It.IsAny<string>()))
            .ReturnsAsync(symbolInfo);

        _apiHandlerMock.Setup(api => api.GetTickPriceAsync(It.IsAny<string>()))
            .ReturnsAsync(tickRef);

        _positionHandler = new PositionHandler(_logger.Object, _apiHandlerMock.Object, "EURUSD");
    }


    private void OpenPositionByTest()
    {
        Test_OpenPosition();
        Test_OpenPosition_CallBack();
    }

    #region OpenPosition

    [Fact]
    public async void Test_OpenPosition()
    {
        // Arrange and Act

        await _positionHandler.OpenPositionAsync("EURUSD", TypePosition.Buy, 0.5, "testing", 1.11237m, 1.11257m);

        // Assert

        _positionHandler.PositionPending.Should().NotBeNull();
        _positionHandler.PositionPending.Id.Should().NotBeNull();
        _positionHandler.PositionPending.Symbol.Should().Be("EURUSD");
        _positionHandler.PositionPending.TypePosition.Should().Be(TypePosition.Buy);
        _positionHandler.PositionPending.OpenPrice.Should().Be(1.112450m);
        _positionHandler.PositionPending.StopLoss.Should().Be(1.11237m);
        _positionHandler.PositionPending.TakeProfit.Should().Be(1.11257m);
        _positionHandler.PositionPending.Volume.Should().Be(0.5);
        _positionHandler.PositionPending.Comment.Should().Be("testing");
        _positionHandler.PositionPending.CustomComment.Should().Be("testing");
        _positionHandler.PositionPending.StrategyId.Should().Be("testing");
    }

    [Fact]
    public async void Test_OpenPosition_Default_sl_tp()
    {
        // Arrange and Act

        await _positionHandler.OpenPositionAsync("EURUSD", TypePosition.Buy, 0.5, "testing");

        // Assert

        _positionHandler.PositionPending.Should().NotBeNull();
        _positionHandler.PositionPending.Id.Should().NotBeNull();
        _positionHandler.PositionPending.Symbol.Should().Be("EURUSD");
        _positionHandler.PositionPending.TypePosition.Should().Be(TypePosition.Buy);
        _positionHandler.PositionPending.OpenPrice.Should().Be(1.112450m);
        _positionHandler.PositionPending.StopLoss.Should().Be(1.11227M);
        _positionHandler.PositionPending.TakeProfit.Should().Be(1.11265M);
        _positionHandler.PositionPending.Volume.Should().Be(0.5);
        _positionHandler.PositionPending.Comment.Should().Be("testing");
        _positionHandler.PositionPending.CustomComment.Should().Be("testing");
        _positionHandler.PositionPending.StrategyId.Should().Be("testing");
    }


    [Fact]
    public async void Test_OpenPosition_throw_Exception()
    {
        // Arrange
        _apiHandlerMock.Setup(x => x.OpenPositionAsync(It.IsAny<Position>()))
            .ThrowsAsync(new Exception());

        // Act
        await _positionHandler.OpenPositionAsync("EURUSD", TypePosition.Buy, 0.5, "testing", 1.11237m, 1.11257m);

        // Assert
        _positionHandler.PositionPending.Should().BeNull();
    }

    [Fact]
    public async void Test_OpenPosition_CallBack()
    {
        // Arrange
        Test_OpenPosition();
        var position = new Position()
            .SetSymbol("EURUSD")
            .SetId(_positionHandler.PositionPending.Id);
        var caller = false;

        _positionHandler.PositionOpenedEvent += (sender, position1) => caller = true;

        // Act 

        _apiHandlerMock.Raise(x => x.PositionOpenedEvent += null, this, position);

        // Assert

        _positionHandler.PositionOpened.Should().NotBeNull();
        _positionHandler.PositionOpened.StatusPosition.Should().Be(StatusPosition.Open);
        _positionHandler.PositionPending.Should().BeNull();
        caller.Should().BeTrue();
    }


    [Fact]
    public async void Test_OpenPosition_CallBack_nomatch_id()
    {
        // Arrange
        Test_OpenPosition();
        var position = new Position()
            .SetSymbol("EURUSD")
            .SetId("test");
        var caller = false;

        _positionHandler.PositionOpenedEvent += (sender, position1) => caller = true;

        // Act 

        _apiHandlerMock.Raise(x => x.PositionOpenedEvent += null, this, position);

        // Assert
        _positionHandler.PositionOpened.Should().BeNull();
        _positionHandler.PositionPending.Should().NotBeNull();
        caller.Should().BeFalse();
    }

    [Fact]
    public async void Test_OpenPosition_CallBack_rejected()
    {
        // Arrange
        Test_OpenPosition();
        var position = new Position()
            .SetSymbol("EURUSD")
            .SetId(_positionHandler.PositionPending.Id);
        var caller = false;

        _positionHandler.PositionRejectedEvent += (sender, position1) => caller = true;

        // Act 

        _apiHandlerMock.Raise(x => x.PositionRejectedEvent += null, this, position);

        // Assert

        _positionHandler.PositionOpened.Should().BeNull();
        _positionHandler.PositionPending.Should().BeNull();
        caller.Should().BeTrue();
    }


    [Fact]
    public async void Test_OpenPosition_CallBack_no_match_id()
    {
        // Arrange
        Test_OpenPosition();
        var position = new Position()
            .SetSymbol("EURUSD")
            .SetId("test");
        var caller = false;

        _positionHandler.PositionRejectedEvent += (sender, position1) => caller = true;

        // Act 

        _apiHandlerMock.Raise(x => x.PositionRejectedEvent += null, this, position);

        // Assert

        _positionHandler.PositionOpened.Should().BeNull();
        _positionHandler.PositionPending.Should().NotBeNull();
        caller.Should().BeFalse();
    }

    #endregion


    #region UpdatePosition

    [Fact]
    public async void Test_UpdatePosition_noUpdate_same_sltp()
    {
        // Arrange
        OpenPositionByTest();
        var position = _positionHandler.PositionOpened;

        // Act
        await _positionHandler.UpdatePositionAsync(position);

        // Assert
        _apiHandlerMock.Verify(x => x.UpdatePositionAsync(It.IsAny<decimal>(), It.IsAny<Position>()), Times.Never);
    }


    [Fact]
    public async void Test_UpdatePosition_different_sl()
    {
        // Arrange
        OpenPositionByTest();
        var position = new Position()
            .SetStopLoss(1.11247m);


        // Act
        await _positionHandler.UpdatePositionAsync(position);

        // Assert
        _apiHandlerMock.Verify(x => x.UpdatePositionAsync(It.IsAny<decimal>(), It.IsAny<Position>()), Times.Once);
    }

    [Fact]
    public async void Test_UpdatePosition__waitClosel()
    {
        // Arrange
        OpenPositionByTest();
        var position = new Position()
            .SetStatusPosition(StatusPosition.WaitClose)
            .SetStopLoss(1.11247m);


        // Act
        await _positionHandler.UpdatePositionAsync(position);

        // Assert
        _apiHandlerMock.Verify(x => x.UpdatePositionAsync(It.IsAny<decimal>(), It.IsAny<Position>()), Times.Never);
    }

    [Fact]
    public async void Test_UpdatePosition_different_tp()
    {
        // Arrange
        OpenPositionByTest();
        var position = new Position()
            .SetTakeProfit(1.11267m);

        // Act
        await _positionHandler.UpdatePositionAsync(position);

        // Assert
        _apiHandlerMock.Verify(x => x.UpdatePositionAsync(It.IsAny<decimal>(), It.IsAny<Position>()), Times.Once);
    }

    [Fact]
    public async void Test_UpdatePosition_throw_Exception()
    {
        // Arrange
        OpenPositionByTest();
        var position = new Position()
            .SetTakeProfit(1.11267m);

        _apiHandlerMock.Setup(x => x.UpdatePositionAsync(It.IsAny<decimal>(), It.IsAny<Position>()))
            .ThrowsAsync(new Exception());

        // Act
        await _positionHandler.UpdatePositionAsync(position);

        // Assert
        _apiHandlerMock.Verify(x => x.UpdatePositionAsync(It.IsAny<decimal>(), It.IsAny<Position>()), Times.Once);
        _logger.Verify(x => x.Error(It.IsAny<Exception?>(), It.IsAny<string>(), It.IsAny<string>()));
    }

    [Fact]
    public void Test_UpdatePosition_Callback()
    {
        // Arrange
        OpenPositionByTest();
        var position = new Position()
            .SetId(_positionHandler.PositionOpened.Id)
            .SetTakeProfit(1.11267m)
            .SetStopLoss(1.11247m)
            .SetProfit(20);

        var caller = false;

        _positionHandler.PositionUpdatedEvent += (sender, position1) => caller = true;

        // Act
        _apiHandlerMock.Raise(x => x.PositionUpdatedEvent += null, this, position);

        // Assert

        _positionHandler.PositionOpened.Profit.Should().Be(position.Profit);
        _positionHandler.PositionOpened.TakeProfit.Should().Be(position.TakeProfit);
        _positionHandler.PositionOpened.StopLoss.Should().Be(position.StopLoss);
        caller.Should().BeTrue();
    }

    [Fact]
    public void Test_UpdatePosition_Callback_noMatch_id()
    {
        // Arrange
        OpenPositionByTest();
        var position = new Position()
            .SetId("test")
            .SetTakeProfit(1.11267m)
            .SetStopLoss(1.11247m)
            .SetProfit(20);

        var caller = false;

        _positionHandler.PositionUpdatedEvent += (sender, position1) => caller = true;

        // Act
        _apiHandlerMock.Raise(x => x.PositionUpdatedEvent += null, this, position);

        // Assert

        _positionHandler.PositionOpened.Profit.Should().NotBe(position.Profit);
        _positionHandler.PositionOpened.TakeProfit.Should().NotBe(position.TakeProfit);
        _positionHandler.PositionOpened.StopLoss.Should().NotBe(position.StopLoss);
        caller.Should().BeFalse();
    }

    #endregion

    #region Close position

    [Fact]
    public async void Test_ClosePosition()
    {
        // Arrange
        OpenPositionByTest();
        var position = new Position()
            .SetId(_positionHandler.PositionOpened.Id);

        // Act

        await _positionHandler.ClosePositionAsync(position);

        // Assert

        _apiHandlerMock.Verify(x => x.ClosePositionAsync(It.IsAny<decimal>(), It.IsAny<Position>()), Times.Once);
    }


    [Fact]
    public async void Test_ClosePosition_state_waitclose()
    {
        // Arrange
        OpenPositionByTest();
        var position = new Position()
            .SetId(_positionHandler.PositionOpened.Id)
            .SetStatusPosition(StatusPosition.WaitClose);

        // Act

        await _positionHandler.ClosePositionAsync(position);

        // Assert

        _apiHandlerMock.Verify(x => x.ClosePositionAsync(It.IsAny<decimal>(), It.IsAny<Position>()), Times.Never);
    }


    [Fact]
    public async void Test_ClosePosition_throw_error()
    {
        // Arrange
        OpenPositionByTest();
        var position = new Position()
            .SetId(_positionHandler.PositionOpened.Id);

        _apiHandlerMock.Setup(x => x.ClosePositionAsync(It.IsAny<decimal>(), It.IsAny<Position>()))
            .ThrowsAsync(new Exception());

        // Act
        await _positionHandler.ClosePositionAsync(position);

        // Assert
        _apiHandlerMock.Verify(x => x.ClosePositionAsync(It.IsAny<decimal>(), It.IsAny<Position>()), Times.Once);
        _logger.Verify(x => x.Error(It.IsAny<Exception?>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async void Test_ClosePosition_callBack()
    {
        // Arrange
        OpenPositionByTest();
        var position = new Position()
            .SetId(_positionHandler.PositionOpened.Id);

        var caller = false;
        _positionHandler.PositionClosedEvent += (sender, position1) => caller = true;

        // Act
        _apiHandlerMock.Raise(x => x.PositionClosedEvent += null, this, position);


        // Assert
        caller.Should().BeTrue();
        _positionHandler.PositionOpened.Should().BeNull();
    }


    [Fact]
    public async void Test_ClosePosition_callBack_noMatch_id()
    {
        // Arrange
        OpenPositionByTest();
        var position = new Position()
            .SetId("test");

        var caller = false;
        _positionHandler.PositionClosedEvent += (sender, position1) => caller = true;

        // Act
        _apiHandlerMock.Raise(x => x.PositionClosedEvent += null, this, position);


        // Assert
        caller.Should().BeFalse();
        _positionHandler.PositionOpened.Should().NotBeNull();
    }

    #endregion

    #region Calculate StopLoss

    [Fact]
    public void Test_Calculate_StopLoss_buy()
    {
        // arrange and act

        var sl = _positionHandler.CalculateStopLoss(50, TypePosition.Buy);

        // Assert 

        sl.Should().Be(1.11197M);
    }

    [Fact]
    public void Test_Calculate_StopLoss_sell()
    {
        // arrange and act

        var sl = _positionHandler.CalculateStopLoss(50, TypePosition.Sell);

        // Assert 

        sl.Should().Be(1.11295M);
    }


    [Fact]
    public void Test_Calculate_StopLoss_buy_other_quotation()
    {
        // Arrange
        var symbolInfo = new SymbolInfo()
                .WithLeverage(10)
                .WithTickSize(0.1)
                .WithTickSize2(0.1)
                .WithCurrency1("EUR")
                .WithCurrency2("EUR")
                .WithCategory(Category.Indices)
                .WithSymbol("DE30")
            ;

        var apiHandlerMock = new Mock<IApiHandler>();

        apiHandlerMock.Setup(x => x.GetSymbolInformationAsync(It.IsAny<string>()))
            .ReturnsAsync(symbolInfo);

        apiHandlerMock.Setup(api => api.GetTickPriceAsync(It.IsAny<string>()))
            .ReturnsAsync(tickRef);


        apiHandlerMock.Setup(api => api.GetTickPriceAsync(It.IsAny<string>()))
            .ReturnsAsync(new Tick { Ask = 15924.1m, Bid = 15921.9m });

        var positionHandler = new PositionHandler(_logger.Object, apiHandlerMock.Object, "DE30");


        // act

        var sl = positionHandler.CalculateStopLoss(50, TypePosition.Buy);

        // Assert 

        sl.Should().Be(15871.9M);
    }

    [Fact]
    public void Test_Calculate_StopLoss_sell_other_quotation()
    {
        // Arrange
        var symbolInfo = new SymbolInfo()
                .WithLeverage(10)
                .WithTickSize(0.1)
                .WithTickSize2(0.1)
                .WithCurrency1("EUR")
                .WithCurrency2("EUR")
                .WithCategory(Category.Indices)
                .WithSymbol("DE30")
            ;

        var apiHandlerMock = new Mock<IApiHandler>();

        apiHandlerMock.Setup(x => x.GetSymbolInformationAsync(It.IsAny<string>()))
            .ReturnsAsync(symbolInfo);

        apiHandlerMock.Setup(api => api.GetTickPriceAsync(It.IsAny<string>()))
            .ReturnsAsync(tickRef);


        apiHandlerMock.Setup(api => api.GetTickPriceAsync(It.IsAny<string>()))
            .ReturnsAsync(new Tick { Ask = 15924.1m, Bid = 15921.9m });

        var positionHandler = new PositionHandler(_logger.Object, apiHandlerMock.Object, "DE30");


        // act

        var sl = positionHandler.CalculateStopLoss(50, TypePosition.Sell);

        // Assert 

        sl.Should().Be(15974.1M);
    }

    #endregion

    #region Calculate Take Profit

    [Fact]
    public void Test_Calculate_takeProfit_buy()
    {
        // arrange and act

        var sl = _positionHandler.CalculateTakeProfit(50, TypePosition.Buy);

        // Assert 

        sl.Should().Be(1.11295M);
    }

    [Fact]
    public void Test_Calculate_TakeProfit_sell()
    {
        // arrange and act

        var sl = _positionHandler.CalculateTakeProfit(50, TypePosition.Sell);

        // Assert 

        sl.Should().Be(1.11197M);
    }


    [Fact]
    public void Test_Calculate_TakePRofit_buy_other_quotation()
    {
        // Arrange
        var symbolInfo = new SymbolInfo()
                .WithLeverage(10)
                .WithTickSize(0.1)
                .WithTickSize2(0.1)
                .WithCurrency1("EUR")
                .WithCurrency2("EUR")
                .WithCategory(Category.Indices)
                .WithSymbol("DE30")
            ;

        var apiHandlerMock = new Mock<IApiHandler>();

        apiHandlerMock.Setup(x => x.GetSymbolInformationAsync(It.IsAny<string>()))
            .ReturnsAsync(symbolInfo);

        apiHandlerMock.Setup(api => api.GetTickPriceAsync(It.IsAny<string>()))
            .ReturnsAsync(tickRef);


        apiHandlerMock.Setup(api => api.GetTickPriceAsync(It.IsAny<string>()))
            .ReturnsAsync(new Tick { Ask = 15924.1m, Bid = 15921.9m });

        var positionHandler = new PositionHandler(_logger.Object, apiHandlerMock.Object, "DE30");


        // act

        var sl = positionHandler.CalculateTakeProfit(50, TypePosition.Buy);

        // Assert 

        sl.Should().Be(15974.1M);
    }

    [Fact]
    public void Test_Calculate_TakeProfit_sell_other_quotation()
    {
        // Arrange
        var symbolInfo = new SymbolInfo()
                .WithLeverage(10)
                .WithTickSize(0.1)
                .WithTickSize2(0.1)
                .WithCurrency1("EUR")
                .WithCurrency2("EUR")
                .WithCategory(Category.Indices)
                .WithSymbol("DE30")
            ;

        var apiHandlerMock = new Mock<IApiHandler>();

        apiHandlerMock.Setup(x => x.GetSymbolInformationAsync(It.IsAny<string>()))
            .ReturnsAsync(symbolInfo);

        apiHandlerMock.Setup(api => api.GetTickPriceAsync(It.IsAny<string>()))
            .ReturnsAsync(tickRef);


        apiHandlerMock.Setup(api => api.GetTickPriceAsync(It.IsAny<string>()))
            .ReturnsAsync(new Tick { Ask = 15924.1m, Bid = 15921.9m });

        var positionHandler = new PositionHandler(_logger.Object, apiHandlerMock.Object, "DE30");


        // act

        var sl = positionHandler.CalculateTakeProfit(50, TypePosition.Sell);

        // Assert 

        sl.Should().Be(15871.9M);
    }

    #endregion
}