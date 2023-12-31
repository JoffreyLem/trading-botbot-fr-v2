using System.Collections.ObjectModel;
using Microsoft.AspNetCore.Components;
using StrategyApi.StrategyBackgroundService.Dto;
using StrategyApi.StrategyBackgroundService.Services;
using Syncfusion.Blazor.Grids;

namespace Front.Pages.Strategy.Composants;

public class PositionClosedComponentBase : StrategyIdComponentBase
{
    protected SfGrid<PositionDto> Grid { get; set; }
    protected ObservableCollection<PositionDto> Positions { get; set; } = new();

    [Inject] private IStrategyHandlerService _apiStrategyService { get; set; }

    protected override async Task OnInitializedAsync()
    {
        var data = await _apiStrategyService.GetStrategyPositionClosed(StrategyId);
        Positions = new ObservableCollection<PositionDto>(data.Positions);
    }
}