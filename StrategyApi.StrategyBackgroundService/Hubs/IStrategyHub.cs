using RobotAppLibraryV2.Modeles;
using StrategyApi.StrategyBackgroundService.Dto.Services.Dto;
using StrategyApi.StrategyBackgroundService.Dto.Services.Enum;

namespace StrategyApi.StrategyBackgroundService.Hubs;

public interface IStrategyHub
{
    Task SendTick(TickDto tickModele);

    Task SendCandle(CandleDto candle);

    Task SendPositionState(PositionDto positionModele, PositionStateEnum state);

    Task SendEvent(EventType eventType, string message);
}