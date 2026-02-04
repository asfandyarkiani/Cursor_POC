using AlGhurair.Core.SystemLayer.Handlers;
using AlGhurair.Core.SystemLayer.Middlewares;
using AlGhurair.SystemLayer.OracleFusionHCM.Abstractions;
using AlGhurair.SystemLayer.OracleFusionHCM.ConfigModels;
using AlGhurair.SystemLayer.OracleFusionHCM.DTO.AtomicHandlerDTOs;
using AlGhurair.SystemLayer.OracleFusionHCM.DTO.CreateLeaveDTO;
using AlGhurair.SystemLayer.OracleFusionHCM.DTO.DownstreamDTOs;
using AlGhurair.SystemLayer.OracleFusionHCM.Implementations.OracleFusionHCM.AtomicHandlers;
using AlGhurair.SystemLayer.OracleFusionHCM.Implementations.OracleFusionHCM.Handlers;
using AlGhurair.SystemLayer.OracleFusionHCM.Implementations.OracleFusionHCM.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Polly;
using Polly.Extensions.Http;

namespace AlGhurair.SystemLayer.OracleFusionHCM;

public class Program
{
    public static void Main()
    {
        IHost host = new HostBuilder()
            .ConfigureFunctionsWebApplication(builder =>
            {
                // Register middleware in EXACT order: ExecutionTiming → ExceptionHandler
                builder.UseMiddleware<ExecutionTimingMiddleware>();
                builder.UseMiddleware<ExceptionHandlerMiddleware>();
            })
            .ConfigureAppConfiguration((context, config) =>
            {
                // Load appsettings based on environment
                string environmentName = context.HostingEnvironment.EnvironmentName;
                
                config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                config.AddJsonFile($"appsettings.{environmentName}.json", optional: true, reloadOnChange: true);
                config.AddEnvironmentVariables();
            })
            .ConfigureServices((context, services) =>
            {
                // Register Application Insights
                services.AddApplicationInsightsTelemetryWorkerService();
                services.ConfigureFunctionsApplicationInsights();

                // Register Configuration
                services.Configure<AppConfigs>(context.Configuration);

                // Register CustomHTTPClient with Polly policies
                services.AddHttpClient<CustomHTTPClient>()
                    .AddPolicyHandler(GetRetryPolicy())
                    .AddPolicyHandler(GetCircuitBreakerPolicy());

                // Register Services
                services.AddScoped<ILeaveMgmt, LeaveMgmtService>();

                // Register Handlers
                services.AddScoped<IBaseHandler<CreateLeaveReqDTO, CreateLeaveResDTO>, CreateLeaveHandler>();

                // Register Atomic Handlers
                services.AddScoped<IAtomicHandler<CreateLeaveHandlerReqDTO, CreateLeaveApiResDTO>, CreateLeaveAtomicHandler>();
            })
            .Build();

        host.Run();
    }

    /// <summary>
    /// Retry policy for HTTP requests
    /// Retries 3 times with exponential backoff
    /// </summary>
    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                    Console.WriteLine($"Retry {retryCount} after {timespan.TotalSeconds} seconds due to: {outcome.Exception?.Message ?? outcome.Result.StatusCode.ToString()}");
                });
    }

    /// <summary>
    /// Circuit breaker policy for HTTP requests
    /// Breaks circuit after 5 consecutive failures for 30 seconds
    /// </summary>
    private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromSeconds(30),
                onBreak: (outcome, duration) =>
                {
                    Console.WriteLine($"Circuit breaker opened for {duration.TotalSeconds} seconds due to: {outcome.Exception?.Message ?? outcome.Result.StatusCode.ToString()}");
                },
                onReset: () =>
                {
                    Console.WriteLine("Circuit breaker reset");
                });
    }
}
