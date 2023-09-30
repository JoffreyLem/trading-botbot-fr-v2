using Microsoft.AspNetCore.Components;
using StrategyApi.Dto.Enum;
using StrategyApi.StrategyBackgroundService.Services;

namespace Front.Pages;

public class IndexBase : ComponentBase
{
    [Inject] protected IApiConnectService ApiConnectService { get; set; }

    protected bool IsConnected = false;

    protected override async Task OnInitializedAsync()
    {
        var isConnected = await ApiConnectService.IsConnected();

        if (isConnected == ConnexionStateEnum.Connected)
        {
            IsConnected = true;
        }
        
    
    }
}