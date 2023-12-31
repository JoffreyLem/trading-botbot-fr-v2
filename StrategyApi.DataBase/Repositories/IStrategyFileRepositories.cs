using StrategyApi.DataBase.Modeles;

namespace StrategyApi.DataBase.Repositories;

public interface IStrategyFileRepository
{
    Task<List<StrategyFile>> GetAllAsync();
    Task<List<StrategyFile>> GetAllWithoutDataAsync();
    Task<StrategyFile> GetByIdAsync(int id);
    Task AddAsync(StrategyFile strategyFile);
    Task UpdateAsync(StrategyFile strategyFile);
    Task DeleteAsync(int id);
}