using Front.Pages.StrategyCreator.Composants;
using Front.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using StrategyApi.StrategyBackgroundService.Dto;
using StrategyApi.StrategyBackgroundService.Dto.Response;
using StrategyApi.StrategyBackgroundService.Services;
using Syncfusion.Blazor.Inputs;

namespace Front.Pages.StrategyCreator;

public class StrategyCreatorIndexBase : ComponentBase
{
    protected SfUploader UploadObj;
    protected bool OnLoading { get; set; }
    protected bool CreationStrategyError { get; set; }
    protected List<string> ErrorCreationList { get; set; } = new();

    protected FileUploadModal fileUploadModal { get; set; }
    protected List<StrategyFileDto> StrategyFiles { get; set; }

    protected StrategyFileDto currentStrategyFile { get; set; }
    [Inject] private IStrategyGeneratorService StrategyGenerator { get; set; }
    [Inject] private ShowToastService ToastService { get; set; }

    [Inject] private IJSRuntime JsRuntime { get; set; }

    protected override async Task OnInitializedAsync()
    {
        try
        {
            await GetStrategyFile();
        }
        catch
        {
            ToastService.ShowToastError("Can't initialize strategy list");
        }
    }

    private async Task GetStrategyFile()
    {
        CreationStrategyError = false;
        ErrorCreationList.Clear();
        StrategyFiles = await StrategyGenerator.GetAllStrategyFile();
        StateHasChanged();
    }

    protected async Task OnChange(UploadChangeEventArgs args)
    {
        try
        {
            CreationStrategyError = false;
            OnLoading = true;
            var file = args.Files.FirstOrDefault();

            if (file is not null)
            {
                using var msStream = new MemoryStream();
                await file.File.OpenReadStream().CopyToAsync(msStream);
                var byteStream = msStream.ToArray();


                var rspCreation = await StrategyGenerator.CreateNewStrategy(byteStream);

                if (!rspCreation.Created)
                {
                    CreationStrategyError = true;
                    ErrorCreationList.Clear();
                    ErrorCreationList.AddRange(rspCreation.Errors);
                    ToastService.ShowToastError("La création de la strategy n'a pas réussi");
                }
                else
                {
                    StrategyFiles.Add(rspCreation.StrategyFile);
                }

                StateHasChanged();
            }
        }
        catch (Exception)
        {
            ToastService.ShowToastError("Erreur a la création de la strategy");
        }

        OnLoading = false;
    }

    protected async Task DeleteStrategyFile(int strategyFileId)
    {
        try
        {
            OnLoading = true;
            CreationStrategyError = true;
            ErrorCreationList.Clear();
            await StrategyGenerator.DeleteStrategyFile(strategyFileId);
            StrategyFiles.Remove(StrategyFiles.Where(x => x.Id == strategyFileId).FirstOrDefault());
            StateHasChanged();
        }
        catch (Exception)
        {
            ToastService.ShowToastError("Can't delete strategy file");
        }

        OnLoading = false;
    }


    protected async Task DownloadStrategyFile(StrategyFileDto strategyFile)
    {
        var fileName = $"{strategyFile.Name}.cs";
        var fileType = "text/plain";
        var dataStream = strategyFile.Data;


        await JsRuntime.InvokeVoidAsync("downloadFileFromByteArray", fileName, fileType, dataStream);
    }

    protected void OnClear()
    {
        CreationStrategyError = false;
        ErrorCreationList.Clear();
        StateHasChanged();
    }

    protected void OpenModal(StrategyFileDto strategyFile)
    {
        CreationStrategyError = true;
        ErrorCreationList.Clear();
        currentStrategyFile = strategyFile;
        fileUploadModal.OpenModal();
    }

    protected async Task HandleModalClose(StrategyUpdateResponseDto strategyUpdateResponseDto)
    {
        if (strategyUpdateResponseDto.Created == false)
        {
            CreationStrategyError = true;
            ErrorCreationList.Clear();
            ErrorCreationList.AddRange(strategyUpdateResponseDto.Errors);
            ToastService.ShowToastError("L'update de la strategy n'a pas réussis");
            StateHasChanged();
        }
        else
        {
            await GetStrategyFile();
        }
    }
}