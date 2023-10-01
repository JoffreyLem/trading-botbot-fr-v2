using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using StrategyApi.StrategyBackgroundService.Dto.Services;

namespace StrategyApi.StrategyBackgroundService.Hubs;

// TODO : Revoke le hub ?
[Authorize]
public class ApiHandlerHub : Hub<IApiHandlerHub>
{
    public const string ApiHubName = "ApiHub";
    
    public static event Action<AccountBalanceDto>? OnAccountBalanceReceived;
    public async Task SendBalanceState(AccountBalanceDto modele)
    {
        OnAccountBalanceReceived?.Invoke(modele);
        await Clients.All.SendBalanceState(modele);
    }
}