using Core.DI;
using Core.Middlewares;
using Core.SystemLayer.Middlewares;
using Microsoft.Azure.Functions.Worker;
using AGI.SysD365DriverLateLoginMgmt.Abstractions;
using AGI.SysD365DriverLateLoginMgmt.ConfigModels;
using AGI.SysD365DriverLateLoginMgmt.Helper;
using AGI.SysD365DriverLateLoginMgmt.Implementations.D365.AtomicHandlers;
using AGI.SysD365DriverLateLoginMgmt.Implementations.D365.Handlers;
using AGI.SysD365DriverLateLoginMgmt.Implementations.D365.Services;
using AGI.SysD365DriverLateLoginMgmt.Middleware;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

// Create Host Builder
IHostBuilder builder = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureFunctionsWebApplication();

builder.ConfigureServices((context, services) =>
{
    // 0. Logging (FIRST!)
    services.AddApplicationInsightsTelemetryWorkerService();
    services.AddLogging(loggingBuilder =>
    {
        loggingBuilder.AddConsole();
    });

    // 1. Configuration
    services.Configure<AppConfigs>(context.Configuration);

    // 2. HTTP Clients (Framework)
    services.AddSingleton<CustomHTTPClient>();
    services.AddSingleton<CustomRestClient>();

    // 3. Helpers
    services.AddSingleton<KeyVaultReader>();
    services.AddScoped<RequestContext>();

    // 4. Services (Interface → Implementation)
    services.AddScoped<IDriverLateLoginMgmt, DriverLateLoginMgmtService>();

    // 5. Handlers (Concrete)
    services.AddScoped<SubmitDriverLateLoginHandler>();

    // 6. Atomic Handlers (Concrete)
    services.AddScoped<SubmitLateLoginAtomicHandler>();

    // 7. Auth Atomic Handlers (for middleware)
    services.AddScoped<AuthenticateD365AtomicHandler>();
    services.AddScoped<LogoutD365AtomicHandler>();
});

builder.ConfigureFunctionsWorkerDefaults((context, workerApp) =>
{
    // 8. Middleware (ORDER CRITICAL!)
    workerApp.UseMiddleware<ExecutionTimingMiddleware>();
    workerApp.UseMiddleware<ExceptionHandlerMiddleware>();
    workerApp.UseMiddleware<D365AuthenticationMiddleware>();
});

// Build host
IHost host = builder.Build();

// 9. Service Locator (LAST!)
ServiceLocator.ServiceProvider = host.Services;

// Run
host.Run();
