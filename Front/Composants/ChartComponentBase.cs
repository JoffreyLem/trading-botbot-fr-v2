using System.Collections.ObjectModel;
using Front.Services;
using Microsoft.AspNetCore.Components;
using RobotAppLibraryV2.Modeles;
using StrategyApi.StrategyBackgroundService.Dto.Services;
using StrategyApi.StrategyBackgroundService.Services;
using Syncfusion.Blazor;
using Syncfusion.Blazor.Charts;

namespace Front.Composants;

public class ChartComponentBase : ComponentBase
{
    [Inject] protected IStrategyHandlerService _strategyService { get; set; }
    [Inject] private ShowToastService ToastService { get; set; }
    [Inject] private IEventBus _eventBus { get; set; }

    protected CandleDto LastCandle
    {
        get
        {
            if (Candles.Last().Open == 0 && Candles.Last().High == 0 && Candles.Last().Low == 0 &&
                Candles.Last().Close == 0)
            {
                return Candles[^2];
            }

            return Candles.Last();
        }
    }

    protected ObservableCollection<CandleDto> Candles { get; set; } = new ObservableCollection<CandleDto>();

    protected string translateY = "-5px";
    protected string loadClass = "stockchartloader";
    protected string loadDiv = "stockchartdiv";
    
    protected override async Task OnInitializedAsync()
    {
        try
        {
            Candles = new ObservableCollection<CandleDto>((await _strategyService.GetChart()));
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
        StateHasChanged();
    }
    
    public void RangeChanged(StockChartRangeChangeEventArgs args)
    {
        // Here you can customize your code
    }

}