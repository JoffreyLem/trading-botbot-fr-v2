using StrategyApi.Dto.Dto;

namespace StrategyApi.StrategyBackgroundService.Hubs;

public interface IApiHandlerHub
{
    Task SendBalanceState(AccountBalanceDto modele);
}