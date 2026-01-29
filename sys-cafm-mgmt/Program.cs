using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;
using Core.Middlewares;
using Core.SystemLayer.Middlewares;
using SysCafmMgmt.ConfigModels;
using SysCafmMgmt.Abstractions;
using SysCafmMgmt.Implementations.FSI.Services;
using SysCafmMgmt.Implementations.FSI.Handlers;
using SysCafmMgmt.Implementations.FSI.AtomicHandlers;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication(builder =>
    {
        builder.UseMiddleware<ExecutionTimingMiddleware>();
        builder.UseMiddleware<ExceptionHandlerMiddleware>();
    })
    .ConfigureAppConfiguration((context, config) =>
    {
        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
        
        config
            .SetBasePath(context.HostingEnvironment.ContentRootPath)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{env}.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables();
    })
    .ConfigureServices((context, services) =>
    {
        // Configuration
        services.Configure<AppConfigs>(context.Configuration.GetSection("AppConfigs"));
        var appConfigs = context.Configuration.GetSection("AppConfigs").Get<AppConfigs>();
        appConfigs?.Validate();

        // Logging
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
        services.AddLogging(logging =>
        {
            logging.AddConsole();
            logging.AddDebug();
        });

        // HTTP Client with Polly policies
        var retryPolicy = HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

        var timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(appConfigs?.CafmSettings?.TimeoutSeconds ?? 60));

        services.AddHttpClient<CustomHTTPClient>()
            .AddPolicyHandler(retryPolicy)
            .AddPolicyHandler(timeoutPolicy);

        // Register CustomHTTPClient
        services.AddScoped<CustomHTTPClient>();

        // Register Atomic Handlers
        services.AddScoped<LoginAtomicHandler>();
        services.AddScoped<LogoutAtomicHandler>();
        services.AddScoped<GetLocationsByDtoAtomicHandler>();
        services.AddScoped<GetInstructionSetsByDtoAtomicHandler>();
        services.AddScoped<GetBreakdownTasksByDtoAtomicHandler>();
        services.AddScoped<CreateEventAtomicHandler>();

        // Register Handlers
        services.AddScoped<CafmWorkOrderHandler>();

        // Register Services
        services.AddScoped<ICafmMgmt, CafmMgmtService>();
    })
    .Build();

host.Run();
