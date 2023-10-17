using System.Collections.ObjectModel;
using Front.Services;
using Microsoft.AspNetCore.Components;
using RobotAppLibraryV2.Modeles;
using StrategyApi.StrategyBackgroundService.Dto.Services;
using StrategyApi.StrategyBackgroundService.Services;
using Syncfusion.Blazor;
using Syncfusion.Blazor.Charts;

namespace Front.Composants;

public class CandleChartComponentBase : ComponentBase
{
    [Inject] protected IStrategyHandlerService _strategyService { get; set; }
    [Inject] private ShowToastService ToastService { get; set; }
    [Inject] private IEventBus _eventBus { get; set; }

    protected CandleDto? LastCandle
    {
        get => Candles.LastOrDefault();
        set => Candles[^1] = value;
    }

    protected TickDto LastTick = new TickDto();


    protected ObservableCollection<CandleDto> Candles { get; set; } = new ObservableCollection<CandleDto>();

    protected string translateY = "-5px";
    protected string loadClass = "stockchartloader";
    protected string loadDiv = "stockchartdiv";

    private bool loaded = false;
    
    protected override async Task OnInitializedAsync()
    {
        try
        {
            Candles = new ObservableCollection<CandleDto>((await _strategyService.GetChart()));
            _eventBus.Subscribe<CandleDto>(StrategyHubOnOnCandleReceived);
            _eventBus.Subscribe<TickDto>(StrategyHubOnOnTickReceived);
        }
        catch
        {
            ToastService.ShowToastError("Can't load graphique");
        } 
    }


    protected void ChartLoaded(StockChartEventArgs args)
    {
        loadClass = "";
        loadDiv = "";
        loaded = true;
        StateHasChanged();
    }
    
    public void RangeChanged(StockChartRangeChangeEventArgs args)
    {
       
    }
    
    private void StrategyHubOnOnTickReceived(TickDto obj)
    {
        if (loaded)
        {
            InvokeAsync(() =>
            {
                LastTick = obj;
            });
        }
    }

    private void StrategyHubOnOnCandleReceived(CandleDto obj)
    {
        if (loaded)
        {
            InvokeAsync(() =>
            {
                LastCandle = obj;
            });
        }
    }

}