using Microsoft.EntityFrameworkCore;
using StrategyApi.DataBase.Modeles;

namespace StrategyApi.DataBase.DbContext;

public class StrategyContext : Microsoft.EntityFrameworkCore.DbContext
{
    public StrategyContext(DbContextOptions<StrategyContext> options)
        : base(options)
    {
    }

    public DbSet<StrategyFile> StrategyFiles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}