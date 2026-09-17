using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProductionPlant.Application;
using ProductionPlant.Domain;

namespace ProductionPlant.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var server =
            configuration["Database:Server"];

        var database =
            configuration["Database:Name"];

        var username =
            configuration["Database:UserName"];

        var password =
            configuration["Database:Password"];

        if (string.IsNullOrWhiteSpace(server))
        {
            throw new InvalidOperationException(
                "ProductionPlant:Database:Server is not configured.");
        }

        if (string.IsNullOrWhiteSpace(database))
        {
            throw new InvalidOperationException(
                "ProductionPlant:Database:Name is not configured.");
        }

        if (string.IsNullOrWhiteSpace(username))
        {
            throw new InvalidOperationException(
                "ProductionPlant:Database:UserName is not configured.");
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException(
                "ProductionPlant:Database:Password is not configured.");
        }

        var connectionString =
            new SqlConnectionStringBuilder
            {
                DataSource = $"{server},1433",
                InitialCatalog = database,
                UserID = username,
                Password = password,
                Encrypt = true,
                TrustServerCertificate = true,
                ConnectTimeout = 60
            }.ConnectionString;

        services.AddDbContext<RulesDbContext>(
            options =>
            {
                options.UseSqlServer(connectionString);
            });

        services.AddScoped<IRulesetRepository,
            RulesetRepository>();

        services.AddScoped<IEvaluationLogRepository,
            EvaluationLogRepository>();

        return services;
    }
}
