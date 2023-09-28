using Microsoft.AspNetCore.SignalR;
using RobotAppLibraryV2.Modeles;
using StrategyApi.Dto.Dto;
using StrategyApi.Dto.Enum;

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