using RobotAppLibraryV2.Modeles;

namespace RobotAppLibraryV2.Interfaces;

public interface IStrategyEvent
{
    public event EventHandler<StrategyReasonClosed>? StrategyClosed;
    public event EventHandler<Tick>? TickEvent;
    public event EventHandler<Candle>? CandleEvent;
    public event EventHandler<EventTreshold>? TresholdEvent;
    public event EventHandler<Position>? PositionOpenedEvent;
    public event EventHandler<Position>? PositionUpdatedEvent;
    public event EventHandler<Position>? PositionRejectedEvent;
    public event EventHandler<Position>? PositionClosedEvent;
}