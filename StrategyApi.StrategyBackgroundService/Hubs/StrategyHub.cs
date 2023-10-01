using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using StrategyApi.StrategyBackgroundService.Dto.Services;
using StrategyApi.StrategyBackgroundService.Dto.Services.Enum;

namespace StrategyApi.StrategyBackgroundService.Hubs;

// TODO : Revoke le hub ?
[Authorize]
public class StrategyHub : Hub<IStrategyHub>
{
    public const string HubName = "StrategyHub";
    
    public static event Action<TickDto>? OnTickReceived;
    public static event Action<CandleDto>? OnCandleReceived;
    public static event Action<PositionDto, PositionStateEnum>? OnPositionStateReceived;
    public static event Action<EventType, string>? OnEventReceived;
    
    public async Task SendTick(TickDto tick)
    {
        OnTickReceived?.Invoke(tick);
        await Clients.All.SendTick(tick);
    }

    public async Task SendCandle(CandleDto candle)
    {
        OnCandleReceived?.Invoke(candle);
        await Clients.All.SendCandle(candle);
    }

    public async Task SendPositionState(PositionDto position, PositionStateEnum state)
    {
        OnPositionStateReceived?.Invoke(position,state);
        await Clients.All.SendPositionState(position, state);
    }

    public async Task SendEvent(EventType eventType, string message)
    {
        OnEventReceived?.Invoke(eventType,message);
        await Clients.All.SendEvent(eventType, message);
    }
}