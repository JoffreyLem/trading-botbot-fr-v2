using StrategyApi.StrategyBackgroundService.Dto.Services;
using StrategyApi.StrategyBackgroundService.Dto.Services.Enum;

namespace StrategyApi.StrategyBackgroundService.Hubs;

public interface IStrategyHub
{
    Task SendTick(TickDto tickModele);

    Task SendCandle(CandleDto candle);

    Task SendPositionState(PositionDto positionModele, PositionStateEnum state);

    Task SendEvent(EventType eventType, string message);
}