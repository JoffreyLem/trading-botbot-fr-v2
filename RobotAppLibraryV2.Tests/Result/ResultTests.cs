using FluentAssertions;
using Moq;
using RobotAppLibraryV2.ApiHandler;
using RobotAppLibraryV2.Modeles;
using RobotAppLibraryV2.Results;

namespace RobotAppLibraryV2.Tests.Result;

public class ResultTests
{
    private readonly Mock<IApiHandler> apiMock = new();

    [Fact]
    public void TestCalculateResults_WithOnePosition_ShouldCalculateCorrectResults()
    {
        // Arrange and act
        var position = new Position { Profit = 100, DateOpen = DateTime.Now };

        apiMock.Setup(x => x.GetAllPositionsByCommentAsync(It.IsAny<string>()))
            .ReturnsAsync(new List<Position> { position });
        var strategyResult = new StrategyResult(apiMock.Object, "");
        // Assert
        strategyResult.Results.Profit.Should().Be(100);
        strategyResult.Results.ProfitPositif.Should().Be(100);
        strategyResult.Results.ProfitNegatif.Should().Be(0);
        strategyResult.Results.TotalPositions.Should().Be(1);
        strategyResult.Results.TotalPositionPositive.Should().Be(1);
        strategyResult.Results.TotalPositionNegative.Should().Be(0);
        strategyResult.Results.MoyenneProfit.Should().Be(100);
        strategyResult.Results.MoyennePositive.Should().Be(100);
        strategyResult.Results.MoyenneNegative.Should().Be(0);
        strategyResult.Results.RatioMoyennePositifNegatif.Should().Be(0);
        strategyResult.Results.GainMax.Should().Be(100);
        strategyResult.Results.PerteMax.Should().Be(0);
        strategyResult.Results.TauxReussite.Should().Be(100);
        strategyResult.Results.ProfitFactor.Should().Be(0);
        strategyResult.Results.DrawndownMax.Should().Be(0);
        strategyResult.Results.Drawndown.Should().Be(0);
    }

    [Fact]
    public void TestCalculateResults_PositionClosed_Callback()
    {
        // Arrange and act
        var position = new Position { Profit = 100, DateOpen = DateTime.Now };
        var position2 = new Position { Profit = 100, DateOpen = DateTime.Now, StrategyId = "test" };
        apiMock.Setup(x => x.GetAllPositionsByCommentAsync(It.IsAny<string>()))
            .ReturnsAsync(new List<Position> { position });
        var strategyResult = new StrategyResult(apiMock.Object, "test");
        apiMock.Raise(x => x.PositionClosedEvent += null, this, position2);

        // Assert
        strategyResult.Results.Profit.Should().Be(200);
        strategyResult.Results.ProfitPositif.Should().Be(200);
        strategyResult.Results.ProfitNegatif.Should().Be(0);
        strategyResult.Results.TotalPositions.Should().Be(2);
        strategyResult.Results.TotalPositionPositive.Should().Be(2);
        strategyResult.Results.TotalPositionNegative.Should().Be(0);
        strategyResult.Results.MoyenneProfit.Should().Be(100);
        strategyResult.Results.MoyennePositive.Should().Be(100);
        strategyResult.Results.MoyenneNegative.Should().Be(0);
        strategyResult.Results.RatioMoyennePositifNegatif.Should().Be(0);
        strategyResult.Results.GainMax.Should().Be(100);
        strategyResult.Results.PerteMax.Should().Be(0);
        strategyResult.Results.TauxReussite.Should().Be(100);
        strategyResult.Results.ProfitFactor.Should().Be(0);
        strategyResult.Results.DrawndownMax.Should().Be(0);
        strategyResult.Results.Drawndown.Should().Be(0);
    }

    [Fact]
    public void TestCalculateResults_PositionClosed_Callback_NoRun()
    {
        // Arrange and act
        var position = new Position { Profit = 100, DateOpen = DateTime.Now };
        var position2 = new Position { Profit = 100, DateOpen = DateTime.Now, StrategyId = "truc" };
        apiMock.Setup(x => x.GetAllPositionsByCommentAsync(It.IsAny<string>()))
            .ReturnsAsync(new List<Position> { position });
        var strategyResult = new StrategyResult(apiMock.Object, "test");
        apiMock.Raise(x => x.PositionClosedEvent += null, this, position2);

        // Assert
        strategyResult.Results.Profit.Should().Be(100);
        strategyResult.Results.ProfitPositif.Should().Be(100);
        strategyResult.Results.ProfitNegatif.Should().Be(0);
        strategyResult.Results.TotalPositions.Should().Be(1);
        strategyResult.Results.TotalPositionPositive.Should().Be(1);
        strategyResult.Results.TotalPositionNegative.Should().Be(0);
        strategyResult.Results.MoyenneProfit.Should().Be(100);
        strategyResult.Results.MoyennePositive.Should().Be(100);
        strategyResult.Results.MoyenneNegative.Should().Be(0);
        strategyResult.Results.RatioMoyennePositifNegatif.Should().Be(0);
        strategyResult.Results.GainMax.Should().Be(100);
        strategyResult.Results.PerteMax.Should().Be(0);
        strategyResult.Results.TauxReussite.Should().Be(100);
        strategyResult.Results.ProfitFactor.Should().Be(0);
        strategyResult.Results.DrawndownMax.Should().Be(0);
        strategyResult.Results.Drawndown.Should().Be(0);
    }


