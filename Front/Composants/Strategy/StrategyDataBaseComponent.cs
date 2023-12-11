using Front.Services;
using Microsoft.AspNetCore.Components;
using StrategyApi.StrategyBackgroundService;
using StrategyApi.StrategyBackgroundService.Dto.Services;
using StrategyApi.StrategyBackgroundService.Dto.Services.Enum;
using StrategyApi.StrategyBackgroundService.Events;
using StrategyApi.StrategyBackgroundService.Services;
using Syncfusion.Blazor.Buttons;

namespace Front.Composants.Strategy;

public class StrategyDataBaseComponent : ComponentBase, IDisposable
{
    protected bool OnLoading;

    protected StrategyInfoDto StrategyInfo = new();
    protected ResultComponent ResultComponent { get; set; }

    protected CandleDto LastCandle { get; set; } = new();

    protected TickDto LastTick { get; set; }

    [Inject] protected IStrategyHandlerService _strategyService { get; set; }
    [Inject] private ShowToastService ToastService { get; set; }


    [Parameter] public EventCallback StrategyCloseRequested { get; set; }

    protected int SelectedTab { get; set; } = 0;

    public void Dispose()
    {
        CommandHandler.StategyEvent += CommandHandlerOnStategyEvent;
        CommandHandler.CandleEvent += CommandHandlerOnCandleEvent;
        CommandHandler.TickEvent += CommandHandlerOnTickEvent;
    }

    protected override async Task OnInitializedAsync()
    {
        try
        {
            StrategyInfo = await _strategyService.GetStrategyInfo();
            LastTick = StrategyInfo.LastTick;
            LastCandle = StrategyInfo.LastCandle;
            CommandHandler.StategyEvent += CommandHandlerOnStategyEvent;
            CommandHandler.CandleEvent += CommandHandlerOnCandleEvent;
            CommandHandler.TickEvent += CommandHandlerOnTickEvent;
        }
        catch (Exception e)
        {
            ToastService.ShowToastError("Error strategy initialization");
        }
    }

    private void CommandHandlerOnStategyEvent(object? sender, StrategyEventEvent e)
    {
        InvokeAsync(async () =>
        {
            ToastService.ShowToast(e.EventType, e.Message);
            if (e.EventType is EventType.Fatal or EventType.Close)
                await StrategyCloseRequested.InvokeAsync();
            else if (e.EventType == EventType.Update) StrategyInfo = await _strategyService.GetStrategyInfo();

            StateHasChanged();
        });
    }


    private void CommandHandlerOnTickEvent(object? sender, TickDto e)
    {
        InvokeAsync(() =>
        {
            LastTick = e;
            StateHasChanged();
        });
    }

    private void CommandHandlerOnCandleEvent(object? sender, CandleDto e)
    {
        InvokeAsync(() =>
        {
            LastCandle = e;
            StateHasChanged();
        });
    }


    private void StrategyHubOnOnEventReceived(EventType eventType, string message)
    {
        InvokeAsync(async () =>
        {
            ToastService.ShowToast(eventType, message);
            if (eventType is EventType.Fatal or EventType.Close) await StrategyCloseRequested.InvokeAsync();

            StateHasChanged();
        });
    }

    protected async void OnCanRunChange(ChangeEventArgs<bool?> args)
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
}