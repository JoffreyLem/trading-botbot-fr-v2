using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
            options.UseNpgsql(connectionString), ServiceLifetime.Singleton);

        services.AddSingleton<IStrategyFileRepository, StrategyFileRepository>();


        return services;
    }
}