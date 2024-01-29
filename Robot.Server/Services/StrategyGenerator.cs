using System.Text;
using AutoMapper;
using Robot.DataBase.Modeles;
using Robot.DataBase.Repositories;
using Robot.Server.Dto.Response;
using RobotAppLibraryV2.ApiConnector.Exceptions;
using RobotAppLibraryV2.StrategyDynamiqCompiler;
using ILogger = Serilog.ILogger;

namespace Robot.Server.Services;

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

    public async Task<StrategyCreatedResponseDto> CreateNewStrategy(string data)
    {
        var strategyCreateRsp = new StrategyCreatedResponseDto();
        try
        {
            var sourceCode = data;
            var compiledCode = StrategyDynamiqCompiler.TryCompileSourceCode(sourceCode);

            var context = new CustomLoadContext();
            using var stream = new MemoryStream(compiledCode);
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
            {
                Data = Encoding.UTF8.GetBytes(data), Name = nameValue, Version = versionValue,
                LastDateUpdate = DateTime.UtcNow
            };

            await _strategyFileRepository.AddAsync(strategyFile);

            strategyCreateRsp.Created = true;


            if (instance is IDisposable disposable) disposable.Dispose();

            context.Unload();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
        catch (System.Exception e) when (e is not CompilationException)
        {
            _logger?.Error(e, "An exception occurred while creating the new strategy");
            throw new ApiException("An exception occurred while creating the new strategy");
        }

        return strategyCreateRsp;
    }

    public async Task<StrategyFileDto> GetStrategyFile(int id)
    {
        try
        {
            var data = await _strategyFileRepository.GetByIdAsync(id);

            return new StrategyFileDto
            {
                Id = data.Id,
                Data = Encoding.UTF8.GetString(data.Data),
                LastDateUpdate = data.LastDateUpdate,
                Name = data.Name,
                Version = data.Version
            };
        }
        catch (System.Exception e)
        {
            _logger.Error(e, "Can't get all strategy file in db");
            throw new ApiException("Can't get all strategy file in db");
        }
    }

    public async Task<StrategyUpdateResponseDto> UpdateStrategyFile(int id, string data)
    {
        var strategyCreateRsp = new StrategyUpdateResponseDto();
        try
        {
            var sourceCode = data;

            var compiledCode = StrategyDynamiqCompiler.TryCompileSourceCode(sourceCode);


            var context = new CustomLoadContext();
            using var stream = new MemoryStream(compiledCode);
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

            var strategyFileSelected = await _strategyFileRepository.GetByIdAsync(id);

            strategyFileSelected.Name = nameValue;
            strategyFileSelected.Version = versionValue;
            strategyFileSelected.LastDateUpdate = DateTime.UtcNow;
            strategyFileSelected.Data = Encoding.UTF8.GetBytes(sourceCode);

            await _strategyFileRepository.UpdateAsync(strategyFileSelected);

            strategyCreateRsp.Created = true;


            if (instance is IDisposable disposable) disposable.Dispose();

            context.Unload();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
        catch (System.Exception e) when (e is not CompilationException)
        {
            _logger?.Error(e, "An exception occurred while updating strategyfile {id}", id);
            throw new ApiException($"An error occured while updating strategy {id}");
        }

        return strategyCreateRsp;
    }

    public async Task<List<StrategyFileDto>> GetAllStrategyFile()
    {
        try
        {
            var data = await _strategyFileRepository.GetAllAsync();

            return data.Select(x => new StrategyFileDto
            {
                Id = x.Id,
                Version = x.Version,
                Name = x.Name,
                LastDateUpdate = x.LastDateUpdate
            }).ToList();
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
}