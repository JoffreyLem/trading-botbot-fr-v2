using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StrategyApi.DataBase.DbContext;
using StrategyApi.DataBase.Repositories;

namespace StrategyApi.DataBase;

public static class ServiceDatabaseExtension
{
    public static IServiceCollection AddStrategyDbContext(this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<StrategyContext>(options =>
            options
                .UseMySql(connectionString, new MariaDbServerVersion(ServerVersion.AutoDetect(connectionString)),
                    builder =>
                        builder.EnableRetryOnFailure(
                            2,
                            TimeSpan.FromSeconds(10),
                            null))
                .LogTo(Console.WriteLine, LogLevel.Information)
                .EnableDetailedErrors()
                .EnableSensitiveDataLogging(), ServiceLifetime.Singleton);

        services.AddSingleton<IStrategyFileRepository, StrategyFileRepository>();


        return services;
    }
}