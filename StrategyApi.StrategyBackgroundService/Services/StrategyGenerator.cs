using AutoMapper;
using Microsoft.CodeAnalysis;
using RobotAppLibraryV2.StrategyDynamiqCompiler;
using Serilog;
using StrategyApi.DataBase.Modeles;
using StrategyApi.DataBase.Repositories;
using StrategyApi.StrategyBackgroundService.Dto;
using StrategyApi.StrategyBackgroundService.Dto.Response;

namespace StrategyApi.StrategyBackgroundService.Services;

public class StrategyGeneratorService : IStrategyGeneratorService
{
    private readonly ILogger _logger;
    private readonly IMapper _mapper;
    private readonly IStrategyFileRepository _strategyFileRepository;

    public StrategyGeneratorService(IStrategyFileRepository strategyFileRepository, ILogger logger, IMapper mapper)
    {
        _strategyFileRepository = strategyFileRepository;
        _mapper = mapper;
        _logger = logger.ForContext<StrategyGeneratorService>();
    }

    public async Task<StrategyCreatedResponseDto> CreateNewStrategy(byte[] file)
    {
        var strategyCreateRsp = new StrategyCreatedResponseDto();
        try
        {
            var sourceCode = StrategyDynamiqCompiler.ConvertByteToString(file);

            if (StrategyDynamiqCompiler.TryCompileSourceCode(sourceCode, out var compileResult, out var compiledBytes,
                    out var compileErrors))
            {
                var context = new CustomLoadContext();
                using var stream = new MemoryStream(compiledBytes);
                var assembly = context.LoadFromStream(stream);

                var className = StrategyDynamiqCompiler.GetFirstClassName(sourceCode);

                if (string.IsNullOrEmpty(className))
                {
                    strategyCreateRsp.Created = false;
                    strategyCreateRsp.Errors.Add("Class name not found in file");
                    return strategyCreateRsp;
                }

                var type = assembly.GetType(className);
                var instance = Activator.CreateInstance(type);

                var nameValue = (string)type.GetProperty("Name")?.GetValue(instance);
                var versionValue = (string)type.GetProperty("Version")?.GetValue(instance);

                if (string.IsNullOrEmpty(nameValue) || string.IsNullOrEmpty(versionValue))
                {
                    strategyCreateRsp.Created = false;
                    strategyCreateRsp.Errors.Add("Name or version not found");
                    return strategyCreateRsp;
                }

                var strategyFile = new StrategyFile
                    { Data = file, Name = nameValue, Version = versionValue, LastDateUpdate = DateTime.UtcNow };

                await _strategyFileRepository.AddAsync(strategyFile);

                strategyCreateRsp.Created = true;
                strategyCreateRsp.StrategyFile = _mapper.Map<StrategyFileDto>(strategyFile);
                ;

                if (instance is IDisposable disposable) disposable.Dispose();

                context.Unload();
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
            else
            {
                HandleCompilationErrors(compileErrors, strategyCreateRsp);
            }
        }
        catch (System.Exception e)
        {
            _logger?.Error(e, "An exception occurred while creating a new strategy.");
            strategyCreateRsp.Created = false;
        }

        return strategyCreateRsp;
    }

    public async Task<StrategyUpdateResponseDto> UpdateStrategyFile(StrategyFileDto strategyFile)
    {
        var strategyCreateRsp = new StrategyUpdateResponseDto();
        try
        {
            var sourceCode = StrategyDynamiqCompiler.ConvertByteToString(strategyFile.Data);

            if (StrategyDynamiqCompiler.TryCompileSourceCode(sourceCode, out var compileResult, out var compiledBytes,
                    out var compileErrors))
            {
                var context = new CustomLoadContext();
                using var stream = new MemoryStream(compiledBytes);
                var assembly = context.LoadFromStream(stream);

                var className = StrategyDynamiqCompiler.GetFirstClassName(sourceCode);
                if (string.IsNullOrEmpty(className))
                {
                    strategyCreateRsp.Created = false;
                    strategyCreateRsp.Errors.Add("Class name not found in file");
                    return strategyCreateRsp;
                }

                var type = assembly.GetType(className);
                var instance = Activator.CreateInstance(type);

                var nameValue = (string)type.GetProperty("Name")?.GetValue(instance);
                var versionValue = (string)type.GetProperty("Version")?.GetValue(instance);

                if (string.IsNullOrEmpty(nameValue) || string.IsNullOrEmpty(versionValue))
                {
                    strategyCreateRsp.Created = false;
                    strategyCreateRsp.Errors.Add("Name or version not found");
                    return strategyCreateRsp;
                }

                var strategyFileSelected = await _strategyFileRepository.GetByIdAsync(strategyFile.Id);

                strategyFileSelected.Name = nameValue;
                strategyFileSelected.Version = versionValue;
                strategyFileSelected.LastDateUpdate = DateTime.UtcNow;
                strategyFileSelected.Data = strategyFile.Data;

                await _strategyFileRepository.UpdateAsync(strategyFileSelected);

                strategyCreateRsp.Created = true;
                strategyCreateRsp.StrategyFile = strategyFile;

                if (instance is IDisposable disposable) disposable.Dispose();

                context.Unload();
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
            else
            {
                HandleCompilationErrors(compileErrors, strategyCreateRsp);
            }
        }
        catch (System.Exception e)
        {
            _logger?.Error(e, "An exception occurred while updating strategyfile {id}", strategyFile.Id);
            strategyCreateRsp.Created = false;
        }

        return strategyCreateRsp;
    }

    public async Task<List<StrategyFileDto>> GetAllStrategyFile()
    {
        try
        {
            var data = await _strategyFileRepository.GetAllAsync();

            return data.Select(x => _mapper.Map<StrategyFileDto>(x)).ToList();
        }
        catch (System.Exception e)
        {
            _logger.Error(e, "Can't get all strategy file in db");
            throw new System.Exception();
        }
    }

    public async Task DeleteStrategyFile(int id)
    {
        try
        {
            await _strategyFileRepository.DeleteAsync(id);
        }
        catch (System.Exception e)
        {
            _logger.Error(e, "Can't delete strategy {id}", id);
            throw new System.Exception();
        }
    }


    private void HandleCompilationErrors(IEnumerable<Diagnostic> compileErrors, StrategyCreatedResponseDto response)
    {
        foreach (var error in compileErrors)
            if (error.Severity == DiagnosticSeverity.Error)
                response.Errors?.Add(error.ToString());
    }
}