using Core;
using Core.Middlewares;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ProcHcmLeave.ConfigModels;
using ProcHcmLeave.Constants;
using ProcHcmLeave.Services;
using ProcHcmLeave.SystemAbstractions.HcmMgmt;
using ProcHcmLeave.SystemAbstractions.HcmMgmt.Interfaces;

FunctionsHostBuilder builder = FunctionsApplication.CreateBuilder(args);

// 1. HTTP Client
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
builder.Services.AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

builder.Logging.AddConsole();
builder.Logging.AddFilter<Microsoft.Extensions.Logging.ApplicationInsights.ApplicationInsightsLoggerProvider>("", LogLevel.Information);

// 4. Configuration Binding
builder.Services.Configure<AppConfigs>(builder.Configuration.GetSection(AppConfigs.SectionName));

// 5. Domains - NOT registered (no constructor dependencies, simple POCOs)

// 6. Redis Caching
builder.Services.AddRedisCacheLibrary(builder.Configuration);

// 7. System Abstractions
builder.Services.AddScoped<ILeaveMgmt, LeaveMgmtSys>();

// 8. Process Abstractions (none for this project)

// 9. Services
builder.Services.AddScoped<LeaveService>();

// 10. CustomHTTPClient
builder.Services.AddScoped<CustomHTTPClient>();

// 11. Configure Functions Web Application
builder.ConfigureFunctionsWebApplication();

// 12. Middleware Registration (STRICT ORDER - Non-Negotiable)
builder.UseMiddleware<ExecutionTimingMiddleware>(); // 1. FIRST - timing
builder.UseMiddleware<ExceptionHandlerMiddleware>(); // 2. SECOND - exception handling

// 13. ServiceLocator
ServiceLocator.ServiceProvider = builder.Services.BuildServiceProvider();

builder.Build().Run();
