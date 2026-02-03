using Core.DI;
using Core.Middlewares;
using Core.SystemLayer.Middlewares;
using AGI.SysD365DriverLateLoginMgmt.Abstractions;
using AGI.SysD365DriverLateLoginMgmt.ConfigModels;
using AGI.SysD365DriverLateLoginMgmt.Helper;
using AGI.SysD365DriverLateLoginMgmt.Implementations.D365.AtomicHandlers;
using AGI.SysD365DriverLateLoginMgmt.Implementations.D365.Handlers;
using AGI.SysD365DriverLateLoginMgmt.Implementations.D365.Services;
using AGI.SysD365DriverLateLoginMgmt.Middleware;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

// Create Functions Application Builder
FunctionsApplicationBuilder builder = FunctionsApplication.CreateBuilder(args);

// 0. Logging (FIRST!)
builder.Services.AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();
builder.Logging.AddConsole();

// 1. Configuration
builder.Services.Configure<AppConfigs>(builder.Configuration);

// 2. HTTP Clients (Framework - CustomRestClient auto-registered)
builder.Services.AddSingleton<CustomRestClient>();

// 3. Helpers
builder.Services.AddSingleton<KeyVaultReader>();
builder.Services.AddScoped<RequestContext>();

// 4. Services (Interface → Implementation)
builder.Services.AddScoped<IDriverLateLoginMgmt, DriverLateLoginMgmtService>();

// 5. Handlers (Concrete)
builder.Services.AddScoped<SubmitDriverLateLoginHandler>();

// 6. Atomic Handlers (Concrete)
builder.Services.AddScoped<SubmitLateLoginAtomicHandler>();

// 7. Auth Atomic Handlers (for middleware)
builder.Services.AddScoped<AuthenticateD365AtomicHandler>();
builder.Services.AddScoped<LogoutD365AtomicHandler>();

// 8. Middleware (ORDER CRITICAL!)
builder.UseMiddleware<ExecutionTimingMiddleware>();
builder.UseMiddleware<ExceptionHandlerMiddleware>();
builder.UseMiddleware<D365AuthenticationMiddleware>();

// 9. Service Locator (LAST!)
ServiceLocator.ServiceProvider = builder.Services.BuildServiceProvider();

// Build and run
builder.Build().Run();
