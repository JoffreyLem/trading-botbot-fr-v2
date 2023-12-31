using RobotAppLibraryV2.Modeles;

namespace RobotAppLibraryV2.MoneyManagement;

public interface ILotValueCalculator
{
    public double PipValueStandard { get; }
    public double PipValueMiniLot => PipValueStandard / 10;
    public double PipValueMicroLot => PipValueStandard / 100;
    public double PipValueNanoLot => PipValueStandard / 1000;
    double MarginPerLot { get; }
    Tick? TickPriceSecondary { get; }
    void Dispose();
}