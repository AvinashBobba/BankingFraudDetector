using BankingFraudDetector.Application.Events;
using Microsoft.Extensions.DependencyInjection;

namespace BankingFraudDetector.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        services.AddScoped<RecordAccountEvent>();

        return services;
    }
}