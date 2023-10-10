using Front.Modeles;
using Front.Services;
using Microsoft.AspNetCore.Components;
using StrategyApi.StrategyBackgroundService.Dto.Services;
using StrategyApi.StrategyBackgroundService.Services;

namespace Front.Composants.Strategy;

public class StrategiInitFormBase : ComponentBase
{
    protected StrategyInitDto _strategyInitDto = new();

    protected bool OnLoading { get; set; } = false;

    [Inject] private IStrategyHandlerService _apiStrategyService { get; set; }

    [Inject] private IApiConnectService _apiConnectService { get; set; }

    [Inject] private ShowToastService ToastService { get; set; }
    
    [Parameter] public EventCallback StrategyFormUpdateRequested { get; set; }

    protected List<string>? StrategyTypes { get; set; }
    protected List<string>? TimeFrames { get; set; }
    protected List<string>? Symbols { get; set; }

    private async Task NotifyParentToUpdate()
    {
        await StrategyFormUpdateRequested.InvokeAsync();
    }

    protected override async Task OnInitializedAsync()
    {
        try
        {
            StrategyTypes = await _apiStrategyService.GetListStrategy();
            TimeFrames = await _apiStrategyService.GetListTimeframes();
            Symbols = await _apiConnectService.GetAllSymbol();
        }
        catch
        {
            ToastService.ShowToastError("Can't initialize strategy form.");
        }
   
    }

    protected async Task InitStrategy()
    {
        try
        {
            OnLoading = true;
            await _apiStrategyService.InitStrategy(_strategyInitDto.StrategyType, _strategyInitDto.Symbol,
                _strategyInitDto.Timeframe, _strategyInitDto.Timeframe2);
            ToastService.ShowToastSuccess("Strategy initialisée");
            await NotifyParentToUpdate();
        }
        catch
        {
            ToastService.ShowToastError("Erreur d'initialisation de la strategy");
        }
        finally
        {
            OnLoading = false;
        }
      
    }
}