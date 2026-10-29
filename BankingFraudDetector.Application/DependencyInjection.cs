using BankingFraudDetector.Application.Events;
using Microsoft.Extensions.DependencyInjection;
using BankingFraudDetector.Application.FraudDetection;
using BankingFraudDetector.Domain.FraudDetection;
namespace BankingFraudDetector.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        services.AddScoped<RecordAccountEvent>();
        services.AddScoped<GetAccountEvents>();
        services.AddScoped<IFraudRule, AccountTakeoverSequenceRule>();
        services.AddScoped<IFraudRule, BankChangeThenSaleRule>();
        services.AddScoped<AssessAccountRisk>();
        services.AddSingleton<TimeProvider>(TimeProvider.System);
        services.AddScoped<RunAccountAssessment>();
        services.AddScoped<GetAssessment>();
        return services;
    }
}