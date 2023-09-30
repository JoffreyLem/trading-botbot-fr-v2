using System.Collections.ObjectModel;
using Microsoft.AspNetCore.Components;
using StrategyApi.StrategyBackgroundService.Dto.Services;
using StrategyApi.StrategyBackgroundService.Services;
using Syncfusion.Blazor.Grids;

namespace Front.Composants.Strategy;

public class PositionClosedComponentBase : ComponentBase
{
    protected SfGrid<PositionDto> Grid { get; set; }
    protected ObservableCollection<PositionDto> Positions { get; set; } = new ObservableCollection<PositionDto>();

    [Inject] private IStrategyHandlerService _apiStrategyService { get; set; }

    protected override async Task OnInitializedAsync()
    {
        ListPositionsDto data = await _apiStrategyService.GetStrategyPosition();
        Positions = new ObservableCollection<PositionDto>(data.Positions);
    }
}