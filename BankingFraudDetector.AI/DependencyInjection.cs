using BankingFraudDetector.Application.Investigations;
using Microsoft.Extensions.DependencyInjection;
using OllamaSharp;

namespace BankingFraudDetector.AI;

public static class DependencyInjection
{
    public static IServiceCollection AddAI(
     this IServiceCollection services,
     OllamaOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (!Uri.TryCreate(
                options.Endpoint,
                UriKind.Absolute,
                out var endpoint) ||
            (endpoint.Scheme != Uri.UriSchemeHttp &&
             endpoint.Scheme != Uri.UriSchemeHttps))
        {
            throw new ArgumentException(
                "A valid HTTP or HTTPS Ollama endpoint is required.");
        }

        if (string.IsNullOrWhiteSpace(options.Model))
        {
            throw new ArgumentException("An Ollama model is required.");
        }

        if (options.TimeoutSeconds <= 0)
        {
            throw new ArgumentException("The timeout must be positive.");
        }

        services.AddSingleton(options);

        services.AddSingleton<HttpClient>(_ => new HttpClient
        {
            BaseAddress = endpoint,
            Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds)
        });

        services.AddSingleton<OllamaApiClient>(provider =>
            new OllamaApiClient(
                provider.GetRequiredService<HttpClient>(),
                options.Model));

        services.AddScoped<
            IAssessmentInvestigator,
            AgentFrameworkAssessmentInvestigator>();

        return services;
    }
}