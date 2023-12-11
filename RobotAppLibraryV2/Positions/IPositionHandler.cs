using RobotAppLibraryV2.Modeles;

namespace RobotAppLibraryV2.Positions;

public interface IPositionHandler
{
    int DefaultSl { get; set; }
    int DefaultTp { get; set; }
    Position? PositionOpened { get; }
    Position? PositionPending { get; }
    Tick LastPrice { get; }
    bool PositionInProgress { get; }
    event EventHandler<Position>? PositionOpenedEvent;
    event EventHandler<Position>? PositionUpdatedEvent;
    event EventHandler<Position>? PositionRejectedEvent;
    event EventHandler<Position>? PositionClosedEvent;

    Task OpenPositionAsync(string symbol, TypeOperation typePosition, double volume,
        decimal sl = 0M,
        decimal tp = 0M, long? expiration = 0L);

    Task UpdatePositionAsync(Position position);
    Task ClosePositionAsync(Position position);
    decimal CalculateStopLoss(decimal pips, TypeOperation positionType);
    decimal CalculateTakeProfit(decimal pips, TypeOperation positionType);
}