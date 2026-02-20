using Cache.Extensions;
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
using OracleFusionHcmMgmt.Abstractions;
using OracleFusionHcmMgmt.ConfigModels;
using OracleFusionHcmMgmt.Implementations.OracleFusion.AtomicHandlers;
using OracleFusionHcmMgmt.Implementations.OracleFusion.Handlers;
using OracleFusionHcmMgmt.Implementations.OracleFusion.Services;
using Polly;
using System.Text.Json;
using System.Text.Json.Serialization;

FunctionsApplicationBuilder builder = FunctionsApplication.CreateBuilder(args);

// Environment Detection
string environment = Environment.GetEnvironmentVariable("ENVIRONMENT") 
    ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") 
    ?? OracleFusionHcmMgmt.Constants.InfoConstants.DEFAULT_ENVIRONMENT;

// Configuration Loading
builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// Application Insights & Logging (FIRST)
builder.Services.AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

builder.Logging.AddConsole();
builder.Logging.AddFilter<ApplicationInsightsLoggerProvider>("", LogLevel.Information);

// Configuration Models
builder.Services.Configure<AppConfigs>(builder.Configuration.GetSection(AppConfigs.SectionName));
builder.Services.Configure<KeyVaultConfigs>(builder.Configuration.GetSection(KeyVaultConfigs.SectionName));

// Configure Functions Web Application
builder.ConfigureFunctionsWebApplication();

// HTTP Client
builder.Services.AddHttpClient<CustomHTTPClient>();

// JSON Options
builder.Services.Configure<JsonSerializerOptions>(options =>
{
    options.Converters.Add(new JsonStringEnumConverter());
    options.PropertyNameCaseInsensitive = true;
});

// Services (WITH interfaces)
builder.Services.AddScoped<ILeaveMgmt, LeaveMgmtService>();

// HTTP Clients (Framework-managed)
builder.Services.AddScoped<CustomRestClient>();
builder.Services.AddScoped<CustomHTTPClient>();

// Handlers (CONCRETE)
builder.Services.AddScoped<CreateLeaveHandler>();

// Atomic Handlers (CONCRETE)
builder.Services.AddScoped<CreateLeaveAtomicHandler>();

// Redis Cache Library (if needed)
builder.Services.AddRedisCacheLibrary(builder.Configuration);

// Polly Policy (Retry + Timeout)
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

// Middleware (ORDER CRITICAL)
builder.UseMiddleware<ExecutionTimingMiddleware>();
builder.UseMiddleware<ExceptionHandlerMiddleware>();

// Service Locator (LAST)
ServiceLocator.ServiceProvider = builder.Services.BuildServiceProvider();

builder.Build().Run();