    [Fact]
    public void TestCalculateResults_WithTwoPositions_OnePositiveAndOneNegative_ShouldCalculateCorrectResults()
    {
        // Arrange and Act
        var positions = new List<Position>
        {
            new() { Profit = 100 },
            new() { Profit = -50 }
        };

        apiMock.Setup(x => x.GetAllPositionsByCommentAsync(It.IsAny<string>()))
            .ReturnsAsync(positions);
        var strategyResult = new StrategyResult(apiMock.Object, "");

        // Assert
        strategyResult.Results.Profit.Should().Be(50);
        strategyResult.Results.ProfitPositif.Should().Be(100);
        strategyResult.Results.ProfitNegatif.Should().Be(-50);
        strategyResult.Results.TotalPositions.Should().Be(2);
        strategyResult.Results.TotalPositionPositive.Should().Be(1);
        strategyResult.Results.TotalPositionNegative.Should().Be(1);
        strategyResult.Results.MoyenneProfit.Should().Be(25);
        strategyResult.Results.MoyennePositive.Should().Be(100);
        strategyResult.Results.MoyenneNegative.Should().Be(-50);
        strategyResult.Results.RatioMoyennePositifNegatif.Should().Be(-2);
        strategyResult.Results.GainMax.Should().Be(100);
        strategyResult.Results.PerteMax.Should().Be(-50);
        strategyResult.Results.TauxReussite.Should().Be(50);
        strategyResult.Results.ProfitFactor.Should().Be(2);
        strategyResult.Results.DrawndownMax.Should().Be(0);
        strategyResult.Results.Drawndown.Should().Be(150);
    }

    [Fact]
    public void CalculateResults_ReturnsCorrectResult()
    {
        // Arrange
        var positions = new List<Position>
        {
            new() { Profit = 10m },
            new() { Profit = 20m },
            new() { Profit = -5m },
            new() { Profit = 15m },
            new() { Profit = -10m }
        };
        apiMock.Setup(x => x.GetAllPositionsByCommentAsync(It.IsAny<string>()))
            .ReturnsAsync(positions);

        var strategyResult = new StrategyResult(apiMock.Object, "");


        // Act
        var result = strategyResult.CalculateResults();

        // Assert
        result.Profit.Should().Be(30m);
        result.ProfitPositif.Should().Be(45m);
        result.ProfitNegatif.Should().Be(-15m);
        result.TotalPositions.Should().Be(5);
        result.TotalPositionPositive.Should().Be(3);
        result.TotalPositionNegative.Should().Be(2);
        result.MoyenneProfit.Should().Be(6m);
        result.MoyennePositive.Should().Be(15m);
        result.MoyenneNegative.Should().Be(-7.5m);
        result.RatioMoyennePositifNegatif.Should().Be(-2m);
        result.GainMax.Should().Be(20m);
        result.PerteMax.Should().Be(-10m);
        result.TauxReussite.Should().Be(60);
        result.ProfitFactor.Should().Be(3m);
        strategyResult.Results.DrawndownMax.Should().Be(0);
        strategyResult.Results.Drawndown.Should().Be(30);
    }

    [Fact]
    public void TestCalculateResults_WithFourPositions_ShouldCalculateCorrectResults_specifiq_drawdown()
    {
        // Arrange and act
        var positions = new List<Position>
        {
            new() { Profit = 100, DateClose = new DateTime(2023, 1, 1) },
            new() { Profit = -50, DateClose = new DateTime(2023, 1, 2) },
            new() { Profit = 25, DateClose = new DateTime(2023, 1, 3) },
            new() { Profit = 10, DateClose = new DateTime(2023, 1, 4) }
        };
        apiMock.Setup(x => x.GetAllPositionsByCommentAsync(It.IsAny<string>()))
            .ReturnsAsync(positions);
        var strategyResult = new StrategyResult(apiMock.Object, "");
        // Act
        var result = strategyResult.CalculateResults();

        // Assert
        result.Profit.Should().Be(85);
        result.ProfitPositif.Should().Be(135);
        result.ProfitNegatif.Should().Be(-50);
        result.TotalPositions.Should().Be(4);
        result.TotalPositionPositive.Should().Be(3);
        result.TotalPositionNegative.Should().Be(1);
        result.MoyenneProfit.Should().Be(21.25m);
        result.MoyennePositive.Should().Be(45);
        result.MoyenneNegative.Should().Be(-50);
        result.RatioMoyennePositifNegatif.Should().Be(-0.9m);
        result.GainMax.Should().Be(100);
        result.PerteMax.Should().Be(-50);
        result.TauxReussite.Should().Be(75);
        result.ProfitFactor.Should().Be(2.7m);
        strategyResult.Results.DrawndownMax.Should().Be(0);
        strategyResult.Results.Drawndown.Should().Be(90);
    }

