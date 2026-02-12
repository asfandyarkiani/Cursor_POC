using Cache.Extensions;
using Core.DI;
using Core.Middlewares;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.ApplicationInsights;
using OracleFusionHCMSystemLayer.Abstractions;
using OracleFusionHCMSystemLayer.ConfigModels;
using OracleFusionHCMSystemLayer.Helper;
using OracleFusionHCMSystemLayer.Implementations.OracleFusionHCM.AtomicHandlers;
using OracleFusionHCMSystemLayer.Implementations.OracleFusionHCM.Handlers;
using OracleFusionHCMSystemLayer.Implementations.OracleFusionHCM.Services;
using Polly;
using System.Text.Json;
using System.Text.Json.Serialization;

FunctionsApplicationBuilder builder = FunctionsApplication.CreateBuilder(args);

// 1. Environment Detection
string environment = Environment.GetEnvironmentVariable("ENVIRONMENT") 
    ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") 
    ?? "dev";

// 2. Configuration Loading
builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// 3. Application Insights & Logging (FIRST service registration)
builder.Services.AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

builder.Logging.AddConsole();
builder.Logging.AddFilter<ApplicationInsightsLoggerProvider>("", LogLevel.Information);

// 4. Configuration Models
builder.Services.Configure<AppConfigs>(builder.Configuration.GetSection(AppConfigs.SectionName));
builder.Services.Configure<KeyVaultConfigs>(builder.Configuration.GetSection(KeyVaultConfigs.SectionName));

// 5. Configure Functions Web Application
builder.ConfigureFunctionsWebApplication();

// 6. HTTP Client
builder.Services.AddHttpClient<CustomHTTPClient>();

// 7. JSON Serialization Options
builder.Services.Configure<JsonSerializerOptions>(options =>
{
    options.Converters.Add(new JsonStringEnumConverter());
    options.PropertyNameCaseInsensitive = true;
});

// 8. Services (WITH interfaces)
builder.Services.AddScoped<IAbsenceMgmt, AbsenceMgmtService>();

// 9. HTTP Clients (Framework)
builder.Services.AddScoped<CustomRestClient>();
builder.Services.AddScoped<CustomHTTPClient>();

// 10. Helpers (Singletons)
builder.Services.AddSingleton<KeyVaultReader>();

// 11. Handlers (CONCRETE)
builder.Services.AddScoped<CreateAbsenceHandler>();

// 12. Atomic Handlers (CONCRETE)
builder.Services.AddScoped<CreateAbsenceAtomicHandler>();

// 13. Redis Cache Library
builder.Services.AddRedisCacheLibrary(builder.Configuration);

// 14. Polly Policies (Retry + Timeout)
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

// 15. Middleware (STRICT ORDER: ExecutionTiming → Exception)
builder.UseMiddleware<ExecutionTimingMiddleware>();
builder.UseMiddleware<ExceptionHandlerMiddleware>();

// 16. Service Locator (LAST before Build)
ServiceLocator.ServiceProvider = builder.Services.BuildServiceProvider();

builder.Build().Run();
