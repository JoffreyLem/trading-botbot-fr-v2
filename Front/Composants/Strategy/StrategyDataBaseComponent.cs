using AutoMapper;
using Front.Services;
using Microsoft.AspNetCore.Components;
using StrategyApi.StrategyBackgroundService.Dto.Services;
using StrategyApi.StrategyBackgroundService.Dto.Services.Enum;
using StrategyApi.StrategyBackgroundService.Hubs;
using StrategyApi.StrategyBackgroundService.Services;

namespace Front.Composants.Strategy;

public class StrategyDataBaseComponent : ComponentBase , IDisposable
{
    protected bool OnLoading;
    
    protected StrategyInfoDto StrategyInfo = new();
    protected ResultComponent ResultComponent { get; set; }

    protected CandleDto LastCandle { get; set; } = new CandleDto();

    protected TickDto LastTick { get; set; } = new TickDto();
    
    [Inject] protected IStrategyHandlerService _strategyService { get; set; }
    [Inject] private ShowToastService ToastService { get; set; }
    [Inject] private IEventBus _eventBus { get; set; }
    
    [Parameter] public EventCallback StrategyCloseRequested { get; set; }

    protected int SelectedTab { get; set; } = 0;
    
    protected override async Task OnInitializedAsync()
    {
        try
        {
            StrategyInfo = await _strategyService.GetStrategyInfo();
            LastTick = StrategyInfo.LastTick;
            LastCandle = StrategyInfo.LastCandle;
            _eventBus.Subscribe<EventType,string>(StrategyHubOnOnEventReceived);
            _eventBus.Subscribe<CandleDto>(StrategyHubOnOnCandleReceived);
            _eventBus.Subscribe<TickDto>(StrategyHubOnOnTickReceived);
        }
        catch (Exception e)
        {
            ToastService.ShowToastError("Error strategy initialization");
        }
    }

    private void StrategyHubOnOnTickReceived(TickDto obj)
    {
        InvokeAsync(() =>
        {
                LastTick = obj;
        });

    }

    private void StrategyHubOnOnCandleReceived(CandleDto obj)
    {
        InvokeAsync(() =>
        {
            LastCandle = obj;
        });

    }


    private void StrategyHubOnOnEventReceived(EventType eventType, string message)
    {
        InvokeAsync(async () =>
        {
            ToastService.ShowToast(eventType, message);
            if (eventType is EventType.Fatal or EventType.Close)
            {
                await StrategyCloseRequested.InvokeAsync();
            }

            StateHasChanged();
        });
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

    public void Dispose()
    {
        _eventBus.Unsubscribe<EventType,string>(StrategyHubOnOnEventReceived);
        _eventBus.Unsubscribe<CandleDto>(StrategyHubOnOnCandleReceived);
        _eventBus.Unsubscribe<TickDto>(StrategyHubOnOnTickReceived);
    }
}