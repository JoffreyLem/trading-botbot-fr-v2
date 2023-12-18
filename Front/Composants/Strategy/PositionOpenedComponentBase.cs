using System.Collections.ObjectModel;
using Microsoft.AspNetCore.Components;
using StrategyApi.StrategyBackgroundService;
using StrategyApi.StrategyBackgroundService.Dto.Services;
using StrategyApi.StrategyBackgroundService.Dto.Services.Enum;
using StrategyApi.StrategyBackgroundService.Events;
using StrategyApi.StrategyBackgroundService.Services;
using Syncfusion.Blazor.Grids;

namespace Front.Composants.Strategy;

public class PositionOpenedComponentBase : StrategyIdComponentBase
{
    internal ObservableCollection<PositionDto> Positions { get; set; } = new();
    protected SfGrid<PositionDto> Grid { get; set; }
    [Inject] private IStrategyHandlerService _apiStrategyService { get; set; }

    protected override async Task OnInitializedAsync()
    {
        var data = await _apiStrategyService.GetOpenedPositions(StrategyId);
        Positions = new ObservableCollection<PositionDto>(data.Positions);
        CommandHandler.PositionChangeEvent += CommandHandlerOnPositionChangeEvent;
    }

    private void CommandHandlerOnPositionChangeEvent(object? sender, BackGroundServiceEvent<PositionDto> e)
    {
        if (e.Id == StrategyId)
        {
            var pos = e.EventField;
            switch (pos.PositionState)
            {
                case PositionStateEnum.Opened:
                {
                    var selected = Positions.Where((x, _) => x.Id == pos.Id)
                        .Select((x, _) => x).FirstOrDefault();
                    if (selected is null) Positions.Add(pos);
                    break;
                }
                case PositionStateEnum.Updated:
                {
                    var selected = Positions
                        .Where((x, i) => x?.Id?.ToString() == e?.Id)
                        .Select((x, i) => i).FirstOrDefault();
                    if (selected >= 0 && selected < Positions.Count)
                        Positions[selected] = pos;
                    else
                        Positions.Add(pos);
                    break;
                }
                case PositionStateEnum.Closed:
                case PositionStateEnum.Rejected:
                {
                    var selected = Positions.Where((x, _) => x.Id == e.Id)
                        .Select((x, _) => x).FirstOrDefault();
                    if (selected is not null) Positions.Remove(selected);
                    break;
                }
            }
        }
    }
}