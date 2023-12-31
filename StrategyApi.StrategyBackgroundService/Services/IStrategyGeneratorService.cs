using StrategyApi.StrategyBackgroundService.Dto;
using StrategyApi.StrategyBackgroundService.Dto.Response;

namespace StrategyApi.StrategyBackgroundService.Services;

public interface IStrategyGeneratorService
{
    Task<StrategyCreatedResponseDto> CreateNewStrategy(byte[] file);

    Task<List<StrategyFileDto>> GetAllStrategyFile();

    Task DeleteStrategyFile(int id);

    Task<StrategyUpdateResponseDto> UpdateStrategyFile(StrategyFileDto strategyFile);
}