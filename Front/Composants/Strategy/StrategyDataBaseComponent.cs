using Front.Services;
using Microsoft.AspNetCore.Components;
using RobotAppLibraryV2.Modeles.events;
using StrategyApi.StrategyBackgroundService;
using StrategyApi.StrategyBackgroundService.Dto.Services;
using StrategyApi.StrategyBackgroundService.Events;
using StrategyApi.StrategyBackgroundService.Services;
using Syncfusion.Blazor.Buttons;

namespace Front.Composants.Strategy;

public class StrategyDataBaseComponent : StrategyIdComponentBase, IDisposable
{
    protected bool OnLoading;

    protected StrategyInfoDto StrategyInfo = new();
    protected CandleDto LastCandle { get; set; } = new();
    protected TickDto LastTick { get; set; }
    [Inject] protected IStrategyHandlerService _strategyService { get; set; }
    [Inject] private ShowToastService ToastService { get; set; }
    [Parameter] public EventCallback StrategyCloseRequested { get; set; }
    protected int SelectedTab { get; set; } = 0;

    public void Dispose()
    {
        CommandHandler.CandleEvent -= CommandHandlerOnCandleEvent;
        CommandHandler.TickEvent -= CommandHandlerOnTickEvent;
        CommandHandler.StrategyDisabled -= CommandHandlerOnStrategyDisabled;
        GC.SuppressFinalize(this);
    }

    protected override async Task OnInitializedAsync()
    {
        try
        {
            StrategyInfo = await _strategyService.GetStrategyInfo(StrategyId);
            LastTick = StrategyInfo.LastTick;
            LastCandle = StrategyInfo.LastCandle;

            CommandHandler.CandleEvent += CommandHandlerOnCandleEvent;
            CommandHandler.TickEvent += CommandHandlerOnTickEvent;
            CommandHandler.StrategyDisabled += CommandHandlerOnStrategyDisabled;
        }
        catch (Exception)
        {
            ToastService.ShowToastError("Error strategy initialization");
        }
    }

    private void CommandHandlerOnStrategyDisabled(object? sender, RobotEvent<string> e)
    {
        if (e.Id == StrategyId)
            InvokeAsync(() =>
            {
                StrategyInfo.StrategyDisabled = true;
                StateHasChanged();
            });
    }


    private void CommandHandlerOnTickEvent(object? sender, BackGroundServiceEvent<TickDto> e)
    {
        if (e.Id == StrategyId)
            InvokeAsync(() =>
            {
                LastTick = e.EventField;
                StateHasChanged();
            });
    }

    private void CommandHandlerOnCandleEvent(object? sender, BackGroundServiceEvent<CandleDto> e)
    {
        if (e.Id == StrategyId)
            InvokeAsync(() =>
            {
                LastCandle = e.EventField;
                StateHasChanged();
            });
    }


    protected async void OnCanRunChange(ChangeEventArgs<bool> args)
    {
        try
        {
            OnLoading = true;
            await _strategyService.SetCanRun(StrategyId, args.Checked);
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

    protected async Task DeleteStrategy()
    {
        try
        {
            OnLoading = true;
            await _strategyService.CloseStrategy(StrategyId);
            await StrategyCloseRequested.InvokeAsync();
            ToastService.ShowToastSuccess("Strategy supprimée");
        }
        catch (Exception e)
        {
            ToastService.ShowToastError(e);
        }
        finally
        {
            OnLoading = false;
            GC.Collect();
        }
    }
}