    [Fact]
    public void TestCalculateResults_WithFourPositions_ShouldCalculateCorrectResults_add_newPosition()
    {
        // Arrange and act
        var positions = new List<Position>
        {
            new() { Profit = 100, DateClose = new DateTime(2023, 1, 1) },
            new() { Profit = -50, DateClose = new DateTime(2023, 1, 2) },
            new() { Profit = 25, DateClose = new DateTime(2023, 1, 3) },
            new() { Profit = 10, DateClose = new DateTime(2023, 1, 4) }
        };
        apiMock.Setup(x => x.GetAllPositionsByCommentAsync(It.IsAny<string>()))
            .ReturnsAsync(positions);
        var strategyResult = new StrategyResult(apiMock.Object, "");
        // Act
        var position = new Position { Profit = 10, DateClose = new DateTime(2023, 1, 5) };
        strategyResult.UpdateGlobalData(position);
        var result = strategyResult.CalculateResults();

        // Assert
        result.Profit.Should().Be(95);
        result.ProfitPositif.Should().Be(145);
        result.ProfitNegatif.Should().Be(-50);
        result.TotalPositions.Should().Be(5);
        result.TotalPositionPositive.Should().Be(4);
        result.TotalPositionNegative.Should().Be(1);
        result.MoyenneProfit.Should().Be(19m);
        result.MoyennePositive.Should().Be(36.25m);
        result.MoyenneNegative.Should().Be(-50);
        result.RatioMoyennePositifNegatif.Should().Be(-0.725m);
        result.GainMax.Should().Be(100);
        result.PerteMax.Should().Be(-50);
        result.TauxReussite.Should().Be(80);
        result.ProfitFactor.Should().Be(2.9m);
        strategyResult.Results.DrawndownMax.Should().Be(0);
        strategyResult.Results.Drawndown.Should().Be(90);
    }

    [Fact]
    public void TestCalculateResults_LooseStreak()
    {
        // Arrange and Act
        var positions = new List<Position>
        {
            new() { Profit = -100 },
            new() { Profit = -50 },
            new() { Profit = -50 },
            new() { Profit = -50 },
            new() { Profit = -50 }
        };

        var caller = false;

        apiMock.Setup(x => x.GetAllPositionsByCommentAsync(It.IsAny<string>()))
            .ReturnsAsync(positions);

        apiMock.Setup(x => x.GetBalanceAsync())
            .ReturnsAsync(new AccountBalance
            {
                Balance = 1000
            });

        var strategyResult = new StrategyResult(apiMock.Object, "test");
        strategyResult.LooseStreak = 5;
        strategyResult.SecureControlPosition = true;

        // Assert
        strategyResult.ResultTresholdEvent += (sender, treshold) =>
        {
            caller = true;
            treshold.Should().Be(EventTreshold.LooseStreak);
        };

        apiMock.Raise(x => x.PositionClosedEvent += null, this, new Position { Profit = -10, StrategyId = "test" });

        caller.Should().Be(true);
    }

    [Fact]
    public void TestCalculateResults_ProfitFactor()
    {
        // Arrange and Act
        var positions = new List<Position>
        {
            new() { Profit = 10 },
            new() { Profit = -100 }
        };

        var caller = false;

        apiMock.Setup(x => x.GetAllPositionsByCommentAsync(It.IsAny<string>()))
            .ReturnsAsync(positions);

        apiMock.Setup(x => x.GetBalanceAsync())
            .ReturnsAsync(new AccountBalance
            {
                Balance = 1000
            });

        var strategyResult = new StrategyResult(apiMock.Object, "test");
        strategyResult.LooseStreak = 5;
        strategyResult.SecureControlPosition = true;
        strategyResult.ToleratedDrawnDown = 50;

        // Assert
        strategyResult.ResultTresholdEvent += (sender, treshold) =>
        {
            caller = true;
            treshold.Should().Be(EventTreshold.Profitfactor);
        };

        apiMock.Raise(x => x.PositionClosedEvent += null, this, new Position { Profit = -10, StrategyId = "test" });

        caller.Should().Be(true);
    }

    [Fact]
    public void TestCalculateResults_Drawdown()
    {
        // Arrange and Act
        var positions = new List<Position>
        {
            new() { Profit = -50 },
            new() { Profit = 100 }
        };

        var caller = false;

        apiMock.Setup(x => x.GetAllPositionsByCommentAsync(It.IsAny<string>()))
            .ReturnsAsync(positions);

        apiMock.Setup(x => x.GetBalanceAsync())
            .ReturnsAsync(new AccountBalance
            {
                Balance = 1000
            });

        var strategyResult = new StrategyResult(apiMock.Object, "test");
        strategyResult.LooseStreak = 5;
        strategyResult.SecureControlPosition = true;
        strategyResult.ToleratedDrawnDown = 10;

        // Assert
        strategyResult.ResultTresholdEvent += (sender, treshold) =>
        {
            caller = true;
            treshold.Should().Be(EventTreshold.Drowdown);
        };

        apiMock.Raise(x => x.PositionClosedEvent += null, this, new Position { Profit = -10, StrategyId = "test" });

        caller.Should().Be(true);
    }
}