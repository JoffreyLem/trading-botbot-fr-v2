using System.Collections.ObjectModel;
using Front.Services;
using Microsoft.AspNetCore.Components;
using StrategyApi.StrategyBackgroundService.Dto.Services;
using StrategyApi.StrategyBackgroundService.Services;

namespace Front.Composants.Strategy;

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
            BackTestDto = await _apiStrategyService.GetBacktestInfo(base.StrategyId);
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
            OnLoading = true;
            BackTestDto = await _apiStrategyService.RunBackTest(base.StrategyId, 1000, 1, 1);
        }
        catch
        {
            ToastService.ShowToastError("Can't run backtest");
        }
        finally
        {
            OnLoading = false;
            this.StateHasChanged();
        }
       
    }
}