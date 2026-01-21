using Core.DI;
using Core.SystemLayer.Middlewares;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.ApplicationInsights;
using SysNetworkMgmt.Abstractions;
using SysNetworkMgmt.ConfigModels;
using SysNetworkMgmt.Implementations.Network.Handlers;
using SysNetworkMgmt.Implementations.Network.Services;

var builder = FunctionsApplication.CreateBuilder(args);

// 0. Logging Configuration (FIRST!)
builder.Services.AddApplicationInsightsTelemetryWorkerService();
builder.Services.ConfigureFunctionsApplicationInsights();
builder.Logging.AddConsole();
builder.Logging.AddFilter<ApplicationInsightsLoggerProvider>("", LogLevel.Trace);

// 1. Configuration Binding
builder.Services.Configure<AppConfigs>(builder.Configuration.GetSection(AppConfigs.SectionName));

// 2. Framework Libraries
// Note: Redis cache can be enabled when needed by uncommenting:
// builder.Services.AddRedisCacheLibrary(builder.Configuration);

// 3. HTTP Clients (auto-registered by Core Framework when needed)
// Note: CustomRestClient, CustomHTTPClient are available from Core Framework

// 4. Helpers (add as needed)
// builder.Services.AddSingleton<KeyVaultReader>();

// 5. Services (Interface → Implementation)
builder.Services.AddScoped<INetworkMgmt, NetworkMgmtService>();

// 6. Handlers (Concrete)
builder.Services.AddScoped<NetworkTestHandler>();

// 7. Atomic Handlers (add when downstream API calls are needed)
// This Network Test operation doesn't require atomic handlers
// as it doesn't make external API calls

// 8. Middleware (ORDER CRITICAL!)
builder.UseMiddleware<ExecutionTimingMiddleware>();
builder.UseMiddleware<ExceptionHandlerMiddleware>();
// Note: CustomAuthenticationMiddleware can be added when authentication is needed

// 9. Service Locator (LAST!)
ServiceLocator.ServiceProvider = builder.Services.BuildServiceProvider();

builder.Build().Run();
