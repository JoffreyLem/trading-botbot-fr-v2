using AutoMapper;
using Front.Services;
using Microsoft.AspNetCore.Components;
using StrategyApi.StrategyBackgroundService.Dto.Services;
using StrategyApi.StrategyBackgroundService.Dto.Services.Enum;
using StrategyApi.StrategyBackgroundService.Hubs;
using StrategyApi.StrategyBackgroundService.Services;

namespace Front.Composants.Strategy;

public class StrategyDataBaseComponent : ComponentBase
{
    protected bool OnLoading;
    
    protected StrategyInfoDto StrategyInfo = new();

    protected PositionClosedComponent PositionClosedComponent { get; set; }

    protected PositionOpenedComponent PositionOpenedCompoennt { get; set; }

    protected ResultComponent ResultComponent { get; set; }

    [Inject] protected IStrategyHandlerService _strategyService { get; set; }
    [Inject] private ShowToastService ToastService { get; set; }
    [Parameter] public EventCallback StrategyCloseRequested { get; set; }

    protected int SelectedTab { get; set; } = 0;
    
    protected override async Task OnInitializedAsync()
    {
        try
        {
            StrategyInfo = await _strategyService.GetStrategyInfo();
            StrategyHub.OnEventReceived += StrategyHubOnOnEventReceived;
            StrategyHub.OnCandleReceived += StrategyHubOnOnCandleReceived;
            StrategyHub.OnTickReceived += StrategyHubOnOnTickReceived;
            StrategyHub.OnPositionStateReceived += StrategyHubOnOnPositionStateReceived;
      
        }
        catch (Exception e)
        {
            ToastService.ShowToastError("Error strategy initialization");
        }
    }

    private void StrategyHubOnOnPositionStateReceived(PositionDto position, PositionStateEnum state)
    {
        switch (state)
        {
            case PositionStateEnum.Opened:
                PositionOpenedCompoennt.Positions.Add(position);
                break;
            case PositionStateEnum.Updated:
            {
                int selected = PositionOpenedCompoennt.Positions
                    .Where((x, i) => x?.Id?.ToString() == position?.Id)
                    .Select((x, i) => i).FirstOrDefault();
                if (selected >= 0 && selected < PositionOpenedCompoennt.Positions.Count)
                {
                    PositionOpenedCompoennt.Positions[selected] = position;
                }

                break;
            }
            case PositionStateEnum.Closed:
            case PositionStateEnum.Rejected:
            {
                int selected = PositionOpenedCompoennt.Positions.Where((x, i) => x.Id == position.Id)
                    .Select((x, i) => i).FirstOrDefault();
                if (selected >= 0 && selected < PositionOpenedCompoennt.Positions.Count)
                {
                    PositionOpenedCompoennt.Positions.RemoveAt(selected);
                }
                break;
            }
        }
    }

    private void StrategyHubOnOnTickReceived(TickDto obj)
    {
        StrategyInfo.LastTick = obj;
        StateHasChanged();
    }

    private void StrategyHubOnOnCandleReceived(CandleDto obj)
    {
        StrategyInfo.LastCandle = obj;
        StateHasChanged();
    }

    private async void StrategyHubOnOnEventReceived(EventType eventType, string message)
    {
        ToastService.ShowToast(eventType, message);
        if (eventType is EventType.Fatal or EventType.Close)
        {
            await StrategyCloseRequested.InvokeAsync();
        }
        StateHasChanged();
    }
    
    protected async void OnCanRunChange(Syncfusion.Blazor.Buttons.ChangeEventArgs<bool?> args)
    {
        try
        {
            OnLoading = true;
            await _strategyService.SetCanRun(args.Checked.GetValueOrDefault());
            ToastService.ShowToastSuccess("Can run updated");
        }
        catch (Exception e)
        {
            ToastService.ShowToastError(e);
        }
        finally
        {
            OnLoading = false;
            StateHasChanged();
        }
    }

    protected async void OnSecureControlePositionChange(Syncfusion.Blazor.Buttons.ChangeEventArgs<bool> args)
    {
        try
        {
            OnLoading = true;
            await _strategyService.SetSecureControlPosition(args.Checked);
            ToastService.ShowToastSuccess("Secure control position updated");
        }
        catch (Exception e)
        {
            ToastService.ShowToastError(e);
        }
        finally
        {
            OnLoading = false;
            StateHasChanged();
        }
    }
}