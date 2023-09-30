using Front.Services;
using Microsoft.AspNetCore.Components;
using Syncfusion.Blazor.Notifications;

namespace Front.Composants;

public abstract class ToastCustomComponentBase : ComponentBase
{
    protected string ToastContent = "";
    protected string ToastCssClass = "";

    private int ToastFlag = 0;

    protected SfToast ToastObj;
    protected string ToastTitle = "";

    [Inject] private ShowToastService ShowToastService { get; set; }


    protected override async Task OnInitializedAsync()
    {
        ShowToastService.ShowToastTrigger += options =>
        {
            InvokeAsync(() =>
            {
                ToastTitle = options.Title;
                ToastContent = options.Content;
                ToastCssClass = options.CssClass;

                StateHasChanged();
                ToastObj.ShowAsync();
            });
        };
    }
}