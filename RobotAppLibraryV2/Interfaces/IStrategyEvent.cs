using RobotAppLibraryV2.Modeles;
using RobotAppLibraryV2.Modeles.Enum;

namespace RobotAppLibraryV2.Interfaces;

public interface IStrategyEvent
{
    public event EventHandler<StrategyReasonClosed>? StrategyClosed;
    public event EventHandler<Tick>? TickEvent;
    public event EventHandler<Candle>? CandleEvent;
    public event EventHandler<MoneyManagementTresholdType>? TresholdEvent;
    public event EventHandler<Position>? PositionOpenedEvent;
    public event EventHandler<Position>? PositionUpdatedEvent;
    public event EventHandler<Position>? PositionRejectedEvent;
    public event EventHandler<Position>? PositionClosedEvent;
}