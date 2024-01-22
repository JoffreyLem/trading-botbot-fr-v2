using Robot.DataBase.Modeles;

namespace Robot.DataBase.Repositories;

public interface IStrategyFileRepository
{
    Task<List<StrategyFile>> GetAllAsync();

    Task<StrategyFile> GetByIdAsync(int id);
    Task AddAsync(StrategyFile strategyFile);
    Task UpdateAsync(StrategyFile strategyFile);
    Task DeleteAsync(int id);
}