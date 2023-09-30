using Microsoft.AspNetCore.SignalR;
using StrategyApi.StrategyBackgroundService.Dto.Services;

namespace StrategyApi.StrategyBackgroundService.Hubs;

[Microsoft.AspNetCore.Authorization.Authorize]
public class ApiHandlerHub : Hub<IApiHandlerHub>
{
    public async Task SendBalanceState(AccountBalanceDto modele)
    {
        await Clients.All.SendBalanceState(modele);
    }
}