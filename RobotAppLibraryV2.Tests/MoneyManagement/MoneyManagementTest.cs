using FluentAssertions;
using Moq;
using RobotAppLibraryV2.ApiHandler.Interfaces;
using RobotAppLibraryV2.Modeles;
using RobotAppLibraryV2.Modeles.Enum;
using RobotAppLibraryV2.MoneyManagement;
using Serilog;

namespace RobotAppLibraryV2.Tests.MoneyManagement;

public class MoneyManagementTest
{
    private readonly Mock<ILogger> _logger = new();


    public MoneyManagementTest()
    {
        _logger.Setup(x => x.ForContext<RobotAppLibraryV2.MoneyManagement.MoneyManagement>())
            .Returns(_logger.Object);
    }

    #region Init

    [Fact]
    public void test_Init_With_BaseSynbol()
    {
        // Arranage
        var apiHandlerMock = new Mock<IApiHandler>();
        var symbolInfo = new SymbolInfo()
            .WithLeverage(10)
            .WithTickSize(0.00001)
            .WithTickSize2(0.0001)
            .WithCurrency1("EUR")
            .WithCurrency2("USD")
            .WithCategory(Category.Forex)
            .WithSymbol("EURUSD");


        apiHandlerMock.SetupSequence(api => api.GetTickPriceAsync(It.IsAny<string>()))
            .ReturnsAsync(new Tick { Bid = (decimal?)0.67288 })
            .ReturnsAsync(new Tick { Bid = (decimal?)1.65313 });

        apiHandlerMock.Setup(x => x.GetSymbolInformationAsync(It.IsAny<string>()))
            .ReturnsAsync(symbolInfo);


        apiHandlerMock.Setup(x => x.GetAllPositionsByCommentAsync("test")).ReturnsAsync(new List<Position>())
            .Verifiable();

        var accountBalance = new AccountBalance
        {
            Balance = 1000
        };

        apiHandlerMock.Setup(x => x.GetBalanceAsync()).ReturnsAsync(accountBalance);

        apiHandlerMock.Setup(x => x.GetAllSymbolsAsync()).ReturnsAsync(new List<string>
        {
            "EURUSD"
        });
        // Act
        var monyManagement =
            new RobotAppLibraryV2.MoneyManagement.MoneyManagement(apiHandlerMock.Object, "EURUSD", _logger.Object,
                "test");

        // Assert
        apiHandlerMock.Verify(x => x.SubscribePrice(It.IsAny<string>()), Times.Never);
        apiHandlerMock.Verify(x => x.GetAllPositionsByCommentAsync("test"), Times.Once);
        apiHandlerMock.Verify(x => x.GetBalanceAsync(), Times.Once);
        apiHandlerMock.Verify(x => x.GetSymbolInformationAsync("EURUSD"), Times.Exactly(2));
        apiHandlerMock.Verify(x => x.GetTickPriceAsync("EURUSD"), Times.Exactly(2));
        apiHandlerMock.Verify(x => x.GetAllSymbolsAsync(), Times.Never);
        monyManagement.PositionReference.Should().Be("test");
    }

    [Fact]
    public void test_Init_With_BaseSynbol_2()
    {
        // Arranage
        var apiHandlerMock = new Mock<IApiHandler>();
        var symbolInfo = new SymbolInfo()
            .WithLeverage(10)
            .WithTickSize(0.00001)
            .WithTickSize2(0.0001)
            .WithCurrency1("AUD")
            .WithCurrency2("CAD")
            .WithCategory(Category.Forex)
            .WithSymbol("AUDCAD");

        apiHandlerMock.SetupSequence(api => api.GetTickPriceAsync(It.IsAny<string>()))
            .ReturnsAsync(new Tick { Bid = (decimal?)0.67288 })
            .ReturnsAsync(new Tick { Bid = (decimal?)0.67288 })
            .ReturnsAsync(new Tick { Bid = (decimal?)1.65313 });

        apiHandlerMock.Setup(x => x.GetSymbolInformationAsync(It.IsAny<string>()))
            .ReturnsAsync(symbolInfo);

        var accountBalance = new AccountBalance
        {
            Balance = 1000
        };

        apiHandlerMock.Setup(x => x.GetBalanceAsync()).ReturnsAsync(accountBalance);

        apiHandlerMock.Setup(x => x.GetAllSymbolsAsync()).ReturnsAsync(new List<string>
        {
            "EURCAD"
        });
        // Act
        var _ =
            new RobotAppLibraryV2.MoneyManagement.MoneyManagement(apiHandlerMock.Object, "AUDCAD", _logger.Object, "");

        // Assert
        apiHandlerMock.Verify(x => x.SubscribePrice("EURCAD"), Times.Once);
    }


    [Fact]
    public void test_Init_With_BaseSynbol_Indices()
    {
        // Arranage
        var apiHandlerMock = new Mock<IApiHandler>();
        var symbolInfo = new SymbolInfo()
            .WithLeverage(10)
            .WithTickSize(0.00001)
            .WithTickSize2(0.0001)
            .WithCurrency1("EUR")
            .WithCurrency2("EUR")
            .WithContractSize(25)
            .WithCategory(Category.Indices)
            .WithSymbol("DE30");

        apiHandlerMock.SetupSequence(api => api.GetTickPriceAsync(It.IsAny<string>()))
            .ReturnsAsync(new Tick { Bid = (decimal?)0.67288 })
            .ReturnsAsync(new Tick { Bid = (decimal?)1.65313 });

        apiHandlerMock.Setup(x => x.GetSymbolInformationAsync(It.IsAny<string>()))
            .ReturnsAsync(symbolInfo);
        var accountBalance = new AccountBalance
        {
            Balance = 1000
        };


        apiHandlerMock.Setup(x => x.GetBalanceAsync()).ReturnsAsync(accountBalance);
        // Act
        _ = new RobotAppLibraryV2.MoneyManagement.MoneyManagement(apiHandlerMock.Object, "DE30", _logger.Object,
            "test");

        // Assert
        apiHandlerMock.Verify(x => x.SubscribePrice(It.IsAny<string>()), Times.Never);
        apiHandlerMock.Verify(x => x.GetAllPositionsByCommentAsync("test"), Times.Once);
        apiHandlerMock.Verify(x => x.GetBalanceAsync(), Times.Once);
        apiHandlerMock.Verify(x => x.GetSymbolInformationAsync(It.IsAny<string>()), Times.Exactly(2));
        apiHandlerMock.Verify(x => x.GetTickPriceAsync("DE30"), Times.Exactly(2));
    }

    [Fact]
    public void test_Init_With_BaseSynbol_Indices_not_eur()
    {
        // Arranage
        var apiHandlerMock = new Mock<IApiHandler>();
        var symbolInfo = new SymbolInfo()
            .WithLeverage(10)
            .WithTickSize(0.00001)
            .WithTickSize2(0.0001)
            .WithCurrency1("USD")
            .WithCurrency2("USD")
            .WithContractSize(25)
            .WithCategory(Category.Indices)
            .WithSymbol("US500");

        apiHandlerMock.SetupSequence(api => api.GetTickPriceAsync(It.IsAny<string>()))
            .ReturnsAsync(new Tick { Bid = (decimal?)0.67288 })
            .ReturnsAsync(new Tick { Bid = (decimal?)0.67288 })
            .ReturnsAsync(new Tick { Bid = (decimal?)1.65313 });

        apiHandlerMock.Setup(x => x.GetSymbolInformationAsync(It.IsAny<string>()))
            .ReturnsAsync(symbolInfo);

        var accountBalance = new AccountBalance
        {
            Balance = 1000
        };

        apiHandlerMock.Setup(x => x.GetAllSymbolsAsync()).ReturnsAsync(new List<string>
        {
            "EURUSD"
        });

        apiHandlerMock.Setup(x => x.GetBalanceAsync()).ReturnsAsync(accountBalance);
        // Act
        _ = new RobotAppLibraryV2.MoneyManagement.MoneyManagement(apiHandlerMock.Object, "US500", _logger.Object,
            "test");

        // Assert

        apiHandlerMock.Verify(x => x.GetAllPositionsByCommentAsync("test"), Times.Once);
        apiHandlerMock.Verify(x => x.GetBalanceAsync(), Times.Once);
        apiHandlerMock.Verify(x => x.GetSymbolInformationAsync("US500"), Times.Exactly(2));
        apiHandlerMock.Verify(x => x.GetTickPriceAsync("US500"), Times.Exactly(2));
        apiHandlerMock.Verify(x => x.SubscribePrice("EURUSD"), Times.Once);
    }

    #endregion


    #region Check perte risque treshold

