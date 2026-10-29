using BankingFraudDetector.Application.Abstractions;
using BankingFraudDetector.Infrastructure.Persistence;
using BankingFraudDetector.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using BankingFraudDetector.Application.Graphs;
using BankingFraudDetector.Infrastructure.Graphs;
using Neo4j.Driver;

namespace BankingFraudDetector.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<FraudDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<IAccountEventRepository, EfAccountEventRepository>();
        services.AddScoped<IAssessmentRepository, EfAssessmentRepository>();
        return services;
    }
    public static IServiceCollection AddGraphInfrastructure(
    this IServiceCollection services,
    string uri,
    string username,
    string password)
    {
        services.AddSingleton<IDriver>(_ =>
            GraphDatabase.Driver(
                uri,
                AuthTokens.Basic(username, password)));

        services.AddScoped<IAccountGraph, Neo4jAccountGraph>();

        return services;
    }
}
