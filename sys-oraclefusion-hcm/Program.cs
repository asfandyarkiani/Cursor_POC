using Core.DI;
using Core.Middlewares;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.ApplicationInsights;
using Polly;
using sys_oraclefusion_hcm.Abstractions;
using sys_oraclefusion_hcm.ConfigModels;
using sys_oraclefusion_hcm.Implementations.OracleFusionHCM.AtomicHandlers;
using sys_oraclefusion_hcm.Implementations.OracleFusionHCM.Handlers;
using sys_oraclefusion_hcm.Implementations.OracleFusionHCM.Services;

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

// 5. Configure Functions Web Application
builder.ConfigureFunctionsWebApplication();

// 6. HTTP Client
builder.Services.AddHttpClient<CustomHTTPClient>();

// 7. Services (WITH interfaces)
builder.Services.AddScoped<ILeaveMgmt, LeaveMgmtService>();

// 8. HTTP Clients
builder.Services.AddScoped<CustomRestClient>();
builder.Services.AddScoped<CustomHTTPClient>();

// 9. Handlers (CONCRETE)
builder.Services.AddScoped<CreateLeaveHandler>();

// 10. Atomic Handlers (CONCRETE)
builder.Services.AddScoped<CreateLeaveAtomicHandler>();

// 11. Polly Policy
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

// 12. Middleware (ORDER CRITICAL)
builder.UseMiddleware<ExecutionTimingMiddleware>();
builder.UseMiddleware<ExceptionHandlerMiddleware>();

// 13. Service Locator
ServiceLocator.ServiceProvider = builder.Services.BuildServiceProvider();

builder.Build().Run();
