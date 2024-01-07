using RobotAppLibraryV2.Modeles;

namespace RobotAppLibraryV2.MoneyManagement;

public interface IMoneyManagement : IDisposable
{
    ILotValueCalculator LotValueCalculator { get; }
    double MaxLot { get; }
    SymbolInfo SymbolInfo { get; }
    double CalculatePositionSize(decimal entryPrice, decimal stopLossPrice, double risk);
}