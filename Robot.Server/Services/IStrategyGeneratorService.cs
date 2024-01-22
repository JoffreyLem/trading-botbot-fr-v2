using Robot.Server.Dto;
using Robot.Server.Dto.Response;

namespace Robot.Server.Services;

public interface IStrategyGeneratorService
{
    Task<StrategyCreatedResponseDto> CreateNewStrategy(string data);

    Task<StrategyFileDto> GetStrategyFile(int id);

    Task<List<StrategyFileDto>> GetAllStrategyFile();

    Task DeleteStrategyFile(int id);

    Task<StrategyUpdateResponseDto> UpdateStrategyFile(StrategyFileDto strategyFile);
}