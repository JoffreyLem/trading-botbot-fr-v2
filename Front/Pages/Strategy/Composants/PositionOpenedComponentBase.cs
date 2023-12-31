using System.Collections.ObjectModel;
using Microsoft.AspNetCore.Components;
using StrategyApi.StrategyBackgroundService;
using StrategyApi.StrategyBackgroundService.Dto;
using StrategyApi.StrategyBackgroundService.Dto.Enum;
using StrategyApi.StrategyBackgroundService.Events;
using StrategyApi.StrategyBackgroundService.Services;
using Syncfusion.Blazor.Grids;

namespace Front.Pages.Strategy.Composants;

public class PositionOpenedComponentBase : StrategyIdComponentBase, IDisposable
{
    internal ObservableCollection<PositionDto> Positions { get; set; } = new();
    protected SfGrid<PositionDto> Grid { get; set; }
    [Inject] private IStrategyHandlerService ApiStrategyService { get; set; }

    public void Dispose()
    {
        Positions.Clear();
        ((IDisposable)Grid).Dispose();
        CommandHandler.PositionChangeEvent -= CommandHandlerOnPositionChangeEvent;
    }

    protected override async Task OnInitializedAsync()
    {
        var data = await ApiStrategyService.GetOpenedPositions(StrategyId);
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
                        .Where((x, i) => x?.Id?.ToString() == pos?.Id)
                        .Select((x, i) => i).FirstOrDefault();
                    if (selected >= 0 && selected < Positions.Count)
                    {
                        Positions[selected].Profit = pos.Profit;
                        Positions[selected].StopLoss = pos.StopLoss;
                        Positions[selected].TakeProfit = pos.TakeProfit;
                    }
                    else
                    {
                        Positions.Add(pos);
                    }

                    break;
                }
                case PositionStateEnum.Closed:
                case PositionStateEnum.Rejected:
                {
                    var selected = Positions.Where((x, _) => x.Id?.ToString() == pos.Id)
                        .Select((x, _) => x).FirstOrDefault();
                    if (selected is not null) Positions.Remove(selected);
                    break;
                }
            }
        }
    }
}