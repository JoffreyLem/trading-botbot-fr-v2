using Front.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using StrategyApi.StrategyBackgroundService.Dto.Services.Dto;
using StrategyApi.StrategyBackgroundService.Dto.Services.Enum;
using StrategyApi.StrategyBackgroundService.Services;
using Syncfusion.Blazor.DropDowns;

namespace Front.Composants.ApiProvider;

public class ApiComponentBase : ComponentBase
{
    protected bool IsConnected { get; set; }
    protected bool OnLoading { get; set; }
    
    protected bool ApiHandlerListEnabled { get; set; }
    protected SfDropDownList<string, string> _dropDownList { get; set; }
    [Inject] private IApiConnectService ApiConnectService { get; set; }
    [Inject] private ShowToastService ToastService { get; set; }
    protected List<string> ApiProviders { get; set; } = new List<string>();
    protected ConnectDto ConnectDto { get; set; } = new ConnectDto();
    protected string? ApiSelected { get; set; } = "";
    
    [Parameter]
    public EventCallback ApiComponentUpdateRequested { get; set; }
    
    private async Task NotifyParentToUpdate()
    {
        await ApiComponentUpdateRequested.InvokeAsync();
    }
    
    protected override async Task OnInitializedAsync()
    {
        try
        {
       
            if (await ApiConnectService.IsConnected() == ConnexionStateEnum.Connected)
            {
                IsConnected = true;
                ApiHandlerListEnabled = false;
            }
            else
            {
                ApiHandlerListEnabled = true;
            }
            await GetListHandler();
        }
        catch (Exception e)
        {
            ToastService.ShowToastError(e.Message);
        }
    }


    private async Task GetListHandler()
    {
        try
        {
            ApiProviders = await ApiConnectService.GetListHandler();
            if (IsConnected)
            {
                string? data = await ApiConnectService.GetTypeHandler();
                data = data.Replace("\"", "");
                ApiSelected = ApiProviders.First(x => x == data);
                
            }
        }
        catch (Exception e)
        {
            ToastService.ShowToastError(e);
        }
    }


    protected async Task Disconnect()
    {
        try
        {
            if (await ApiConnectService.IsConnected() == ConnexionStateEnum.Connected)
            {
                OnLoading = true;
                await ApiConnectService.Disconnect();
                OnLoading = false;
                ToastService.ShowToastSuccess("Deconnecter");
                IsConnected = false;
                ApiSelected = null;
                ApiHandlerListEnabled = true;
                await NotifyParentToUpdate();
            }
            else
            {
                ToastService.ShowToastWarning("API Non connecter");
            }
        }
        catch (Exception e)
        {
            OnLoading = false;
            ToastService.ShowToastError(e);
        }
    }


    protected async Task Connect(EditContext obj)
    {
        try
        {
            OnLoading = true;
            await ApiConnectService.Connect(ConnectDto.User,ConnectDto.Pwd);
            ToastService.ShowToastSuccess("Connecter");
            IsConnected = true;
            ApiHandlerListEnabled = false;
            ConnectDto = new ConnectDto();
            await NotifyParentToUpdate();
        }
        catch (Exception e)
        {
            OnLoading = false;
            ToastService.ShowToastError(e);
        }
        finally
        {
            OnLoading = false;
        }
    }

    protected async Task OnApiProviderValueChange(ChangeEventArgs<string, string> obj)
    {
        try
        {
            if (obj.IsInteracted && !IsConnected)
            {
                ApiHandlerEnum apiHandlerEnum = Enum.Parse<ApiHandlerEnum>(obj.Value);
                await ApiConnectService.InitHandler(apiHandlerEnum);
                ToastService.ShowToastSuccess($"Handler set to {obj.Value}");
            }
        }
        catch (Exception e)
        {
            ApiSelected = obj.PreviousItemData;
            ToastService.ShowToastError(e);
        }
    }
}