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
using SysNetworkMgmt.Abstractions;
using SysNetworkMgmt.ConfigModels;
using SysNetworkMgmt.Implementations.Network.Handlers;
using SysNetworkMgmt.Implementations.Network.Services;

var builder = FunctionsApplication.CreateBuilder(args);

// 0. Configuration - Load appsettings based on environment
var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{environment.ToLower()}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// 1. Logging - FIRST
builder.Services.AddApplicationInsightsTelemetryWorkerService();
builder.Services.ConfigureFunctionsApplicationInsights();
builder.Logging.AddConsole();
builder.Logging.AddFilter<ApplicationInsightsLoggerProvider>("", LogLevel.Trace);

// 2. Configuration Binding
builder.Services.Configure<AppConfigs>(builder.Configuration.GetSection(AppConfigs.SectionName));

// 3. HTTP Clients (from Core Framework)
builder.Services.AddSingleton<CustomHTTPClient>();
builder.Services.AddSingleton<CustomRestClient>();

// 4. Services (Interface → Implementation)
builder.Services.AddScoped<INetworkMgmt, NetworkMgmtService>();

// 5. Handlers (Concrete)
builder.Services.AddScoped<NetworkTestHandler>();

// 6. Middleware Registration (ORDER CRITICAL!)
// ExecutionTimingMiddleware → ExceptionHandlerMiddleware
builder.UseMiddleware<ExecutionTimingMiddleware>();
builder.UseMiddleware<ExceptionHandlerMiddleware>();

// 7. Service Locator (LAST!)
ServiceLocator.ServiceProvider = builder.Services.BuildServiceProvider();

var app = builder.Build();
app.Run();
