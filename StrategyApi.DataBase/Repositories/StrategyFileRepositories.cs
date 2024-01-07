using Microsoft.EntityFrameworkCore;
using StrategyApi.DataBase.DbContext;
using StrategyApi.DataBase.Modeles;

namespace StrategyApi.DataBase.Repositories;

public class StrategyFileRepository : IStrategyFileRepository
{
    private readonly StrategyContext _context;

    public StrategyFileRepository(StrategyContext context)
    {
        _context = context;
    }

    public async Task<List<StrategyFile>> GetAllAsync()
    {
        return await _context.StrategyFiles.ToListAsync();
    }

    public async Task<List<StrategyFile>> GetAllWithoutDataAsync()
    {
        return await _context.StrategyFiles.Select(x => new StrategyFile
        {
            Name = x.Name,
            Id = x.Id,
            Version = x.Version
        }).ToListAsync();
    }

    public async Task<StrategyFile> GetByIdAsync(int id)
    {
        return await _context.StrategyFiles.FindAsync(id);
    }

    public async Task AddAsync(StrategyFile strategyFile)
    {
        _context.StrategyFiles.Add(strategyFile);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(StrategyFile strategyFile)
    {
        _context.StrategyFiles.Update(strategyFile);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var strategyFile = await _context.StrategyFiles.FindAsync(id);
        if (strategyFile != null)
        {
            _context.StrategyFiles.Remove(strategyFile);
            await _context.SaveChangesAsync();
        }
    }
}