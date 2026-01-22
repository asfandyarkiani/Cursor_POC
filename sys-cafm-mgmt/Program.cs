using Core.Middlewares;
using Core.SystemLayer.Middlewares;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Polly;
using Polly.Extensions.Http;
using SysCafmMgmt.Abstractions;
using SysCafmMgmt.ConfigModels;
using SysCafmMgmt.Implementations.Fsi.AtomicHandlers;
using SysCafmMgmt.Implementations.Fsi.Handlers;
using SysCafmMgmt.Implementations.Fsi.Services;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults(worker =>
    {
        worker.UseMiddleware<ExceptionHandlerMiddleware>();
    })
    .ConfigureAppConfiguration((context, config) =>
    {
        var env = Environment.GetEnvironmentVariable("Environment") ?? "dev";
        config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
              .AddJsonFile($"appsettings.{env}.json", optional: true, reloadOnChange: true)
              .AddEnvironmentVariables();
    })
    .ConfigureServices((context, services) =>
    {
        // Configuration
        services.Configure<AppConfigs>(context.Configuration.GetSection("AppConfigs"));

        // Define retry policy for HTTP client
        var retryPolicy = HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

        // Register HttpClient with Polly policy
        services.AddHttpClient<CustomHTTPClient>()
            .AddPolicyHandler(retryPolicy);

        // Register Polly policy as IAsyncPolicy for direct injection
        services.AddSingleton<IAsyncPolicy<HttpResponseMessage>>(retryPolicy);

        // Register Atomic Handlers
        services.AddScoped<AuthenticateAtomicHandler>();
        services.AddScoped<LogoutAtomicHandler>();
        services.AddScoped<GetLocationsByDtoAtomicHandler>();
        services.AddScoped<GetInstructionSetsByDtoAtomicHandler>();
        services.AddScoped<CreateBreakdownTaskAtomicHandler>();

        // Register Handlers
        services.AddScoped<CreateWorkOrderHandler>();

        // Register Services
        services.AddScoped<IWorkOrderMgmt, WorkOrderService>();

        // Application Insights
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
    })
    .Build();

host.Run();
