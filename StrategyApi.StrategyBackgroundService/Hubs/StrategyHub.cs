using Microsoft.AspNetCore.SignalR;
using RobotAppLibraryV2.Modeles;
using StrategyApi.StrategyBackgroundService.Dto.Services.Dto;
using StrategyApi.StrategyBackgroundService.Dto.Services.Enum;

namespace StrategyApi.StrategyBackgroundService.Hubs;

[Microsoft.AspNetCore.Authorization.Authorize]
public class StrategyHub : Hub<IStrategyHub>
{
    public async Task SendTick(TickDto tick)
    {
        await Clients.All.SendTick(tick);
    }

    public async Task SendCandle(CandleDto candle)
    {
        await Clients.All.SendCandle(candle);
    }

    public async Task SendPositionState(PositionDto position, PositionStateEnum state)
    {
        await Clients.All.SendPositionState(position, state);
    }

    public async Task SendEvent(EventType eventType, string message)
    {
        await Clients.All.SendEvent(eventType, message);
    }
}