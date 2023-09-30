using StrategyApi.StrategyBackgroundService.Dto.Services.Dto;

namespace StrategyApi.StrategyBackgroundService.Hubs;

public interface IApiHandlerHub
{
    Task SendBalanceState(AccountBalanceDto modele);
}