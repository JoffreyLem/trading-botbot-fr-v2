using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Robot.Server.Dto.Response;

namespace Robot.Server.Hubs;

[Authorize]
public class HubInfoClient : Hub<IHubInfoClient>
{
    public async Task SendCandle(CandleDto candle)
    {
        await Clients.All.ReceiveCandle(candle);
    }

    public async Task SendTick(TickDto tick)
    {
        await Clients.All.ReceiveTick(tick);
    }

    public async Task SendPosition(PositionDto positionDto)
    {
        await Clients.All.ReceivePosition(positionDto);
    }
}