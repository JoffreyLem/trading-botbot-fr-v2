using StrategyApi.StrategyBackgroundService.Dto.Services;

namespace StrategyApi.StrategyBackgroundService.Hubs;

public interface IApiHandlerHub
{
    Task SendBalanceState(AccountBalanceDto modele);
}