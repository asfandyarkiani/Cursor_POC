using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Options;
using Polly;
using Polly.CircuitBreaker;
using Polly.Extensions.Http;
using Polly.Timeout;
using SystemLayer.Application.Interfaces;
using SystemLayer.Infrastructure.Configuration;
using SystemLayer.Infrastructure.Mapping;
using SystemLayer.Infrastructure.Services;
using SystemLayer.Infrastructure.Soap;

namespace SystemLayer.Infrastructure.DependencyInjection;

/// <summary>
/// Extension methods for configuring System Layer services
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds System Layer infrastructure services to the DI container
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configuration">Configuration</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddSystemLayerInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Configuration
        services.Configure<CafmOptions>(configuration.GetSection(CafmOptions.SectionName));
        services.Configure<KeyVaultOptions>(configuration.GetSection(KeyVaultOptions.SectionName));
        services.Configure<ResilienceOptions>(configuration.GetSection(ResilienceOptions.SectionName));
        
        // Validate configuration on startup
        services.AddSingleton<IValidateOptions<CafmOptions>, ValidateOptionsService<CafmOptions>>();
        
        // Core services
        services.AddScoped<ICafmService, CafmService>();
        services.AddScoped<ICafmClient, CafmClient>();
        services.AddScoped<CafmMappingService>();
        services.AddScoped<SoapMessageBuilder>();
        
        // HTTP Client with Polly resilience policies
        services.AddHttpClient<CafmClient>(client =>
        {
            // Base configuration will be done in CafmClient constructor
        })
        .AddPolicyHandler((serviceProvider, request) =>
        {
            var resilienceOptions = serviceProvider
                .GetRequiredService<IOptions<ResilienceOptions>>().Value;
            
            // Retry policy with exponential backoff
            var retryPolicy = HttpPolicyExtensions
                .HandleTransientHttpError()
                .Or<TaskCanceledException>()
                .WaitAndRetryAsync(
                    retryCount: resilienceOptions.Retry.MaxAttempts,
                    sleepDurationProvider: retryAttempt => TimeSpan.FromMilliseconds(
                        Math.Min(
                            resilienceOptions.Retry.BaseDelayMs * Math.Pow(2, retryAttempt - 1),
                            resilienceOptions.Retry.MaxDelayMs)),
                    onRetry: (outcome, timespan, retryCount, context) =>
                    {
                        var logger = serviceProvider.GetRequiredService<ILogger<CafmClient>>();
                        logger.LogWarning("Retry attempt {RetryCount} for CAFM request after {Delay}ms", 
                            retryCount, timespan.TotalMilliseconds);
                    });
            
            // Circuit breaker policy
            var circuitBreakerPolicy = HttpPolicyExtensions
                .HandleTransientHttpError()
                .CircuitBreakerAsync(
                    handledEventsAllowedBeforeBreaking: resilienceOptions.CircuitBreaker.FailureThreshold,
                    durationOfBreak: TimeSpan.FromSeconds(resilienceOptions.CircuitBreaker.DurationSeconds),
                    onBreak: (exception, duration) =>
                    {
                        var logger = serviceProvider.GetRequiredService<ILogger<CafmClient>>();
                        logger.LogWarning("CAFM circuit breaker opened for {Duration}s due to: {Exception}", 
                            duration.TotalSeconds, exception.Message);
                    },
                    onReset: () =>
                    {
                        var logger = serviceProvider.GetRequiredService<ILogger<CafmClient>>();
                        logger.LogInformation("CAFM circuit breaker reset");
                    });
            
            // Timeout policy
            var timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(
                TimeSpan.FromSeconds(resilienceOptions.Timeout.TimeoutSeconds));
            
            // Combine policies: Timeout -> Retry -> Circuit Breaker
            return Policy.WrapAsync(retryPolicy, circuitBreakerPolicy, timeoutPolicy);
        });
        
        return services;
    }
    
    /// <summary>
    /// Adds Azure Key Vault configuration if enabled
    /// </summary>
    /// <param name="builder">Configuration builder</param>
    /// <param name="keyVaultOptions">Key Vault options</param>
    /// <returns>Configuration builder for chaining</returns>
    public static IConfigurationBuilder AddAzureKeyVaultIfEnabled(
        this IConfigurationBuilder builder,
        KeyVaultOptions keyVaultOptions)
    {
        if (keyVaultOptions.Enabled && !string.IsNullOrEmpty(keyVaultOptions.VaultUrl))
        {
            builder.AddAzureKeyVault(
                new Uri(keyVaultOptions.VaultUrl),
                new Azure.Identity.DefaultAzureCredential());
        }
        
        return builder;
    }
}

/// <summary>
/// Generic options validation service
/// </summary>
/// <typeparam name="T">Options type to validate</typeparam>
public class ValidateOptionsService<T> : IValidateOptions<T> where T : class
{
    public ValidateOptionsResult Validate(string? name, T options)
    {
        var validationContext = new System.ComponentModel.DataAnnotations.ValidationContext(options);
        var validationResults = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
        
        var isValid = System.ComponentModel.DataAnnotations.Validator.TryValidateObject(
            options, validationContext, validationResults, true);
        
        if (isValid)
        {
            return ValidateOptionsResult.Success;
        }
        
        var errors = validationResults.Select(r => r.ErrorMessage ?? "Unknown validation error").ToArray();
        return ValidateOptionsResult.Fail(errors);
    }
}