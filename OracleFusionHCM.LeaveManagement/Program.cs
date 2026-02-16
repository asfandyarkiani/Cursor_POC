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
using OracleFusionHCM.LeaveManagement.Abstractions;
using OracleFusionHCM.LeaveManagement.ConfigModels;
using OracleFusionHCM.LeaveManagement.Implementations.OracleFusion.AtomicHandlers;
using OracleFusionHCM.LeaveManagement.Implementations.OracleFusion.Handlers;
using OracleFusionHCM.LeaveManagement.Implementations.OracleFusion.Services;
using Polly;

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

// 4. Configuration Binding
builder.Services.Configure<AppConfigs>(builder.Configuration.GetSection(AppConfigs.SectionName));

// 5. Configure Functions Web Application
builder.ConfigureFunctionsWebApplication();

// 6. HTTP Client
builder.Services.AddHttpClient<CustomHTTPClient>();

// 7. Services (WITH interfaces)
builder.Services.AddScoped<ILeaveMgmt, LeaveMgmtService>();

// 8. Handlers (CONCRETE only)
builder.Services.AddScoped<CreateLeaveHandler>();

// 9. Atomic Handlers (CONCRETE only)
builder.Services.AddScoped<CreateLeaveAtomicHandler>();

// 10. CustomRestClient (Framework)
builder.Services.AddScoped<CustomRestClient>();

// 11. Polly Policies (Singleton)
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
        .TimeoutAsync<HttpResponseMessage>(
            config.GetValue<int>("HttpClientPolicy:TimeoutSeconds", 60)
        );
    
    return Policy.WrapAsync(retryPolicy, timeoutPolicy);
});

// 12. Middleware Registration (STRICT ORDER - NON-NEGOTIABLE)
builder.UseMiddleware<ExecutionTimingMiddleware>();  // 1. FIRST - timing
builder.UseMiddleware<ExceptionHandlerMiddleware>(); // 2. SECOND - exception handling
// NO CustomAuthenticationMiddleware - Basic Auth per request (credentials in AppConfigs)

// 13. Service Locator (LAST)
ServiceLocator.ServiceProvider = builder.Services.BuildServiceProvider();

builder.Build().Run();
