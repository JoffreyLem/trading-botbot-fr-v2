using FluentAssertions;
using Moq;
using RobotAppLibraryV2.ApiHandler.Interfaces;
using RobotAppLibraryV2.Modeles;
using RobotAppLibraryV2.MoneyManagement;
using Serilog;

namespace RobotAppLibraryV2.Tests.MoneyManagement;

public class MoneyManagementTest
{
    private readonly Mock<IApiHandler> _apihandler = new();
    private readonly Mock<ILogger> _logger = new();
    private readonly Mock<ILotValueCalculator> lotValueCalculatorMock = new();
    private readonly RobotAppLibraryV2.MoneyManagement.MoneyManagement moneyManagement;


    public MoneyManagementTest()
    {
        _logger.Setup(x => x.ForContext<RobotAppLibraryV2.MoneyManagement.MoneyManagement>())
            .Returns(_logger.Object);

        _apihandler.Setup(x => x.GetBalanceAsync())
            .ReturnsAsync(new AccountBalance
            {
                Balance = 1000
            });
        lotValueCalculatorMock.Setup(x => x.MarginPerLot).Returns(25000);
        moneyManagement = new RobotAppLibraryV2.MoneyManagement.MoneyManagement(_apihandler.Object, "test",
            _logger.Object,
            lotValueCalculatorMock.Object, "test");
    }

    [Fact]
    public void Test_Init()
    {
        _apihandler.Verify(x => x.GetBalanceAsync(), Times.Once);
        _apihandler.Verify(x => x.GetSymbolInformationAsync(It.IsAny<string>()), Times.Once);
        moneyManagement.MaxLot.Should().Be(0.04);
    }

    [Fact]
    public void Test_Position_Size_Forex()
    {
        // Arrange 
        Mock<ILogger> logger = new();
        var valueCalculatorMock = new Mock<ILotValueCalculator>();
        var apihandler = new Mock<IApiHandler>();

        apihandler.Setup(x => x.GetSymbolInformationAsync(It.IsAny<string>()))
            .ReturnsAsync(new SymbolInfo
            {
                Symbol = "EURUSD",
                Category = Category.Forex
            });

        apihandler.Setup(x => x.GetBalanceAsync()).ReturnsAsync(new AccountBalance
        {
            Balance = 1000,
            Equity = 1000,
            MarginFree = 1000
        });
        valueCalculatorMock.SetupGet(x => x.PipValueStandard).Returns(9.80);
        valueCalculatorMock.SetupGet(x => x.MarginPerLot).Returns(2400);
        var management = new RobotAppLibraryV2.MoneyManagement.MoneyManagement(apihandler.Object, "", logger.Object,
            valueCalculatorMock.Object, "");

        // Act
        var positionSize = management.CalculatePositionSize(1.1210m, 1.1250m, 5);

        // Assert
        positionSize.Should().BeApproximately(0.12, 0.01);
    }

    [Fact]
    public void Test_Position_Size_Forex_Margin_Exceded()
    {
        // Arrange 
        Mock<ILogger> logger = new();
        var valueCalculatorMock = new Mock<ILotValueCalculator>();
        var apihandler = new Mock<IApiHandler>();

        apihandler.Setup(x => x.GetSymbolInformationAsync(It.IsAny<string>()))
            .ReturnsAsync(new SymbolInfo
            {
                Symbol = "EURUSD",
                Category = Category.Forex,
                LotMin = 0.01
            });

        apihandler.Setup(x => x.GetBalanceAsync()).ReturnsAsync(new AccountBalance
        {
            Balance = 1000,
            Equity = 1000,
            MarginFree = 100
        });
        valueCalculatorMock.SetupGet(x => x.PipValueStandard).Returns(9.80);
        valueCalculatorMock.SetupGet(x => x.MarginPerLot).Returns(2400);
        var management = new RobotAppLibraryV2.MoneyManagement.MoneyManagement(apihandler.Object, "", logger.Object,
            valueCalculatorMock.Object, "");

        // Act
        var positionSize = management.CalculatePositionSize(1.1210m, 1.1250m, 5);

        // Assert
        positionSize.Should().Be(0.12);
    }


    [Fact]
    public void Test_Position_Size_Indices()
    {
        // Arrange 
        Mock<ILogger> logger = new();
        var valueCalculatorMock = new Mock<ILotValueCalculator>();
        var apihandler = new Mock<IApiHandler>();

        apihandler.Setup(x => x.GetSymbolInformationAsync(It.IsAny<string>()))
            .ReturnsAsync(new SymbolInfo
            {
                Symbol = "DE30",
                Category = Category.Indices
            });

        apihandler.Setup(x => x.GetBalanceAsync()).ReturnsAsync(new AccountBalance
        {
            Balance = 5000,
            Equity = 5000,
            MarginFree = 5000
        });
        valueCalculatorMock.SetupGet(x => x.PipValueStandard).Returns(25);
        valueCalculatorMock.SetupGet(x => x.MarginPerLot).Returns(19000);
        var management = new RobotAppLibraryV2.MoneyManagement.MoneyManagement(apihandler.Object, "", logger.Object,
            valueCalculatorMock.Object, "");

        // Act
        var positionSize = management.CalculatePositionSize(15211.1m, 15221m, 5);

        // Assert
        positionSize.Should().BeApproximately(0.26, 0.01);
    }
}