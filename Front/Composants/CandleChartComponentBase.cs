using System.Collections.ObjectModel;
using Front.Services;
using Microsoft.AspNetCore.Components;
using StrategyApi.StrategyBackgroundService;
using StrategyApi.StrategyBackgroundService.Dto.Services;
using StrategyApi.StrategyBackgroundService.Services;
using Syncfusion.Blazor.Charts;

namespace Front.Composants;

public class CandleChartComponentBase : ComponentBase, IDisposable
{
    protected TickDto LastTick;
    protected string loadClass = "stockchartloader";
    protected string loadDiv = "stockchartdiv";


    private bool loaded;

    protected string translateY = "-5px";
    [Inject] protected IStrategyHandlerService _strategyService { get; set; }
    [Inject] private ShowToastService ToastService { get; set; }


    protected CandleDto? LastCandle
    {
        get => Candles.LastOrDefault();
        set => Candles[^1] = value;
    }


    protected ObservableCollection<CandleDto> Candles { get; set; } = new();

    public void Dispose()
    {
        CommandHandler.CandleEvent -= CommandHandlerOnCandleEvent;
        CommandHandler.TickEvent -= CommandHandlerOnTickEvent;
    }

    protected override async Task OnInitializedAsync()
    {
        try
        {
            Candles = new ObservableCollection<CandleDto>(await _strategyService.GetChart());
            CommandHandler.CandleEvent += CommandHandlerOnCandleEvent;
            CommandHandler.TickEvent += CommandHandlerOnTickEvent;
        }
        catch
        {
            ToastService.ShowToastError("Can't load graphique");
        }
    }

    private void CommandHandlerOnTickEvent(object? sender, TickDto e)
    {
        InvokeAsync(() =>
        {
            if (loaded) LastTick = e;
            StateHasChanged();
        });
    }

    private void CommandHandlerOnCandleEvent(object? sender, CandleDto e)
    {
        InvokeAsync(() =>
        {
            if (loaded)
            {
                if (LastCandle?.Date == e.Date)
                    LastCandle = e;
                else
                    Candles.Add(e);

                StateHasChanged();
            }
        });
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
}