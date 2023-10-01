using System.Collections.ObjectModel;
using Microsoft.AspNetCore.Components;
using StrategyApi.StrategyBackgroundService.Dto.Services;
using StrategyApi.StrategyBackgroundService.Services;
using Syncfusion.Blazor.Grids;

namespace Front.Composants.Strategy;

public class PositionOpenedComponentBase : ComponentBase
{
    internal ObservableCollection<PositionDto> Positions { get; set; } = new();
    protected SfGrid<PositionDto> Grid { get; set; }
    [Inject] private IStrategyHandlerService _apiStrategyService { get; set; }

    protected override async Task OnInitializedAsync()
    {
        var data = await _apiStrategyService.GetOpenedPositions();
        Positions = new ObservableCollection<PositionDto>(data.Positions);
    }
}