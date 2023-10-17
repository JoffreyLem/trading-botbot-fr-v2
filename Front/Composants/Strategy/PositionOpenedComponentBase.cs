using System.Collections.ObjectModel;
using Microsoft.AspNetCore.Components;
using StrategyApi.StrategyBackgroundService.Dto.Services;
using StrategyApi.StrategyBackgroundService.Dto.Services.Enum;
using StrategyApi.StrategyBackgroundService.Services;
using Syncfusion.Blazor.Grids;

namespace Front.Composants.Strategy;

public class PositionOpenedComponentBase : ComponentBase
{
    internal ObservableCollection<PositionDto> Positions { get; set; } = new();
    protected SfGrid<PositionDto> Grid { get; set; }
    [Inject] private IStrategyHandlerService _apiStrategyService { get; set; }
    
    [Inject] private IEventBus _eventBus { get; set; }

    protected override async Task OnInitializedAsync()
    {
        var data = await _apiStrategyService.GetOpenedPositions();
        Positions = new ObservableCollection<PositionDto>(data.Positions);
        _eventBus.Subscribe<PositionDto,PositionStateEnum>(StrategyHubOnOnPositionStateReceived);
    }
    
    private void StrategyHubOnOnPositionStateReceived(PositionDto position, PositionStateEnum state)
    {
       
        switch (state)
        {
            case PositionStateEnum.Opened:
                Positions.Add(position);
                break;
            case PositionStateEnum.Updated:
            {
                int selected = Positions
                    .Where((x, i) => x?.Id?.ToString() == position?.Id)
                    .Select((x, i) => i).FirstOrDefault();
                if (selected >= 0 && selected < Positions.Count)
                {
                    Positions[selected] = position;
                }
                else
                {
                    Positions.Add(position);
                }
                break;
            }
            case PositionStateEnum.Closed:
            case PositionStateEnum.Rejected:
            {
                int selected = Positions.Where((x, i) => x.Id == position.Id)
                    .Select((x, i) => i).FirstOrDefault();
                if (selected >= 0 && selected < Positions.Count)
                {
                    Positions.RemoveAt(selected);
                }

                break;
            }
            default:
                break;
        }
    }
}