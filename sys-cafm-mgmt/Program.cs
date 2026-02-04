using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using AGI.SystemLayer.CAFM.ConfigModels;
using AGI.SystemLayer.CAFM.Abstractions;
using AGI.SystemLayer.CAFM.Implementations.FSI.Services;
using AGI.SystemLayer.CAFM.Implementations.FSI.Handlers;
using AGI.SystemLayer.CAFM.Implementations.FSI.AtomicHandlers;
using AGI.SystemLayer.CAFM.Middlewares;
using Core.Middlewares;
using Core.SystemLayer.Middlewares;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication(builder =>
    {
        builder.UseMiddleware<ExceptionHandlerMiddleware>();
        builder.UseMiddleware<ExecutionTimingMiddleware>();
        builder.UseMiddleware<CAFMAuthenticationMiddleware>();
    })
    .ConfigureAppConfiguration((context, config) =>
    {
        var env = Environment.GetEnvironmentVariable("Environment") ?? "local";
        config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
        config.AddJsonFile($"appsettings.{env}.json", optional: true, reloadOnChange: true);
        config.AddEnvironmentVariables();
    })
    .ConfigureServices((context, services) =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        // Configuration
        services.Configure<AppConfigs>(context.Configuration.GetSection("AppConfigs"));

        // HTTP Clients
        services.AddHttpClient<CustomHTTPClient>();
        services.AddScoped<CustomHTTPClient>();

        // Atomic Handlers
        services.AddScoped<LoginAtomicHandler>();
        services.AddScoped<LogoutAtomicHandler>();
        services.AddScoped<GetLocationsByDtoAtomicHandler>();
        services.AddScoped<GetInstructionSetsByDtoAtomicHandler>();
        services.AddScoped<GetBreakdownTasksByDtoAtomicHandler>();
        services.AddScoped<CreateBreakdownTaskAtomicHandler>();
        services.AddScoped<CreateEventAtomicHandler>();

        // Handlers
        services.AddScoped<CreateWorkOrderHandler>();

        // Services
        services.AddScoped<ICAFMMgmt, CAFMMgmtService>();
    })
    .Build();

host.Run();
