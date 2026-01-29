using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;
using SystemLayer.Application.Configuration;
using SystemLayer.Application.Interfaces;
using SystemLayer.Infrastructure.Clients;
using SystemLayer.Infrastructure.Services;
using SystemLayer.Infrastructure.Xml;

namespace SystemLayer.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Configuration
        services.Configure<CafmConfiguration>(configuration.GetSection(CafmConfiguration.SectionName));
        services.AddSingleton<IValidateOptions<CafmConfiguration>, CafmConfigurationValidator>();

        // HTTP Client with resilience policies
        services.AddHttpClient<ICafmClient, CafmClient>((serviceProvider, client) =>
        {
            var config = serviceProvider.GetRequiredService<IOptions<CafmConfiguration>>().Value;
            client.Timeout = TimeSpan.FromSeconds(config.TimeoutSeconds);
            client.BaseAddress = new Uri(config.BaseUrl);
        })
        .AddPolicyHandler(GetRetryPolicy)
        .AddPolicyHandler(GetCircuitBreakerPolicy);

        // Services
        services.AddScoped<ICafmService, CafmService>();
        services.AddScoped<ICafmAuthenticationService, CafmAuthenticationService>();
        services.AddScoped<ICafmXmlBuilder, CafmXmlBuilder>();
        services.AddScoped<ICafmXmlParser, CafmXmlParser>();

        return services;
    }

    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(IServiceProvider serviceProvider)
    {
        var config = serviceProvider.GetRequiredService<IOptions<CafmConfiguration>>().Value;
        
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(
                retryCount: config.Retry.MaxAttempts,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(
                    Math.Min(
                        config.Retry.BaseDelaySeconds * Math.Pow(2, retryAttempt - 1),
                        config.Retry.MaxDelaySeconds)),
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                    var logger = serviceProvider.GetService<Microsoft.Extensions.Logging.ILogger<CafmClient>>();
                    logger?.LogWarning(
                        "Retry {RetryCount} for CAFM request after {Delay}ms. Exception: {Exception}",
                        retryCount, timespan.TotalMilliseconds, outcome.Exception?.Message);
                });
    }

    private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy(IServiceProvider serviceProvider)
    {
        var config = serviceProvider.GetRequiredService<IOptions<CafmConfiguration>>().Value;
        
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: config.CircuitBreaker.FailureThreshold,
                durationOfBreak: TimeSpan.FromSeconds(config.CircuitBreaker.DurationOfBreakSeconds),
                onBreak: (exception, duration) =>
                {
                    var logger = serviceProvider.GetService<Microsoft.Extensions.Logging.ILogger<CafmClient>>();
                    logger?.LogWarning(
                        "CAFM Circuit breaker opened for {Duration}ms. Exception: {Exception}",
                        duration.TotalMilliseconds, exception.Exception?.Message);
                },
                onReset: () =>
                {
                    var logger = serviceProvider.GetService<Microsoft.Extensions.Logging.ILogger<CafmClient>>();
                    logger?.LogInformation("CAFM Circuit breaker reset");
                });
    }
}

public class CafmConfigurationValidator : IValidateOptions<CafmConfiguration>
{
    public ValidateOptionsResult Validate(string? name, CafmConfiguration options)
    {
        var failures = new List<string>();

        if (string.IsNullOrEmpty(options.BaseUrl))
            failures.Add("CAFM BaseUrl is required");

        if (string.IsNullOrEmpty(options.Username))
            failures.Add("CAFM Username is required");

        if (string.IsNullOrEmpty(options.Password))
            failures.Add("CAFM Password is required");

        if (string.IsNullOrEmpty(options.Database))
            failures.Add("CAFM Database is required");

        if (options.TimeoutSeconds <= 0)
            failures.Add("CAFM TimeoutSeconds must be greater than 0");

        if (options.Retry.MaxAttempts <= 0)
            failures.Add("CAFM Retry MaxAttempts must be greater than 0");

        if (options.Retry.BaseDelaySeconds <= 0)
            failures.Add("CAFM Retry BaseDelaySeconds must be greater than 0");

        if (options.CircuitBreaker.FailureThreshold <= 0)
            failures.Add("CAFM CircuitBreaker FailureThreshold must be greater than 0");

        if (!Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out _))
            failures.Add("CAFM BaseUrl must be a valid absolute URI");

        return failures.Any()
            ? ValidateOptionsResult.Fail(failures)
            : ValidateOptionsResult.Success;
    }
}