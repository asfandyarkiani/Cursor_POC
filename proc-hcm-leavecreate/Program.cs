using Core.Middlewares;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ProcHcmLeaveCreate.ConfigModels;
using ProcHcmLeaveCreate.Constants;
using ProcHcmLeaveCreate.Services;
using ProcHcmLeaveCreate.SystemAbstractions.OracleFusionMgmt;

FunctionsHostBuilder builder = FunctionsApplication.CreateBuilder(args);

// 1. HTTP Client (FIRST)
builder.Services.AddHttpClient<CustomHTTPClient>();

// 2. Environment & Configuration Loading
string environment = Environment.GetEnvironmentVariable("ENVIRONMENT")
    ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
    ?? InfoConstants.DEFAULT_ENVIRONMENT;

builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// 3. Application Insights & Logging
builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

builder.Logging.AddConsole();
builder.Logging.AddFilter<Microsoft.Extensions.Logging.ApplicationInsights.ApplicationInsightsLoggerProvider>("", LogLevel.Information);

// 4. Configuration Binding
builder.Services.Configure<AppConfigs>(builder.Configuration.GetSection(AppConfigs.SectionName));

// 5. Domains - NOT registered (no constructor dependencies, simple POCOs)

// 6. Redis Caching (optional - not used in this project)
// builder.Services.AddRedisCacheLibrary(builder.Configuration);

// 7. System Abstractions
builder.Services.AddScoped<AbsenceMgmtSys>();

// 8. Process Abstractions (none in this project)

// 9. Services
builder.Services.AddScoped<LeaveService>();

// 10. CustomHTTPClient
builder.Services.AddScoped<CustomHTTPClient>();

// 11. Polly Policies (Singleton)
builder.Services.AddSingleton<IAsyncPolicy<HttpResponseMessage>>(sp =>
{
    IConfiguration config = sp.GetRequiredService<IConfiguration>();
    
    IAsyncPolicy<HttpResponseMessage> retryPolicy = Policy
        .Handle<HttpRequestException>()
        .OrResult<HttpResponseMessage>(r => (int)r.StatusCode >= 500)
        .WaitAndRetryAsync(
            retryCount: config.GetValue<int>("HttpClientPolicy:RetryCount"),
            sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(retryAttempt * 5)
        );
    
    IAsyncPolicy<HttpResponseMessage> timeoutPolicy = Policy
        .TimeoutAsync<HttpResponseMessage>(config.GetValue<int>("HttpClientPolicy:TimeoutSeconds"));
    
    return Policy.WrapAsync(retryPolicy, timeoutPolicy);
});

// 12. Configure Functions Web Application
builder.ConfigureFunctionsWebApplication();

// 13. Middleware Registration (STRICT ORDER - Non-Negotiable)
builder.UseMiddleware<ExecutionTimingMiddleware>();
builder.UseMiddleware<ExceptionHandlerMiddleware>();

// 14. ServiceLocator (For nested domain object resolution)
ServiceLocator.ServiceProvider = builder.Services.BuildServiceProvider();

builder.Build().Run();
