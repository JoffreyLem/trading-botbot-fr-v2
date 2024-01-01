using Front.Services;
using Microsoft.AspNetCore.Components;
using StrategyApi.StrategyBackgroundService.Dto.Services;
using StrategyApi.StrategyBackgroundService.Services;

namespace Front.Pages.Strategy.Composants;

public class BacktestComponentBase : StrategyIdComponentBase
{
    [Inject] private IStrategyHandlerService _apiStrategyService { get; set; }

    [Inject] private ShowToastService ToastService { get; set; }

    protected BackTestDto BackTestDto { get; set; }

    protected bool OnLoading { get; set; }

    protected override async Task OnInitializedAsync()
    {
        try
        {
            BackTestDto = await _apiStrategyService.GetBacktestInfo(StrategyId);
        }
        catch (Exception)
        {
            ToastService.ShowToastError("Can't get backtest data");
        }
    }

    protected async Task RunBacktest()
    {
        try
        {
            // TODO : Refacto pour harmoniser avec l'autre backtest
            OnLoading = true;
            BackTestDto = await _apiStrategyService.RunBackTest(StrategyId, 1000, 1, 1);
            ToastService.ShowToastSuccess("BacktestEnd");
        }
        catch
        {
            ToastService.ShowToastError("Can't run backtest");
            OnLoading = false;
        }

        OnLoading = false;
        StateHasChanged();
    }
}