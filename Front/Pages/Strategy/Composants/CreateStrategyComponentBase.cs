using Front.Services;
using Microsoft.AspNetCore.Components;
using RobotAppLibraryV2.Modeles;
using StrategyApi.StrategyBackgroundService.Dto;
using StrategyApi.StrategyBackgroundService.Services;

namespace Front.Pages.Strategy.Composants;

public class CreateStrategyComponentBase : ComponentBase, IDisposable
{
    protected readonly StrategyInitDto StrategyInitDto = new();
    private bool _disposed;
    private string actionType;

    protected bool ShowForm;
    protected bool OnLoading { get; set; }

    protected ResultDto? BacktestResult { get; set; }

    [Inject] private IStrategyHandlerService _apiStrategyService { get; set; }

    [Inject] private IApiConnectService _apiConnectService { get; set; }

    [Inject] private ShowToastService ToastService { get; set; }

    [Inject] private IStrategyGeneratorService StrategyGenerator { get; set; }

    [Parameter] public EventCallback StrategyFormUpdateRequested { get; set; }

    protected List<StrategyFileDto> StrategyTypes { get; set; }
    protected List<string>? TimeFrames { get; set; }
    protected List<SymbolInfo> Symbols { get; set; }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private async Task NotifyParentToUpdate()
    {
        await StrategyFormUpdateRequested.InvokeAsync();
    }

    protected override async Task OnInitializedAsync()
    {
        try
        {
            StrategyTypes = await StrategyGenerator.GetAllStrategyFile();
            TimeFrames = await _apiStrategyService.GetListTimeframes();
            Symbols = await _apiConnectService.GetAllSymbol();
        }
        catch
        {
            ToastService.ShowToastError("Can't initialize strategy form.");
        }
    }


    protected void OnSubmitClicked()
    {
        actionType = "submit";
    }

    protected void OnBacktestClicked()
    {
        actionType = "backtest";
    }

    protected async Task ValidateForm()
    {
        if (actionType == "submit")
            await InitStrategy();
        else if (actionType == "backtest") await RunBacktest();
    }

    private async Task RunBacktest()
    {
        try
        {
            // TODO : Refacto pour harmoniser avec l'autre backtest
            OnLoading = true;
            BacktestResult = (await _apiStrategyService.RunBacktestExternal(StrategyInitDto, 1000, 1, 1))
                .ResultBacktest;
        }
        catch
        {
            ToastService.ShowToastError("Can't run backtest");
        }

        OnLoading = false;
        StateHasChanged();
    }

    private async Task InitStrategy()
    {
        try
        {
            OnLoading = true;
            await _apiStrategyService.InitStrategy(StrategyInitDto);
            ToastService.ShowToastSuccess("Strategy initialisée");
            await NotifyParentToUpdate();
        }
        catch (Exception e)
        {
            ToastService.ShowToastError("Erreur d'initialisation de la strategy");
        }
        finally
        {
            OnLoading = false;
            ShowForm = false;
            StateHasChanged();
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        // if (!_disposed)
        // {
        //     if (disposing)
        //     {
        //         Symbols?.Clear();
        //         Symbols = null;
        //     }
        //     _disposed = true;
        // }
        // GC.Collect();
    }

    protected void CreateStrategy()
    {
        ShowForm = !ShowForm;
    }
}