using Core.Middlewares;
using Core.SystemLayer.Middlewares;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;
using SysCafmMgmt.Abstractions;
using SysCafmMgmt.ConfigModels;
using SysCafmMgmt.Implementations.FSI.AtomicHandlers;
using SysCafmMgmt.Implementations.FSI.Handlers;
using SysCafmMgmt.Implementations.FSI.Services;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults(worker =>
    {
        worker.UseMiddleware<ExceptionHandlerMiddleware>();
        worker.UseMiddleware<ExecutionTimingMiddleware>();
    })
    .ConfigureAppConfiguration((context, config) =>
    {
        var env = Environment.GetEnvironmentVariable("AZURE_FUNCTIONS_ENVIRONMENT") ?? "Development";
        config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
              .AddJsonFile($"appsettings.{env}.json", optional: true, reloadOnChange: true)
              .AddEnvironmentVariables();
    })
    .ConfigureServices((context, services) =>
    {
        // Configuration
        services.Configure<AppConfigs>(context.Configuration.GetSection("AppConfigs"));

        // Polly retry policy
        var retryPolicy = HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

        // Register CustomHTTPClient with typed HttpClient
        services.AddHttpClient<CustomHTTPClient>()
            .AddPolicyHandler(retryPolicy);

        // Register IAsyncPolicy for CustomHTTPClient constructor
        services.AddSingleton<IAsyncPolicy<HttpResponseMessage>>(_ => retryPolicy);

        // Register Atomic Handlers
        services.AddScoped<AuthenticateAtomicHandler>();
        services.AddScoped<LogoutAtomicHandler>();
        services.AddScoped<GetLocationsByDtoAtomicHandler>();
        services.AddScoped<GetInstructionSetsByDtoAtomicHandler>();
        services.AddScoped<CreateBreakdownTaskAtomicHandler>();
        services.AddScoped<GetBreakdownTasksByDtoAtomicHandler>();

        // Register Handlers
        services.AddScoped<CreateWorkOrderHandler>();

        // Register Services
        services.AddScoped<IWorkOrderMgmt, WorkOrderService>();

        // Application Insights
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
    })
    .ConfigureLogging(logging =>
    {
        logging.AddConsole();
        logging.SetMinimumLevel(LogLevel.Information);
    })
    .Build();

host.Run();
