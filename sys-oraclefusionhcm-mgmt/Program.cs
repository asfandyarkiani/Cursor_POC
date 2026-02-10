using Core.DI;
using Core.Middlewares;
using Core.SystemLayer.Middlewares;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.ApplicationInsights;
using OracleFusionHCMSystem.Abstractions;
using OracleFusionHCMSystem.ConfigModels;
using OracleFusionHCMSystem.Helper;
using OracleFusionHCMSystem.Implementations.OracleFusionHCM.AtomicHandlers;
using OracleFusionHCMSystem.Implementations.OracleFusionHCM.Handlers;
using OracleFusionHCMSystem.Implementations.OracleFusionHCM.Services;
using Polly;
using System.Text.Json;
using System.Text.Json.Serialization;

FunctionsApplicationBuilder builder = FunctionsApplication.CreateBuilder(args);

// 1. Environment & Configuration Loading
string environment = Environment.GetEnvironmentVariable("ENVIRONMENT") 
    ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") 
    ?? "dev";

builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// 2. Application Insights & Logging
builder.Services.AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

builder.Logging.AddConsole();
builder.Logging.AddFilter<ApplicationInsightsLoggerProvider>("", LogLevel.Information);

// 3. Configuration Binding
builder.Services.Configure<AppConfigs>(builder.Configuration.GetSection(AppConfigs.SectionName));
builder.Services.Configure<KeyVaultConfigs>(builder.Configuration.GetSection(KeyVaultConfigs.SectionName));

// 4. HTTP Client
builder.Services.AddHttpClient<CustomHTTPClient>();

// 6. JSON Options
builder.Services.Configure<JsonSerializerOptions>(options =>
{
    options.Converters.Add(new JsonStringEnumConverter());
    options.PropertyNameCaseInsensitive = true;
});

// 7. Services (WITH interfaces)
builder.Services.AddScoped<ILeaveMgmt, LeaveMgmtService>();

// 8. HTTP Clients
builder.Services.AddScoped<CustomRestClient>();
builder.Services.AddScoped<CustomHTTPClient>();

// 9. Singletons/Helpers
builder.Services.AddSingleton<KeyVaultReader>();

// 10. Handlers (CONCRETE)
builder.Services.AddScoped<CreateLeaveHandler>();

// 11. Atomic Handlers (CONCRETE)
builder.Services.AddScoped<CreateLeaveAtomicHandler>();

// 12. Redis Cache (commented out - Cache framework not used due to private NuGet dependency)
// builder.Services.AddRedisCacheLibrary(builder.Configuration);

// 13. Polly Policies
builder.Services.AddSingleton<IAsyncPolicy<HttpResponseMessage>>(sp =>
{
    IConfiguration config = sp.GetRequiredService<IConfiguration>();
    
    IAsyncPolicy<HttpResponseMessage> retryPolicy = Policy
        .Handle<HttpRequestException>()
        .OrResult<HttpResponseMessage>(r => (int)r.StatusCode >= 500)
        .WaitAndRetryAsync(
            retryCount: config.GetValue<int>("HttpClientPolicy:RetryCount", 0),
            sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(retryAttempt * 5)
        );

    IAsyncPolicy<HttpResponseMessage> timeoutPolicy = Policy
        .TimeoutAsync<HttpResponseMessage>(config.GetValue<int>("HttpClientPolicy:TimeoutSeconds", 60));

    return Policy.WrapAsync(retryPolicy, timeoutPolicy);
});

// 14. Configure Middleware (Azure Functions Worker v2.0.0 pattern)
builder.ConfigureFunctionsWorkerDefaults(app =>
{
    app.UseMiddleware<ExecutionTimingMiddleware>();
    app.UseMiddleware<ExceptionHandlerMiddleware>();
});

// 15. Service Locator
ServiceLocator.ServiceProvider = builder.Services.BuildServiceProvider();

builder.Build().Run();
