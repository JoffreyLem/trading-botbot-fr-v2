using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StrategyApi.DataBase.DbContext;
using StrategyApi.DataBase.Modeles;

namespace StrategyApi.DataBase.Repositories;

public class StrategyFileRepository : IStrategyFileRepository
{
    private readonly IServiceScopeFactory _scopeFactory;

    public StrategyFileRepository(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public async Task<List<StrategyFile>> GetAllAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<StrategyContext>();
        return await dbContext.StrategyFiles.ToListAsync();
    }

    public async Task<List<StrategyFile>> GetAllWithoutDataAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<StrategyContext>();
        return await dbContext.StrategyFiles.Select(x => new StrategyFile
        {
            Name = x.Name,
            Id = x.Id,
            Version = x.Version
        }).ToListAsync();
    }

    public async Task<StrategyFile> GetByIdAsync(int id)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<StrategyContext>();
        return await dbContext.StrategyFiles.FindAsync(id);
    }

    public async Task AddAsync(StrategyFile strategyFile)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<StrategyContext>();
        dbContext.StrategyFiles.Add(strategyFile);
        await dbContext.SaveChangesAsync();
    }

    public async Task UpdateAsync(StrategyFile strategyFile)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<StrategyContext>();
        dbContext.StrategyFiles.Update(strategyFile);
        await dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<StrategyContext>();
        var strategyFile = await dbContext.StrategyFiles.FindAsync(id);
        if (strategyFile != null)
        {
            dbContext.StrategyFiles.Remove(strategyFile);
            await dbContext.SaveChangesAsync();
        }
    }
}