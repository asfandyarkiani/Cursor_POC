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
using OracleFusionHcmSystemLayer.Abstractions;
using OracleFusionHcmSystemLayer.ConfigModels;
using OracleFusionHcmSystemLayer.Helper;
using OracleFusionHcmSystemLayer.Implementations.OracleFusion.AtomicHandlers;
using OracleFusionHcmSystemLayer.Implementations.OracleFusion.Handlers;
using OracleFusionHcmSystemLayer.Implementations.OracleFusion.Services;
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

// 3. Application Insights & Logging
builder.Services.AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();
builder.Logging.AddConsole();
builder.Logging.AddFilter<ApplicationInsightsLoggerProvider>("", LogLevel.Information);

// 4. Configuration Models
builder.Services.Configure<AppConfigs>(builder.Configuration.GetSection(AppConfigs.SectionName));
builder.Services.Configure<KeyVaultConfigs>(builder.Configuration.GetSection(KeyVaultConfigs.SectionName));

// 5. Configure Functions Web Application
builder.ConfigureFunctionsWebApplication();
builder.Services.AddHttpClient<CustomHTTPClient>();

// 6. JSON Options
builder.Services.Configure<JsonSerializerOptions>(options =>
{
    options.Converters.Add(new JsonStringEnumConverter());
    options.PropertyNameCaseInsensitive = true;
});

// 7. Services (WITH interfaces)
builder.Services.AddScoped<IAbsenceMgmt, AbsenceMgmtService>();

// 8. HTTP Clients
builder.Services.AddScoped<CustomRestClient>();
builder.Services.AddScoped<CustomHTTPClient>();

// 9. Singletons/Helpers
builder.Services.AddSingleton<KeyVaultReader>();

// 10. Handlers (CONCRETE)
builder.Services.AddScoped<CreateAbsenceHandler>();

// 11. Atomic Handlers (CONCRETE)
builder.Services.AddScoped<CreateAbsenceAtomicHandler>();

// 12. Redis Cache
builder.Services.AddRedisCacheLibrary(builder.Configuration);

// 13. Polly Policy
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

// 14. Middleware (ORDER CRITICAL)
builder.UseMiddleware<ExecutionTimingMiddleware>();
builder.UseMiddleware<ExceptionHandlerMiddleware>();

// 15. Service Locator
ServiceLocator.ServiceProvider = builder.Services.BuildServiceProvider();

builder.Build().Run();
