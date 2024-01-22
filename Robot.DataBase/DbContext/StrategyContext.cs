using Microsoft.EntityFrameworkCore;
using Robot.DataBase.Modeles;

namespace Robot.DataBase.DbContext;

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