    [Fact]
    public void
        CheckPerteRisqueTreshold_Should_Return_True_When_Profit_Is_Less_Than_Negative_Risk_Percentage_Of_Balance()
    {
        // Arrange
        var position = new Position { Profit = -101m };
        double balance = 1000;
        double risk = 10;
        var symbolInfo = new SymbolInfo()
            .WithLeverage(50)
            .WithSymbol("test")
            .WithCurrency1("test")
            .WithCurrency2("test")
            .WithTickSize(0.0001)
            .WithCategory(Category.Forex);

        var accountBalance = new AccountBalance
        {
            Balance = balance
        };

        var apiHandlerMock = new Mock<IApiHandler>();

        apiHandlerMock.Setup(x => x.GetBalanceAsync()).ReturnsAsync(accountBalance);

        apiHandlerMock.Setup(api => api.GetTickPriceAsync(It.IsAny<string>()))
            .ReturnsAsync(new Tick { Bid = (decimal?)1.11247 });

        apiHandlerMock.Setup(x => x.GetSymbolInformationAsync(It.IsAny<string>()))
            .ReturnsAsync(symbolInfo);

        apiHandlerMock.Setup(x => x.GetAllSymbolsAsync()).ReturnsAsync(new List<string>
        {
            "EURtest"
        });


        var moneyManagement =
            new RobotAppLibraryV2.MoneyManagement.MoneyManagement(apiHandlerMock.Object, "EURUSD", _logger.Object, "");

        moneyManagement.Risque = risk;

        // Act
        var result = moneyManagement.CheckPerteRisqueTreshold(position);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void
        CheckPerteRisqueTreshold_Should_Return_False_When_Profit_Is_Greater_Than_Negative_Risk_Percentage_Of_Balance()
    {
        // Arrange
        var position = new Position { Profit = -99m };
        double balance = 1000;
        double risk = 10;
        var symbolInfo = new SymbolInfo()
            .WithLeverage(50)
            .WithSymbol("test")
            .WithCurrency1("test")
            .WithCurrency2("test")
            .WithTickSize(0.0001)
            .WithCategory(Category.Forex);

        var accountBalance = new AccountBalance
        {
            Balance = balance
        };

        var apiHandlerMock = new Mock<IApiHandler>();

        apiHandlerMock.Setup(x => x.GetBalanceAsync()).ReturnsAsync(accountBalance);

        apiHandlerMock.Setup(api => api.GetTickPriceAsync(It.IsAny<string>()))
            .ReturnsAsync(new Tick { Bid = (decimal?)1.11247 });

        apiHandlerMock.Setup(x => x.GetSymbolInformationAsync(It.IsAny<string>()))
            .ReturnsAsync(symbolInfo);

        apiHandlerMock.Setup(x => x.GetAllSymbolsAsync()).ReturnsAsync(new List<string>
        {
            "EURtest"
        });


        var moneyManagement =
            new RobotAppLibraryV2.MoneyManagement.MoneyManagement(apiHandlerMock.Object, "EURUSD", _logger.Object, "");

        moneyManagement.Risque = risk;

        // Act
        var result = moneyManagement.CheckPerteRisqueTreshold(position);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CheckPerteRisqueTreshold_Should_Return_False_When_Profit_Is_Positive()
    {
        // Arrange
        var position = new Position { Profit = 50m };
        double balance = 1000;
        double risk = 10;
        var symbolInfo = new SymbolInfo()
            .WithLeverage(50)
            .WithSymbol("test")
            .WithCurrency1("test")
            .WithCurrency2("test")
            .WithTickSize(0.0001)
            .WithCategory(Category.Forex);

        var accountBalance = new AccountBalance
        {
            Balance = balance
        };

        var apiHandlerMock = new Mock<IApiHandler>();

        apiHandlerMock.Setup(x => x.GetBalanceAsync()).ReturnsAsync(accountBalance);

        apiHandlerMock.Setup(api => api.GetTickPriceAsync(It.IsAny<string>()))
            .ReturnsAsync(new Tick { Bid = (decimal?)1.11247 });

        apiHandlerMock.Setup(x => x.GetSymbolInformationAsync(It.IsAny<string>()))
            .ReturnsAsync(symbolInfo);

        apiHandlerMock.Setup(x => x.GetAllSymbolsAsync()).ReturnsAsync(new List<string>
        {
            "EURtest"
        });

        var moneyManagement =
            new RobotAppLibraryV2.MoneyManagement.MoneyManagement(apiHandlerMock.Object, "EURUSD", _logger.Object, "");

        moneyManagement.Risque = risk;
        // Act
        var result = moneyManagement.CheckPerteRisqueTreshold(position);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Check loose streak threshold

    [Fact]
    public void CheckLooseStreakTreshold_Should_Return_True_When_All_Last_Positions_Are_Losses()
    {
        // Arrange
        var apihandlerMock = new Mock<IApiHandler>();
        var symbolInfo = new SymbolInfo()
                .WithLeverage(10)
                .WithTickSize(0.00001)
                .WithTickSize2(0.0001)
                .WithCurrency1("EUR")
                .WithCurrency2("USD")
                .WithCategory(Category.Forex)
                .WithSymbol("EURUSD")
            ;
        apihandlerMock.Setup(x => x.GetSymbolInformationAsync(It.IsAny<string>()))
            .ReturnsAsync(symbolInfo);
        apihandlerMock.Setup(x => x.GetAllPositionsByCommentAsync(It.IsAny<string>()))
            .ReturnsAsync(new List<Position>
            {
                new() { Profit = -10m },
                new() { Profit = -20m },
                new() { Profit = -15m },
                new() { Profit = -10m },
                new() { Profit = -5m }
            });

        var accountBalance = new AccountBalance
        {
            Balance = 1000
        };

        apihandlerMock.Setup(x => x.GetBalanceAsync()).ReturnsAsync(accountBalance);
        var moneyManagement =
            new RobotAppLibraryV2.MoneyManagement.MoneyManagement(apihandlerMock.Object, "EURUSD", _logger.Object,
                "test");

        moneyManagement.LooseStreak = 5;

        // Act
        var result = moneyManagement.CheckLooseStreakTreshold();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void CheckLooseStreakTreshold_Should_Return_False_When_Not_All_Last_Positions_Are_Losses()
    {
        // Arrange
        var looseStreak = 5;
        var apihandlerMock = new Mock<IApiHandler>();
        var symbolInfo = new SymbolInfo()
            .WithLeverage(10)
            .WithTickSize(0.00001)
            .WithTickSize2(0.0001)
            .WithCurrency1("EUR")
            .WithCurrency2("USD")
            .WithCategory(Category.Forex)
            .WithSymbol("AUDUSD");
        apihandlerMock.Setup(x => x.GetSymbolInformationAsync(It.IsAny<string>()))
            .ReturnsAsync(symbolInfo);
        apihandlerMock.Setup(x => x.GetAllPositionsByCommentAsync(It.IsAny<string>()))
            .ReturnsAsync(new List<Position>
            {
                new() { Profit = -10m },
                new() { Profit = 20m },
                new() { Profit = -15m },
                new() { Profit = -10m },
                new() { Profit = -5m }
            });

        var accountBalance = new AccountBalance
        {
            Balance = 1000
        };

        apihandlerMock.Setup(x => x.GetAllSymbolsAsync()).ReturnsAsync(new List<string>
        {
            "EURUSD"
        });


        apihandlerMock.Setup(x => x.GetBalanceAsync()).ReturnsAsync(accountBalance);
        var moneyManagement =
            new RobotAppLibraryV2.MoneyManagement.MoneyManagement(apihandlerMock.Object, "EURUSD", _logger.Object,
                "test");
        moneyManagement.LooseStreak = looseStreak;
        // Act
        var result = moneyManagement.CheckLooseStreakTreshold();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CheckLooseStreakTreshold_Should_Return_False_When_Not_Enough_Closed_Positions()
    {
        // Arrange
        var looseStreak = 5;
        var apihandlerMock = new Mock<IApiHandler>();
        var symbolInfo = new SymbolInfo()
                .WithLeverage(10)
                .WithTickSize(0.00001)
                .WithTickSize2(0.0001)
                .WithCurrency1("EUR")
                .WithCurrency2("USD")
                .WithCategory(Category.Forex)
                .WithSymbol("EURUSD")
            ;
        apihandlerMock.Setup(x => x.GetSymbolInformationAsync(It.IsAny<string>()))
            .ReturnsAsync(symbolInfo);
        apihandlerMock.Setup(x => x.GetAllPositionsByCommentAsync(It.IsAny<string>()))
            .ReturnsAsync(new List<Position>
            {
                new() { Profit = -10m },
                new() { Profit = -15m },
                new() { Profit = -10m },
                new() { Profit = -5m }
            });
        var accountBalance = new AccountBalance
        {
            Balance = 1000
        };

        apihandlerMock.Setup(x => x.GetBalanceAsync()).ReturnsAsync(accountBalance);
        var moneyManagement =
            new RobotAppLibraryV2.MoneyManagement.MoneyManagement(apihandlerMock.Object, "EURUSD", _logger.Object,
                "test");
        moneyManagement.LooseStreak = looseStreak;
        // Act
        var result = moneyManagement.CheckLooseStreakTreshold();

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Check draw down treshold

    [Fact]
    public void CheckDrawnDownTreshold_Should_Return_True_When_CurrentDrawdown_Exceeds_ToleratedDrawdown()
    {
        // Arrange

        double balance = 1000;
        double toleratedDrawdown = 3;
        var symbolInfo = new SymbolInfo()
                .WithLeverage(50)
                .WithSymbol("test")
                .WithCurrency1("test")
                .WithCurrency2("test")
                .WithTickSize(0.0001)
                .WithCategory(Category.Forex)
            ;

        var accountBalance = new AccountBalance
        {
            Balance = balance
        };

        var apiHandlerMock = new Mock<IApiHandler>();


        apiHandlerMock.Setup(x => x.GetBalanceAsync()).ReturnsAsync(accountBalance);

        apiHandlerMock.Setup(api => api.GetTickPriceAsync(It.IsAny<string>()))
            .ReturnsAsync(new Tick { Bid = (decimal?)1.11247 });

        apiHandlerMock.Setup(x => x.GetSymbolInformationAsync(It.IsAny<string>()))
            .ReturnsAsync(symbolInfo);

        apiHandlerMock.Setup(x => x.GetAllPositionsByCommentAsync(It.IsAny<string>()))
            .ReturnsAsync(new List<Position>
            {
                new() { Profit = 10, DateClose = new DateTime(2022, 01, 01) },
                new() { Profit = 20, DateClose = new DateTime(2022, 01, 02) },
                new() { Profit = 30, DateClose = new DateTime(2022, 01, 03) },
                new() { Profit = -100, DateClose = new DateTime(2022, 01, 04) }
            });

        apiHandlerMock.Setup(x => x.GetAllSymbolsAsync()).ReturnsAsync(new List<string>
        {
            "EURtest"
        });

        var moneyManagement =
            new RobotAppLibraryV2.MoneyManagement.MoneyManagement(apiHandlerMock.Object, "EURUSD", _logger.Object, "");
        moneyManagement.ToleratedDrawnDown = toleratedDrawdown;
        // Act
        var result = moneyManagement.CheckDrawnDownTreshold();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void CheckDrawnDownTreshold_Should_Return_False_When_CurrentDrawdown_Less_Than_ToleratedDrawdown()
    {
        // Arrange
        var currentDrawdown = 20m;
        double balance = 1000;
        double toleratedDrawdown = 3;
        var symbolInfo = new SymbolInfo()
                .WithLeverage(50)
                .WithSymbol("test")
                .WithCurrency1("test")
                .WithCurrency2("test")
                .WithTickSize(0.0001)
                .WithCategory(Category.Forex)
            ;

        var accountBalance = new AccountBalance
        {
            Balance = balance
        };

        var apiHandlerMock = new Mock<IApiHandler>();

        apiHandlerMock.Setup(x => x.GetBalanceAsync()).ReturnsAsync(accountBalance);

        apiHandlerMock.Setup(api => api.GetTickPriceAsync(It.IsAny<string>()))
            .ReturnsAsync(new Tick { Bid = (decimal?)1.11247 });

        apiHandlerMock.Setup(x => x.GetSymbolInformationAsync(It.IsAny<string>()))
            .ReturnsAsync(symbolInfo);

        apiHandlerMock.Setup(x => x.GetAllPositionsByCommentAsync(It.IsAny<string>()))
            .ReturnsAsync(new List<Position>
            {
                new() { Profit = 1, DateClose = new DateTime(2022, 01, 01) }
            });

        apiHandlerMock.Setup(x => x.GetAllSymbolsAsync()).ReturnsAsync(new List<string>
        {
            "EURtest"
        });

        var moneyManagement =
            new RobotAppLibraryV2.MoneyManagement.MoneyManagement(apiHandlerMock.Object, "EURUSD", _logger.Object, "");
        moneyManagement.ToleratedDrawnDown = toleratedDrawdown;
        // Act
        var result = moneyManagement.CheckDrawnDownTreshold();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CheckDrawnDownTreshold_Should_Return_False_When_CurrentDrawdown_Equals_Zero()
    {
        // Arrange
        var currentDrawdown = 0m;
        double balance = 1000;
        double toleratedDrawdown = 3;

        var symbolInfo = new SymbolInfo()
                .WithLeverage(50)
                .WithSymbol("test")
                .WithCurrency1("test")
                .WithCurrency2("test")
                .WithTickSize(0.0001)
                .WithCategory(Category.Forex)
            ;

        var accountBalance = new AccountBalance
        {
            Balance = balance
        };

        var apiHandlerMock = new Mock<IApiHandler>();

        apiHandlerMock.Setup(x => x.GetBalanceAsync()).ReturnsAsync(accountBalance);

        apiHandlerMock.Setup(api => api.GetTickPriceAsync(It.IsAny<string>()))
            .ReturnsAsync(new Tick { Bid = (decimal?)1.11247 });

        apiHandlerMock.Setup(x => x.GetSymbolInformationAsync(It.IsAny<string>()))
            .ReturnsAsync(symbolInfo);

        apiHandlerMock.Setup(x => x.GetAllSymbolsAsync()).ReturnsAsync(new List<string>
        {
            "EURtest"
        });

        apiHandlerMock.Setup(x => x.GetAllPositionsByCommentAsync(It.IsAny<string>()))
            .ReturnsAsync(new List<Position>
            {
                new() { Profit = 1, DateClose = new DateTime(2022, 01, 01) }
            });


        var moneyManagement =
            new RobotAppLibraryV2.MoneyManagement.MoneyManagement(apiHandlerMock.Object, "EURUSD", _logger.Object, "");
        moneyManagement.ToleratedDrawnDown = toleratedDrawdown;
        // Act
        var result = moneyManagement.CheckDrawnDownTreshold();

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Check profit factor treshold

    [Fact]
    public void
        CheckProfitFactorTreshold_Should_Return_True_When_ProfitFactor_Is_Less_Than_Or_Equals_One_And_Greater_Than_Zero()
    {
        // Arrange
        var profitFactor = 0.5m;

        var apihandlerMock = new Mock<IApiHandler>();
        var symbolInfo = new SymbolInfo()
                .WithLeverage(10)
                .WithTickSize(0.00001)
                .WithTickSize2(0.0001)
                .WithCurrency1("EUR")
                .WithCurrency2("USD")
                .WithCategory(Category.Forex)
                .WithSymbol("EURUSD")
            ;
        apihandlerMock.Setup(x => x.GetSymbolInformationAsync(It.IsAny<string>()))
            .ReturnsAsync(symbolInfo);

        var positions = new List<Position>
        {
            new() { Profit = -200 },
            new() { Profit = 100 }
        };

        apihandlerMock.Setup(x => x.GetAllPositionsByCommentAsync(It.IsAny<string>()))
            .ReturnsAsync(positions);

        var accountBalance = new AccountBalance
        {
            Balance = 1000
        };

        apihandlerMock.Setup(x => x.GetBalanceAsync()).ReturnsAsync(accountBalance);
        var moneyManagement =
            new RobotAppLibraryV2.MoneyManagement.MoneyManagement(apihandlerMock.Object, "EURUSD", _logger.Object,
                "test");
        // Act
        var result = moneyManagement.CheckProfitFactorTreshold();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void CheckProfitFactorTreshold_Should_Return_False_When_ProfitFactor_Is_Greater_Than_One()
    {
        // Arrange
        var profitFactor = 1.5m;
        var apihandlerMock = new Mock<IApiHandler>();
        var symbolInfo = new SymbolInfo()
                .WithLeverage(10)
                .WithTickSize(0.00001)
                .WithTickSize2(0.0001)
                .WithCurrency1("EUR")
                .WithCurrency2("USD")
                .WithCategory(Category.Forex)
                .WithSymbol("EURUSD")
            ;
        apihandlerMock.Setup(x => x.GetSymbolInformationAsync(It.IsAny<string>()))
            .ReturnsAsync(symbolInfo);

        var positions = new List<Position>
        {
            new() { Profit = -200 },
            new() { Profit = 100 },
            new() { Profit = 100 },
            new() { Profit = 100 }
        };

        apihandlerMock.Setup(x => x.GetAllPositionsByCommentAsync(It.IsAny<string>()))
            .ReturnsAsync(positions);

        var accountBalance = new AccountBalance
        {
            Balance = 1000
        };

        apihandlerMock.Setup(x => x.GetBalanceAsync()).ReturnsAsync(accountBalance);
        var moneyManagement =
            new RobotAppLibraryV2.MoneyManagement.MoneyManagement(apihandlerMock.Object, "EURUSD", _logger.Object,
                "test");

        // Act
        var result = moneyManagement.CheckProfitFactorTreshold();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CheckProfitFactorTreshold_Should_Return_False_When_ProfitFactor_Is_Less_Than_Or_Equals_Zero()
    {
        // Arrange
        var profitFactor = 0m;
        var apihandlerMock = new Mock<IApiHandler>();
        var symbolInfo = new SymbolInfo()
                .WithLeverage(10)
                .WithTickSize(0.00001)
                .WithTickSize2(0.0001)
                .WithCurrency1("EUR")
                .WithCurrency2("USD")
                .WithCategory(Category.Forex)
                .WithSymbol("EURUSD")
            ;
        apihandlerMock.Setup(x => x.GetSymbolInformationAsync(It.IsAny<string>()))
            .ReturnsAsync(symbolInfo);
        var accountBalance = new AccountBalance
        {
            Balance = 1000
        };

        apihandlerMock.Setup(x => x.GetBalanceAsync()).ReturnsAsync(accountBalance);
        var moneyManagement =
            new RobotAppLibraryV2.MoneyManagement.MoneyManagement(apihandlerMock.Object, "EURUSD", _logger.Object,
                "test");

        var positions = new List<Position>
        {
            new() { Profit = 0 }
        };

        apihandlerMock.Setup(x => x.GetAllPositionsByCommentAsync(It.IsAny<string>()))
            .ReturnsAsync(positions);

        // Act
        var result = moneyManagement.CheckProfitFactorTreshold();

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Update max lot

    [Fact]
    public void UpdateMaxLot_Should_Calculate_Correctly_forex()
    {
        // Arrange
        var symbolInfo = new SymbolInfo()
                .WithLeverage(3.33)
                .WithSymbol("EURUSD")
                .WithCurrency1("EUR")
                .WithCurrency2("USD")
                .WithTickSize(0.00001)
                .WithTickSize2(0.0001)
                .WithCategory(Category.Forex)
            ;

        var accountBalance = new AccountBalance
        {
            Balance = 10000
        };

        var apiHandlerMock = new Mock<IApiHandler>();

        apiHandlerMock.Setup(x => x.GetBalanceAsync()).ReturnsAsync(accountBalance);

        apiHandlerMock.Setup(api => api.GetTickPriceAsync(It.IsAny<string>()))
            .ReturnsAsync(new Tick { Bid = (decimal?)1.09755 });

        apiHandlerMock.Setup(x => x.GetSymbolInformationAsync(It.IsAny<string>()))
            .ReturnsAsync(symbolInfo);

        // Act
        var moneyManagement =
            new RobotAppLibraryV2.MoneyManagement.MoneyManagement(apiHandlerMock.Object, "EURUSD", _logger.Object, "");


        // assert
        moneyManagement.MaxLot.Should().BeApproximately(2.74, 0.02);
    }

    [Fact]
    public void UpdateMaxLot_Should_Calculate_Correctly_forex_new_balance_received()
    {
        // Arrange
        var symbolInfo = new SymbolInfo()
                .WithLeverage(3.33)
                .WithSymbol("EURUSD")
                .WithCurrency1("EUR")
                .WithCurrency2("USD")
                .WithTickSize(0.00001)
                .WithTickSize2(0.0001)
                .WithCategory(Category.Forex)
            ;

        var accountBalance = new AccountBalance
        {
            Balance = 10000
        };

        var apiHandlerMock = new Mock<IApiHandler>();

        apiHandlerMock.Setup(x => x.GetBalanceAsync()).ReturnsAsync(accountBalance);

        apiHandlerMock.Setup(api => api.GetTickPriceAsync(It.IsAny<string>()))
            .ReturnsAsync(new Tick { Bid = (decimal?)1.09755 });

        apiHandlerMock.Setup(x => x.GetSymbolInformationAsync(It.IsAny<string>()))
            .ReturnsAsync(symbolInfo);

        // Act
        var moneyManagement =
            new RobotAppLibraryV2.MoneyManagement.MoneyManagement(apiHandlerMock.Object, "EURUSD", _logger.Object, "");

        var accountBalance2 = new AccountBalance
        {
            Balance = 20000
        };

        apiHandlerMock.Raise(x => x.NewBalanceEvent += null, this, accountBalance2);

        // assert
        moneyManagement.MaxLot.Should().BeApproximately(5.47, 0.02);
    }

    [Fact]
    public void UpdateMaxLot_Should_Return_Zero_When_Leverage_Is_Zero_forex()
    {
        // Arrange
        var symbolInfo = new SymbolInfo()
                .WithLeverage(0)
                .WithSymbol("test")
                .WithCurrency1("test")
                .WithCurrency2("test")
                .WithTickSize(0.0001)
                .WithCategory(Category.Forex)
            ;

        var accountBalance = new AccountBalance
        {
            Balance = 200000
        };

        var apiHandlerMock = new Mock<IApiHandler>();

        apiHandlerMock.Setup(x => x.GetBalanceAsync()).ReturnsAsync(accountBalance);

        apiHandlerMock.Setup(api => api.GetTickPriceAsync(It.IsAny<string>()))
            .ReturnsAsync(new Tick { Bid = (decimal?)1.11247 });

        apiHandlerMock.Setup(x => x.GetSymbolInformationAsync(It.IsAny<string>()))
            .ReturnsAsync(symbolInfo);

        apiHandlerMock.Setup(x => x.GetAllSymbolsAsync()).ReturnsAsync(new List<string>
        {
            "EURtest"
        });

        var moneyManagement =
            new RobotAppLibraryV2.MoneyManagement.MoneyManagement(apiHandlerMock.Object, "EURUSD", _logger.Object, "");

        // Act && assert
        moneyManagement.MaxLot.Should().Be(1.8);
    }


    [Fact]
    public void UpdateMaxLot_Should_Calculate_Correctly_indices()
    {
        // Arrange
        var symbolInfo = new SymbolInfo()
                .WithLeverage(5)
                .WithSymbol("DE30")
                .WithCurrency1("EUR")
                .WithCurrency2("EUR")
                .WithContractSize(25)
                .WithTickSize(0.1)
                .WithTickSize2(0.1)
                .WithCategory(Category.Indices)
            ;

        var accountBalance = new AccountBalance
        {
            Balance = 10000
        };

        var apiHandlerMock = new Mock<IApiHandler>();

        apiHandlerMock.Setup(x => x.GetBalanceAsync()).ReturnsAsync(accountBalance);

        apiHandlerMock.Setup(api => api.GetTickPriceAsync(It.IsAny<string>()))
            .ReturnsAsync(new Tick { Bid = 16545.8m });

        apiHandlerMock.Setup(x => x.GetSymbolInformationAsync(It.IsAny<string>()))
            .ReturnsAsync(symbolInfo);


        var moneyManagement =
            new RobotAppLibraryV2.MoneyManagement.MoneyManagement(apiHandlerMock.Object, "DE30", _logger.Object, "");


        // Act && assert
        moneyManagement.MaxLot.Should().BeApproximately(0.48, 0.02);
    }

    [Fact]
    public void UpdateMaxLot_Should_Calculate_Correctly_indices_new_balance()
    {
        // Arrange
        var symbolInfo = new SymbolInfo()
                .WithLeverage(5)
                .WithSymbol("DE30")
                .WithCurrency1("EUR")
                .WithCurrency2("EUR")
                .WithContractSize(25)
                .WithTickSize(0.1)
                .WithTickSize2(0.1)
                .WithCategory(Category.Indices)
            ;

        var accountBalance = new AccountBalance
        {
            Balance = 10000
        };

        var apiHandlerMock = new Mock<IApiHandler>();

        apiHandlerMock.Setup(x => x.GetBalanceAsync()).ReturnsAsync(accountBalance);

        apiHandlerMock.Setup(api => api.GetTickPriceAsync(It.IsAny<string>()))
            .ReturnsAsync(new Tick { Bid = 16545.8m });

        apiHandlerMock.Setup(x => x.GetSymbolInformationAsync(It.IsAny<string>()))
            .ReturnsAsync(symbolInfo);

        // Act

        var moneyManagement =
            new RobotAppLibraryV2.MoneyManagement.MoneyManagement(apiHandlerMock.Object, "DE30", _logger.Object, "");

        var accountBalance2 = new AccountBalance
        {
            Balance = 20000
        };

        apiHandlerMock.Raise(x => x.NewBalanceEvent += null, this, accountBalance2);

        //  assert
        moneyManagement.MaxLot.Should().BeApproximately(0.97, 0.02);
    }

    [Fact]
    public void UpdateMaxLot_Should_Return_Zero_When_Leverage_Is_Zero_indices()
    {
        // Arrange
        var symbolInfo = new SymbolInfo()
                .WithLeverage(0)
                .WithSymbol("DE30")
                .WithCurrency1("EUR")
                .WithCurrency2("EUR")
                .WithContractSize(25)
                .WithTickSize(0.1)
                .WithTickSize2(0.1)
                .WithCategory(Category.Indices)
            ;

        var accountBalance = new AccountBalance
        {
            Balance = 10000
        };

        var apiHandlerMock = new Mock<IApiHandler>();

        apiHandlerMock.Setup(x => x.GetBalanceAsync()).ReturnsAsync(accountBalance);

        apiHandlerMock.Setup(api => api.GetTickPriceAsync(It.IsAny<string>()))
            .ReturnsAsync(new Tick { Bid = 16545.8m });

        apiHandlerMock.Setup(x => x.GetSymbolInformationAsync(It.IsAny<string>()))
            .ReturnsAsync(symbolInfo);


        var moneyManagement =
            new RobotAppLibraryV2.MoneyManagement.MoneyManagement(apiHandlerMock.Object, "DE30", _logger.Object, "");


        // Act && assert
        moneyManagement.MaxLot.Should().BeApproximately(0.02, 0.02);
    }

    #endregion

    #region LotValue

    [Fact]
    public void Test_LotValue()
    {
        // Arrange 
        var symbolInfo = new SymbolInfo()
                .WithLeverage(10)
                .WithTickSize(0.00001)
                .WithTickSize2(0.0001)
                .WithCurrency1("EUR")
                .WithCurrency2("USD")
                .WithSymbol("EURUSD")
                .WithCategory(Category.Forex)
            ;

        var apiHandlerMock = new Mock<IApiHandler>();


        apiHandlerMock.Setup(api => api.GetTickPriceAsync(It.IsAny<string>()))
            .ReturnsAsync(new Tick { Bid = (decimal?)1.11247 });

        apiHandlerMock.Setup(x => x.GetSymbolInformationAsync(It.IsAny<string>()))
            .ReturnsAsync(symbolInfo);

        var accountBalance = new AccountBalance
        {
            Balance = 1000
        };

        apiHandlerMock.Setup(x => x.GetBalanceAsync()).ReturnsAsync(accountBalance);

        var moneyManagement =
            new RobotAppLibraryV2.MoneyManagement.MoneyManagement(apiHandlerMock.Object, "EURUSD", _logger.Object,
                "test");

        // act and assert
        moneyManagement.LotValueCalculator.LotValueStandard.Should().BeApproximately(8.99, 0.02);
    }

    [Fact]
    public void Test_LotValue_OnNewTick()
    {
        // Arrange
        var symbolInfo = new SymbolInfo()
                .WithLeverage(10)
                .WithTickSize2(0.0001)
                .WithTickSize(0.00001)
                .WithCurrency1("EUR")
                .WithCurrency2("USD")
                .WithSymbol("EURUSD")
                .WithCategory(Category.Forex)
            ;

        var apiHandlerMock = new Mock<IApiHandler>();


        apiHandlerMock.Setup(api => api.GetTickPriceAsync(It.IsAny<string>()))
            .ReturnsAsync(new Tick { Bid = (decimal?)1.11247 });

        apiHandlerMock.Setup(x => x.GetSymbolInformationAsync(It.IsAny<string>()))
            .ReturnsAsync(symbolInfo);
        var tick = new Tick { Bid = (decimal?)1.11447, Symbol = "EURUSD" };
        var accountBalance = new AccountBalance
        {
            Balance = 1000
        };

        apiHandlerMock.Setup(x => x.GetBalanceAsync()).ReturnsAsync(accountBalance);
        var moneyManagement =
            new RobotAppLibraryV2.MoneyManagement.MoneyManagement(apiHandlerMock.Object, "EURUSD", _logger.Object,
                "test");

        // Act
        apiHandlerMock.Raise(x => x.TickEvent += null, this, tick);

        // Assert
        moneyManagement.LotValueCalculator.LotValueStandard.Should().BeApproximately(8.99, 0.02);
    }

    #endregion

    #region lot value case not eur

    [Fact]
    public void Test_LotValue_BaseNotEur_usd()
    {
        // Arranage
        var symbolInfo = new SymbolInfo()
                .WithLeverage(10)
                .WithTickSize(0.00001)
                .WithTickSize2(0.0001)
                .WithCurrency1("AUD")
                .WithCurrency2("USD")
                .WithSymbol("AUDUSD")
                .WithCategory(Category.Forex)
            ;

        var apiHandlerMock = new Mock<IApiHandler>();

        apiHandlerMock.SetupSequence(api => api.GetTickPriceAsync(It.IsAny<string>()))
            .ReturnsAsync(new Tick { Bid = (decimal?)1.45737 })
            .ReturnsAsync(new Tick { Bid = (decimal?)1.1076 })
            .ReturnsAsync(new Tick { Bid = (decimal?)1.45737 });

        apiHandlerMock.Setup(x => x.GetSymbolInformationAsync(It.IsAny<string>()))
            .ReturnsAsync(symbolInfo);

        apiHandlerMock.Setup(x => x.GetAllSymbolsAsync()).ReturnsAsync(new List<string>
        {
            "EURUSD"
        });

        var accountBalance = new AccountBalance
        {
            Balance = 1000
        };

        apiHandlerMock.Setup(x => x.GetBalanceAsync()).ReturnsAsync(accountBalance);
        // Act
        var monyManagement =
            new RobotAppLibraryV2.MoneyManagement.MoneyManagement(apiHandlerMock.Object, "AUDUSD", _logger.Object, "");

        // Assert
        monyManagement.LotValueCalculator.LotValueStandard.Should().BeApproximately(9.02, 0.02);
    }

    [Fact]
    public void Test_LotValue_BaseNotEur_NotUsd()
    {
        // Arranage
        var symbolInfo = new SymbolInfo()
                .WithLeverage(10)
                .WithTickSize(0.00001)
                .WithTickSize2(0.0001)
                .WithCurrency1("AUD")
                .WithCurrency2("CAD")
                .WithSymbol("AUDCAD")
                .WithCategory(Category.Forex)
            ;

        var apiHandlerMock = new Mock<IApiHandler>();

        apiHandlerMock.SetupSequence(api => api.GetTickPriceAsync(It.IsAny<string>()))
            .ReturnsAsync(new Tick { Bid = (decimal?)0.88768 })
            .ReturnsAsync(new Tick { Bid = (decimal?)1.45737 })
            .ReturnsAsync(new Tick { Bid = (decimal?)0.88768 });

        apiHandlerMock.Setup(x => x.GetSymbolInformationAsync(It.IsAny<string>()))
            .ReturnsAsync(symbolInfo);

        apiHandlerMock.Setup(x => x.GetAllSymbolsAsync()).ReturnsAsync(new List<string>
        {
            "EURCAD"
        });


        var accountBalance = new AccountBalance
        {
            Balance = 1000
        };

        apiHandlerMock.Setup(x => x.GetBalanceAsync()).ReturnsAsync(accountBalance);

        // Act
        var monyManagement =
            new RobotAppLibraryV2.MoneyManagement.MoneyManagement(apiHandlerMock.Object, "AUDCAD", _logger.Object, "");

        // Assert
        monyManagement.LotValueCalculator.LotValueStandard.Should().BeApproximately(6.86, 0.02);
    }


    [Fact]
    public void Test_LotValue_BaseNotEur_NewTick()
    {
        // Arranage


        var apiHandlerMock = new Mock<IApiHandler>();

        apiHandlerMock.SetupSequence(api => api.GetTickPriceAsync(It.IsAny<string>()))
            .ReturnsAsync(new Tick { Bid = (decimal?)0.67288 })
            .ReturnsAsync(new Tick { Bid = (decimal?)1.65313 });
        var symbolInfo = new SymbolInfo()
                .WithLeverage(10)
                .WithTickSize(0.00001)
                .WithTickSize2(0.0001)
                .WithCurrency1("AUD")
                .WithCurrency2("USD")
                .WithCategory(Category.Forex)
                .WithSymbol("AUDUSD")
            ;
        apiHandlerMock.Setup(x => x.GetSymbolInformationAsync(It.IsAny<string>()))
            .ReturnsAsync(symbolInfo);

        apiHandlerMock.Setup(x => x.GetAllSymbolsAsync()).ReturnsAsync(new List<string>
        {
            "EURUSD"
        });


        var tick = new Tick { Bid = (decimal?)0.67788, Symbol = "AUDUSD" };

        var tick2 = new Tick { Bid = (decimal?)1.10802, Symbol = "EURUSD" };

        var accountBalance = new AccountBalance
        {
            Balance = 1000
        };

        apiHandlerMock.Setup(x => x.GetBalanceAsync()).ReturnsAsync(accountBalance);
        apiHandlerMock.Setup(x => x.GetTickPriceAsync(It.IsAny<string>()));
        // Act
        var monyManagement =
            new RobotAppLibraryV2.MoneyManagement.MoneyManagement(apiHandlerMock.Object, "AUDUSD", _logger.Object, "");
        apiHandlerMock.Raise(x => x.TickEvent += null, this, tick);
        apiHandlerMock.Raise(x => x.TickEvent += null, this, tick2);


        // Assert
        monyManagement.LotValueCalculator.LotValueStandard.Should().BeApproximately(9.02, 0.02);
    }

    #endregion

    #region Lot value case yen

    [Fact]
    public void Test_LotValue_BaseYen_Eur()
    {
        // Arranage
        var symbolInfo = new SymbolInfo()
                .WithLeverage(10)
                .WithTickSize(0.001)
                .WithTickSize2(0.01)
                .WithCurrency1("EUR")
                .WithCurrency2("JPY")
                .WithSymbol("EURJPY")
                .WithCategory(Category.Forex)
            ;

        var apiHandlerMock = new Mock<IApiHandler>();

        apiHandlerMock.SetupSequence(api => api.GetTickPriceAsync(It.IsAny<string>()))
            .ReturnsAsync(new Tick { Bid = (decimal?)156.61 })
            .ReturnsAsync(new Tick { Bid = (decimal?)156.61 });

        apiHandlerMock.Setup(x => x.GetSymbolInformationAsync(It.IsAny<string>()))
            .ReturnsAsync(symbolInfo);
        var accountBalance = new AccountBalance
        {
            Balance = 1000
        };

        apiHandlerMock.Setup(x => x.GetAllSymbolsAsync()).ReturnsAsync(new List<string>
        {
            "EURUSD"
        });

        apiHandlerMock.Setup(x => x.GetBalanceAsync()).ReturnsAsync(accountBalance);
        // Act
        var monyManagement =
            new RobotAppLibraryV2.MoneyManagement.MoneyManagement(apiHandlerMock.Object, "EURJPY", _logger.Object, "");

        // Assert
        monyManagement.LotValueCalculator.LotValueStandard.Should().BeApproximately(6.38, 0.02);
    }


    [Fact]
    public void Test_LotValue_BaseYen_NotEur()
    {
        // Arranage
        var symbolInfo = new SymbolInfo()
                .WithLeverage(10)
                .WithTickSize(0.001)
                .WithTickSize2(0.01)
                .WithCurrency1("USD")
                .WithCurrency2("JPY")
                .WithSymbol("USDJPY")
                .WithCategory(Category.Forex)
            ;

        var apiHandlerMock = new Mock<IApiHandler>();

        apiHandlerMock.SetupSequence(api => api.GetTickPriceAsync(It.IsAny<string>()))
            .ReturnsAsync(new Tick { Bid = (decimal?)141.466 })
            .ReturnsAsync(new Tick { Bid = (decimal?)156.61 })
            .ReturnsAsync(new Tick { Bid = (decimal?)141.466 });

        apiHandlerMock.Setup(x => x.GetSymbolInformationAsync(It.IsAny<string>()))
            .ReturnsAsync(symbolInfo);

        apiHandlerMock.Setup(x => x.GetAllSymbolsAsync()).ReturnsAsync(new List<string>
        {
            "EURJPY"
        });

        var accountBalance = new AccountBalance
        {
            Balance = 1000
        };

        apiHandlerMock.Setup(x => x.GetBalanceAsync()).ReturnsAsync(accountBalance);

        // Act
        var monyManagement =
            new RobotAppLibraryV2.MoneyManagement.MoneyManagement(apiHandlerMock.Object, "USDJPY", _logger.Object, "");

        // Assert
        monyManagement.LotValueCalculator.LotValueStandard.Should().BeApproximately(6.38, 0.02);
    }

    #endregion

    #region Lot value indices

    [Fact]
    public void Test_LotValue_ind_eur()
    {
        // Arranage
        var symbolInfo = new SymbolInfo()
                .WithLeverage(10)
                .WithTickSize(0.1)
                .WithContractSize(25)
                .WithCurrency1("EUR")
                .WithCurrency2("EUR")
                .WithSymbol("DE30")
                .WithCategory(Category.Indices)
            ;

        var apiHandlerMock = new Mock<IApiHandler>();

        apiHandlerMock.Setup(api => api.GetTickPriceAsync(It.IsAny<string>()));

        apiHandlerMock.Setup(x => x.GetSymbolInformationAsync(It.IsAny<string>()))
            .ReturnsAsync(symbolInfo);
        var accountBalance = new AccountBalance
        {
            Balance = 1000
        };

        apiHandlerMock.Setup(x => x.GetBalanceAsync()).ReturnsAsync(accountBalance);
        // Act
        var monyManagement =
            new RobotAppLibraryV2.MoneyManagement.MoneyManagement(apiHandlerMock.Object, "DE30", _logger.Object,
                "test");

        // Assert
        monyManagement.LotValueCalculator.LotValueStandard.Should().Be(25);
    }


    [Fact]
    public void Test_LotValue_ind_not_eur()
    {
        // Arranage
        var symbolInfo = new SymbolInfo()
            .WithLeverage(10)
            .WithTickSize(0.1)
            .WithContractSize(50)
            .WithCurrency1("USD")
            .WithCurrency2("USD")
            .WithSymbol("US500")
            .WithCategory(Category.Indices);

        var apiHandlerMock = new Mock<IApiHandler>();

        apiHandlerMock.SetupSequence(api => api.GetTickPriceAsync(It.IsAny<string>()))
            .ReturnsAsync(new Tick { Bid = 4410.9m })
            .ReturnsAsync(new Tick { Bid = 1.10395m })
            .ReturnsAsync(new Tick { Bid = 4410.9m });

        apiHandlerMock.Setup(x => x.GetAllSymbolsAsync()).ReturnsAsync(new List<string>
        {
            "EURUSD"
        });

        apiHandlerMock.Setup(x => x.GetSymbolInformationAsync(It.IsAny<string>()))
            .ReturnsAsync(symbolInfo);
        var accountBalance = new AccountBalance
        {
            Balance = 1000
        };

        apiHandlerMock.Setup(x => x.GetBalanceAsync()).ReturnsAsync(accountBalance);

        // Act
        var monyManagement =
            new RobotAppLibraryV2.MoneyManagement.MoneyManagement(apiHandlerMock.Object, "US500", _logger.Object,
                "test");

        // Assert
        monyManagement.LotValueCalculator.LotValueStandard.Should().BeApproximately(45.29, 0.02);
    }

    #endregion

    #region Position size

    [Fact]
    public void Test_Position_Size_4decimal_baseEur()
    {
        // Arranage
        var symbolInfo = new SymbolInfo()
                .WithLeverage(3.33)
                .WithTickSize(0.00001)
                .WithTickSize2(0.0001)
                .WithContractSize(100000)
                .WithCurrency1("EUR")
                .WithCurrency2("USD")
                .WithSymbol("EURUSD")
                .WithCategory(Category.Forex)
            ;

        var accountBalance = new AccountBalance
        {
            Balance = 1000,
            Equity = 1000
        };

        var apiHandlerMock = new Mock<IApiHandler>();

        apiHandlerMock.Setup(api => api.GetTickPriceAsync(It.IsAny<string>())).ReturnsAsync(
            new Tick { Bid = 1.10539m });

        apiHandlerMock.Setup(x => x.GetSymbolInformationAsync(It.IsAny<string>()))
            .ReturnsAsync(symbolInfo);

        apiHandlerMock.Setup(x => x.GetBalanceAsync()).ReturnsAsync(accountBalance);

        // Act
        var monyManagement =
            new RobotAppLibraryV2.MoneyManagement.MoneyManagement(apiHandlerMock.Object, "EURUSD", _logger.Object,
                "test");

        monyManagement.Risque = 1;

        var result = monyManagement.CalculatePositionSize(1.10539m, 1.10405m);
        // Assert

        result.Should().BeApproximately(0.08, 0.02);
    }

    [Fact]
    public void Test_Position_Size_throw_exception()
    {
        // Arranage
        var symbolInfo = new SymbolInfo()
                .WithLeverage(3.33)
                .WithTickSize(0.00001)
                .WithTickSize2(0.0001)
                .WithLotMin(0.01)
                .WithCurrency1("EUR")
                .WithCurrency2("USD")
                .WithSymbol("EURUSD")
                .WithCategory(Category.Forex)
            ;

        var accountBalance = new AccountBalance
        {
            Balance = 1000,
            Equity = 1000
        };

        var apiHandlerMock = new Mock<IApiHandler>();

        apiHandlerMock.Setup(api => api.GetTickPriceAsync(It.IsAny<string>())).ReturnsAsync(
            new Tick { Bid = 1.10539m });

        apiHandlerMock.Setup(x => x.GetSymbolInformationAsync(It.IsAny<string>()))
            .ReturnsAsync(symbolInfo);

        apiHandlerMock.Setup(x => x.GetBalanceAsync()).ReturnsAsync(accountBalance);

        // Act
        var monyManagement =
            new RobotAppLibraryV2.MoneyManagement.MoneyManagement(apiHandlerMock.Object, "EURUSD", _logger.Object,
                "test");

        monyManagement.Risque = 1;

        var act = () => monyManagement.CalculatePositionSize(1.10539m, 1.99999m);
        // Assert

        act.Should().Throw<MoneyManagementException>();
    }

    [Fact]
    public void Test_Position_Size_4decimal_baseEur_margin_exceded()
    {
        // Arranage
        var symbolInfo = new SymbolInfo()
                .WithLeverage(3.33)
                .WithTickSize(0.00001)
                .WithTickSize2(0.0001)
                .WithContractSize(100000)
                .WithLotMin(0.01)
                .WithCurrency1("EUR")
                .WithCurrency2("USD")
                .WithSymbol("EURUSD")
                .WithCategory(Category.Forex)
            ;

        var accountBalance = new AccountBalance
        {
            MarginFree = 100,
            Balance = 1000,
            Equity = 1000
        };

        var apiHandlerMock = new Mock<IApiHandler>();

        apiHandlerMock.Setup(api => api.GetTickPriceAsync(It.IsAny<string>())).ReturnsAsync(
            new Tick { Bid = 1.10539m });

        apiHandlerMock.Setup(x => x.GetSymbolInformationAsync(It.IsAny<string>()))
            .ReturnsAsync(symbolInfo);

        apiHandlerMock.Setup(x => x.GetBalanceAsync()).ReturnsAsync(accountBalance);

        // Act
        var monyManagement =
            new RobotAppLibraryV2.MoneyManagement.MoneyManagement(apiHandlerMock.Object, "EURUSD", _logger.Object,
                "test");

        monyManagement.Risque = 1;

        var result = monyManagement.CalculatePositionSize(1.10539m, 1.10405m);
        // Assert

        result.Should().BeApproximately(0.01, 0.02);
    }


    [Fact]
    public void Test_Position_Size_4decimal_base_notEur()
    {
        // Arranage
        var symbolInfo = new SymbolInfo()
                .WithLeverage(3.33)
                .WithTickSize(0.00001)
                .WithTickSize2(0.0001)
                .WithContractSize(100000)
                .WithCurrency1("AUD")
                .WithCurrency2("USD")
                .WithSymbol("AUDUSD")
                .WithCategory(Category.Forex)
            ;

        var accountBalance = new AccountBalance
        {
            Balance = 1000,
            Equity = 1000
        };

        var apiHandlerMock = new Mock<IApiHandler>();

        apiHandlerMock.SetupSequence(api => api.GetTickPriceAsync(It.IsAny<string>()))
            .ReturnsAsync(new Tick { Bid = 0.67545m })
            .ReturnsAsync(new Tick { Bid = 1.10765m })
            .ReturnsAsync(new Tick { Bid = 0.67545m });

        apiHandlerMock.Setup(x => x.GetAllSymbolsAsync()).ReturnsAsync(new List<string>
        {
            "EURUSD"
        });

        apiHandlerMock.Setup(x => x.GetSymbolInformationAsync(It.IsAny<string>()))
            .ReturnsAsync(symbolInfo);


        apiHandlerMock.Setup(x => x.GetBalanceAsync()).ReturnsAsync(accountBalance);

        // Act
        var monyManagement =
            new RobotAppLibraryV2.MoneyManagement.MoneyManagement(apiHandlerMock.Object, "AUDUSD", _logger.Object,
                "test");

        monyManagement.Risque = 1;

        var result = monyManagement.CalculatePositionSize(0.67545m, 0.67637m);
        // Assert

        result.Should().BeApproximately(0.12, 0.02);
    }


    [Fact]
    public void Test_Position_Size_base_case_jpy()
    {
        // Arranage
        var symbolInfo = new SymbolInfo()
                .WithLeverage(3.33)
                .WithTickSize(0.001)
                .WithTickSize2(0.01)
                .WithContractSize(100000)
                .WithCurrency1("EUR")
                .WithCurrency2("JPY")
                .WithSymbol("EURJPY")
                .WithCategory(Category.Forex)
            ;

        var accountBalance = new AccountBalance
        {
            Balance = 1000,
            Equity = 1000
        };

        var apiHandlerMock = new Mock<IApiHandler>();

        apiHandlerMock.Setup(api => api.GetTickPriceAsync(It.IsAny<string>()))
            .ReturnsAsync(new Tick { Bid = 0.67545m });

        apiHandlerMock.Setup(x => x.GetSymbolInformationAsync(It.IsAny<string>()))
            .ReturnsAsync(symbolInfo);

        apiHandlerMock.Setup(x => x.GetBalanceAsync()).ReturnsAsync(accountBalance);

        // Act
        var monyManagement =
            new RobotAppLibraryV2.MoneyManagement.MoneyManagement(apiHandlerMock.Object, "EURJPY", _logger.Object,
                "test");

        monyManagement.Risque = 1;

        var result = monyManagement.CalculatePositionSize(156.584m, 156.036m);
        // Assert

        result.Should().BeApproximately(0.02, 0.02);
    }


    [Fact]
    public void Test_Position_Size_case_indices()
    {
        // Arranage
        var symbolInfo = new SymbolInfo()
                .WithLeverage(5)
                .WithTickSize(0.1)
                .WithContractSize(25)
                .WithCurrency1("EUR")
                .WithCurrency2("EUR")
                .WithSymbol("DE30")
                .WithCategory(Category.Indices)
            ;

        var accountBalance = new AccountBalance
        {
            Balance = 10000,
            Equity = 10000
        };

        var apiHandlerMock = new Mock<IApiHandler>();

        apiHandlerMock.Setup(api => api.GetTickPriceAsync(It.IsAny<string>()))
            .ReturnsAsync(new Tick { Bid = 16154.8m });

        apiHandlerMock.Setup(x => x.GetSymbolInformationAsync(It.IsAny<string>()))
            .ReturnsAsync(symbolInfo);

        apiHandlerMock.Setup(x => x.GetBalanceAsync()).ReturnsAsync(accountBalance);

        // Act
        var monyManagement =
            new RobotAppLibraryV2.MoneyManagement.MoneyManagement(apiHandlerMock.Object, "DE30", _logger.Object,
                "test");

        monyManagement.Risque = 1;

        var result = monyManagement.CalculatePositionSize(16281.4m, 16342.1m);
        // Assert

        result.Should().BeApproximately(0.07, 0.02);
    }


    [Fact]
    public void Test_Position_Size_case_indices_margin_exceded()
    {
        // Arranage
        var symbolInfo = new SymbolInfo()
                .WithLeverage(5)
                .WithTickSize(0.1)
                .WithLotMin(0.01)
                .WithContractSize(25)
                .WithCurrency1("EUR")
                .WithCurrency2("EUR")
                .WithSymbol("DE30")
                .WithCategory(Category.Indices)
            ;

        var accountBalance = new AccountBalance
        {
            Balance = 10000,
            Equity = 10000,
            MarginFree = 100
        };

        var apiHandlerMock = new Mock<IApiHandler>();

        apiHandlerMock.Setup(api => api.GetTickPriceAsync(It.IsAny<string>()))
            .ReturnsAsync(new Tick { Bid = 16154.8m });

        apiHandlerMock.Setup(x => x.GetSymbolInformationAsync(It.IsAny<string>()))
            .ReturnsAsync(symbolInfo);

        apiHandlerMock.Setup(x => x.GetAllSymbolsAsync()).ReturnsAsync(new List<string>
        {
            "EUREUR"
        });

        apiHandlerMock.Setup(x => x.GetBalanceAsync()).ReturnsAsync(accountBalance);

        // Act
        var monyManagement =
            new RobotAppLibraryV2.MoneyManagement.MoneyManagement(apiHandlerMock.Object, "DE30", _logger.Object,
                "test");

        monyManagement.Risque = 1;

        var result = monyManagement.CalculatePositionSize(16281.4m, 16342.1m);
        // Assert

        result.Should().BeApproximately(0.01, 0.02);
    }

    #endregion

    #region Treshold event

    [Fact]
    public void test_perteRisqueTreshold_noTrigger()
    {
        // Arranage
        var apiHandlerMock = new Mock<IApiHandler>();
        var symbolInfo = new SymbolInfo()
                .WithLeverage(10)
                .WithTickSize(0.00001)
                .WithTickSize2(0.0001)
                .WithCurrency1("EUR")
                .WithCurrency2("USD")
                .WithCategory(Category.Forex)
                .WithSymbol("EURUSD")
            ;


        apiHandlerMock.SetupSequence(api => api.GetTickPriceAsync(It.IsAny<string>()))
            .ReturnsAsync(new Tick { Bid = (decimal?)0.67288 })
            .ReturnsAsync(new Tick { Bid = (decimal?)1.65313 });

        apiHandlerMock.Setup(x => x.GetSymbolInformationAsync(It.IsAny<string>()))
            .ReturnsAsync(symbolInfo);

        apiHandlerMock.Setup(x => x.GetAllPositionsByCommentAsync("test")).ReturnsAsync(new List<Position>())
            .Verifiable();

        var accountBalance = new AccountBalance
        {
            Balance = 1000
        };

        apiHandlerMock.Setup(x => x.GetBalanceAsync()).ReturnsAsync(accountBalance);
        var monyManagement =
            new RobotAppLibraryV2.MoneyManagement.MoneyManagement(apiHandlerMock.Object, "EURUSD", _logger.Object,
                "test");
        var caller = false;

        monyManagement.TreshHoldEvent += (sender, type) => caller = true;

        // Act
        var position = new Position().SetProfit(10000);
        apiHandlerMock.Raise(x => x.PositionUpdatedEvent += null, this, position);

        // Assert
        caller.Should().BeFalse();
    }


    [Fact]
    public void test_perteRisqueTreshold_Trigger()
    {
        // Arranage
        var apiHandlerMock = new Mock<IApiHandler>();
        var symbolInfo = new SymbolInfo()
                .WithLeverage(10)
                .WithTickSize(0.00001)
                .WithTickSize2(0.0001)
                .WithCurrency1("EUR")
                .WithCurrency2("USD")
                .WithCategory(Category.Forex)
                .WithSymbol("EURUSD")
            ;


        apiHandlerMock.SetupSequence(api => api.GetTickPriceAsync(It.IsAny<string>()))
            .ReturnsAsync(new Tick { Bid = (decimal?)0.67288 })
            .ReturnsAsync(new Tick { Bid = (decimal?)1.65313 });

        apiHandlerMock.Setup(x => x.GetSymbolInformationAsync(It.IsAny<string>()))
            .ReturnsAsync(symbolInfo);

        apiHandlerMock.Setup(x => x.GetAllPositionsByCommentAsync("test")).ReturnsAsync(new List<Position>())
            .Verifiable();

        var accountBalance = new AccountBalance
        {
            Balance = 1000
        };

        apiHandlerMock.Setup(x => x.GetBalanceAsync()).ReturnsAsync(accountBalance);
        var monyManagement =
            new RobotAppLibraryV2.MoneyManagement.MoneyManagement(apiHandlerMock.Object, "EURUSD", _logger.Object,
                "test");
        var caller = false;
        MoneyManagementTresholdType? tresholdType = MoneyManagementTresholdType.None;
        monyManagement.TreshHoldEvent += (sender, type) =>
        {
            tresholdType = type;
            caller = true;
        };

        // Act
        var position = new Position().SetProfit(-10000);
        apiHandlerMock.Raise(x => x.PositionUpdatedEvent += null, this, position);

        // Assert
        caller.Should().BeTrue();
        tresholdType.Should().Be(MoneyManagementTresholdType.ProfitTreshHold);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void test_drawdownEvent_positionClose(bool secureCOntrolPosition)
    {
        // Arranage
        var apiHandlerMock = new Mock<IApiHandler>();
        var symbolInfo = new SymbolInfo()
                .WithLeverage(10)
                .WithTickSize(0.00001)
                .WithTickSize2(0.0001)
                .WithCurrency1("EUR")
                .WithCurrency2("USD")
                .WithCategory(Category.Forex)
                .WithSymbol("EURUSD")
            ;


        apiHandlerMock.SetupSequence(api => api.GetTickPriceAsync(It.IsAny<string>()))
            .ReturnsAsync(new Tick { Bid = (decimal?)0.67288 })
            .ReturnsAsync(new Tick { Bid = (decimal?)1.65313 });

        apiHandlerMock.Setup(x => x.GetSymbolInformationAsync(It.IsAny<string>()))
            .ReturnsAsync(symbolInfo);

        apiHandlerMock.Setup(x => x.GetAllPositionsByCommentAsync("test")).ReturnsAsync(new List<Position>())
            .Verifiable();

        var accountBalance = new AccountBalance
        {
            Balance = 100
        };

        apiHandlerMock.Setup(x => x.GetBalanceAsync()).ReturnsAsync(accountBalance);


        apiHandlerMock.Setup(x => x.GetAllPositionsByCommentAsync(It.IsAny<string>()))
            .ReturnsAsync(new List<Position>
            {
                new() { Profit = 10, DateClose = new DateTime(2022, 01, 01) },
                new() { Profit = 20, DateClose = new DateTime(2022, 01, 02) },
                new() { Profit = 30, DateClose = new DateTime(2022, 01, 03) },
                new() { Profit = -10, DateClose = new DateTime(2022, 01, 04) }
            });


        var monyManagement =
            new RobotAppLibraryV2.MoneyManagement.MoneyManagement(apiHandlerMock.Object, "EURUSD", _logger.Object,
                "test");
        monyManagement.ToleratedDrawnDown = 10;
        monyManagement.LooseStreak = 20;
        monyManagement.SecureControlPosition = secureCOntrolPosition;

        var caller = false;
        var eventType = MoneyManagementTresholdType.None;
        monyManagement.TreshHoldEvent += (sender, type) =>
        {
            eventType = type;
            caller = true;
        };

        // Act
        var position = new Position().SetProfit(-10000).SetDateClose(new DateTime(2022, 01, 05));
        apiHandlerMock.Raise(x => x.PositionClosedEvent += null, this, position);

        // Assert
        caller.Should().Be(secureCOntrolPosition);
        if (secureCOntrolPosition) eventType.Should().Be(MoneyManagementTresholdType.Drowdown);
    }


    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void test_looseStreak_positionClose(bool secureCOntrolPosition)
    {
        // Arranage
        var apiHandlerMock = new Mock<IApiHandler>();
        var symbolInfo = new SymbolInfo()
                .WithLeverage(10)
                .WithTickSize(0.00001)
                .WithTickSize2(0.0001)
                .WithCurrency1("EUR")
                .WithCurrency2("USD")
                .WithCategory(Category.Forex)
                .WithSymbol("EURUSD")
            ;


        apiHandlerMock.SetupSequence(api => api.GetTickPriceAsync(It.IsAny<string>()))
            .ReturnsAsync(new Tick { Bid = (decimal?)0.67288 })
            .ReturnsAsync(new Tick { Bid = (decimal?)1.65313 });

        apiHandlerMock.Setup(x => x.GetSymbolInformationAsync(It.IsAny<string>()))
            .ReturnsAsync(symbolInfo);

        apiHandlerMock.Setup(x => x.GetAllPositionsByCommentAsync("test")).ReturnsAsync(new List<Position>())
            .Verifiable();

        var accountBalance = new AccountBalance
        {
            Balance = 100000
        };

        apiHandlerMock.Setup(x => x.GetBalanceAsync()).ReturnsAsync(accountBalance);


        apiHandlerMock.Setup(x => x.GetAllPositionsByCommentAsync(It.IsAny<string>()))
            .ReturnsAsync(new List<Position>
            {
                new() { Profit = -10, DateClose = new DateTime(2022, 01, 01) },
                new() { Profit = -20, DateClose = new DateTime(2022, 01, 02) },
                new() { Profit = -30, DateClose = new DateTime(2022, 01, 03) },
                new() { Profit = -10, DateClose = new DateTime(2022, 01, 04) }
            });


        var monyManagement =
            new RobotAppLibraryV2.MoneyManagement.MoneyManagement(apiHandlerMock.Object, "EURUSD", _logger.Object,
                "test");
        monyManagement.ToleratedDrawnDown = 10;
        monyManagement.LooseStreak = 5;
        monyManagement.SecureControlPosition = secureCOntrolPosition;

        var caller = false;
        var eventType = MoneyManagementTresholdType.None;
        monyManagement.TreshHoldEvent += (sender, type) =>
        {
            eventType = type;
            caller = true;
        };

        // Act
        var position = new Position().SetProfit(-1).SetDateClose(new DateTime(2022, 01, 05));
        apiHandlerMock.Raise(x => x.PositionClosedEvent += null, this, position);

        // Assert
        caller.Should().Be(secureCOntrolPosition);
        if (secureCOntrolPosition) eventType.Should().Be(MoneyManagementTresholdType.LooseStreak);
    }


    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void test_profitFactor_positionClose(bool secureCOntrolPosition)
    {
        // Arranage
        var apiHandlerMock = new Mock<IApiHandler>();
        var symbolInfo = new SymbolInfo()
                .WithLeverage(10)
                .WithTickSize(0.00001)
                .WithTickSize2(0.0001)
                .WithCurrency1("EUR")
                .WithCurrency2("USD")
                .WithCategory(Category.Forex)
                .WithSymbol("EURUSD")
            ;


        apiHandlerMock.SetupSequence(api => api.GetTickPriceAsync(It.IsAny<string>()))
            .ReturnsAsync(new Tick { Bid = (decimal?)0.67288 })
            .ReturnsAsync(new Tick { Bid = (decimal?)1.65313 });

        apiHandlerMock.Setup(x => x.GetSymbolInformationAsync(It.IsAny<string>()))
            .ReturnsAsync(symbolInfo);

        apiHandlerMock.Setup(x => x.GetAllPositionsByCommentAsync("test")).ReturnsAsync(new List<Position>())
            .Verifiable();

        var accountBalance = new AccountBalance
        {
            Balance = 100000
        };

        apiHandlerMock.Setup(x => x.GetBalanceAsync()).ReturnsAsync(accountBalance);


        apiHandlerMock.Setup(x => x.GetAllPositionsByCommentAsync(It.IsAny<string>()))
            .ReturnsAsync(new List<Position>
            {
                new() { Profit = -10, DateClose = new DateTime(2022, 01, 01) },
                new() { Profit = -20, DateClose = new DateTime(2022, 01, 02) },
                new() { Profit = -30, DateClose = new DateTime(2022, 01, 03) },
                new() { Profit = -10, DateClose = new DateTime(2022, 01, 04) }
            });


        var monyManagement =
            new RobotAppLibraryV2.MoneyManagement.MoneyManagement(apiHandlerMock.Object, "EURUSD", _logger.Object,
                "test");
        monyManagement.ToleratedDrawnDown = 10;
        monyManagement.LooseStreak = 10;
        monyManagement.SecureControlPosition = secureCOntrolPosition;

        var caller = false;
        var eventType = MoneyManagementTresholdType.None;
        monyManagement.TreshHoldEvent += (sender, type) =>
        {
            eventType = type;
            caller = true;
        };

        // Act
        var position = new Position().SetProfit(10).SetDateClose(new DateTime(2022, 01, 05));
        apiHandlerMock.Raise(x => x.PositionClosedEvent += null, this, position);

        // Assert
        caller.Should().Be(secureCOntrolPosition);
        if (secureCOntrolPosition) eventType.Should().Be(MoneyManagementTresholdType.Profitfactor);
    }

    #endregion
}