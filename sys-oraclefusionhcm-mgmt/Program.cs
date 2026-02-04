using Core.Middlewares;
using Core.SystemLayer.Middlewares;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;
using sys_oraclefusionhcm_mgmt.Abstractions;
using sys_oraclefusionhcm_mgmt.ConfigModels;
using sys_oraclefusionhcm_mgmt.Implementations.OracleFusionHCM.AtomicHandlers;
using sys_oraclefusionhcm_mgmt.Implementations.OracleFusionHCM.Handlers;
using sys_oraclefusionhcm_mgmt.Implementations.OracleFusionHCM.Services;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication(builder =>
    {
        // Register middleware in EXACT order per System-Layer-Rules.mdc
        builder.UseMiddleware<ExecutionTimingMiddleware>();  // FIRST
        builder.UseMiddleware<ExceptionHandlerMiddleware>(); // SECOND
        // No CustomAuthenticationMiddleware needed - Basic Auth per-request
    })
    .ConfigureAppConfiguration((context, config) =>
    {
        // Load configuration from appsettings files
        config.SetBasePath(Directory.GetCurrentDirectory())
              .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
              .AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true)
              .AddEnvironmentVariables();
    })
    .ConfigureServices((context, services) =>
    {
        // Register Application Insights
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
        
        // Register configuration
        services.Configure<AppConfigs>(context.Configuration);
        
        // Register CustomHTTPClient with Polly retry policies
        services.AddHttpClient<CustomHTTPClient>()
            .AddPolicyHandler(GetRetryPolicy())
            .AddPolicyHandler(GetCircuitBreakerPolicy());
        
        // Register Atomic Handlers
        services.AddScoped<CreateLeaveAtomicHandler>();
        
        // Register Handlers
        services.AddScoped<CreateLeaveHandler>();
        
        // Register Services
        services.AddScoped<ILeaveMgmt, LeaveMgmtService>();
        
        // Register logging
        services.AddLogging(logging =>
        {
            logging.AddConsole();
            logging.AddDebug();
            logging.AddApplicationInsights();
        });
    })
    .Build();

host.Run();

/// <summary>
/// Retry policy for HTTP calls
/// Retries 3 times with exponential backoff
/// </summary>
static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .WaitAndRetryAsync(
            retryCount: 3,
            sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
            onRetry: (outcome, timespan, retryCount, context) =>
            {
                Console.WriteLine($"Retry {retryCount} after {timespan.TotalSeconds}s due to {outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString()}");
            });
}

/// <summary>
/// Circuit breaker policy for HTTP calls
/// Opens circuit after 5 consecutive failures, stays open for 30 seconds
/// </summary>
static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .CircuitBreakerAsync(
            handledEventsAllowedBeforeBreaking: 5,
            durationOfBreak: TimeSpan.FromSeconds(30),
            onBreak: (outcome, timespan) =>
            {
                Console.WriteLine($"Circuit breaker opened for {timespan.TotalSeconds}s due to {outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString()}");
            },
            onReset: () =>
            {
                Console.WriteLine("Circuit breaker reset");
            });
}
