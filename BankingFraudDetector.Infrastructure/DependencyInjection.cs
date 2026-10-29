using BankingFraudDetector.Application.Abstractions;
using BankingFraudDetector.Infrastructure.Persistence;
using BankingFraudDetector.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

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
}
