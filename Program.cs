using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;
using FacilitiesMgmtSystem.ConfigModels;
using FacilitiesMgmtSystem.Helper;
using FacilitiesMgmtSystem.Middlewares;
using FacilitiesMgmtSystem.Implementations.MRI.Handlers;
using FacilitiesMgmtSystem.Implementations.MRI.AtomicHandlers.CreateWorkOrderAtomicHandler;
using FacilitiesMgmtSystem.Implementations.MRI.AtomicHandlers.GetBreakdownTaskAtomicHandler;
using FacilitiesMgmtSystem.Implementations.MRI.AtomicHandlers.GetLocationAtomicHandler;
using FacilitiesMgmtSystem.Implementations.MRI.AtomicHandlers.GetInstructionSetsAtomicHandler;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication(builder =>
    {
        // Register middleware in correct order
        builder.UseMiddleware<ExecutionTimingMiddleware>();
        builder.UseMiddleware<ExceptionHandlerMiddleware>();
        builder.UseMiddleware<MRIAuthenticationMiddleware>();
    })
    .ConfigureAppConfiguration((context, config) =>
    {
        // Determine environment
        var environment = Environment.GetEnvironmentVariable("ENVIRONMENT")
                       ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
                       ?? "dev";

        config.SetBasePath(Directory.GetCurrentDirectory())
              .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
              .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
              .AddEnvironmentVariables();
    })
    .ConfigureServices((context, services) =>
    {
        // NOTE: Application Insights can be configured via Azure portal 
        // or by adding Microsoft.ApplicationInsights.WorkerService package
        // services.AddApplicationInsightsTelemetryWorkerService();
        // services.ConfigureFunctionsApplicationInsights();

        // Register configuration
        services.Configure<AppConfigs>(context.Configuration);

        // Get HTTP client configuration
        var httpClientConfig = context.Configuration.GetSection("HttpClient").Get<HttpClientConfig>()
                             ?? new HttpClientConfig();

        // Create retry policy
        var retryPolicy = HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(
                httpClientConfig.RetryCount,
                retryAttempt => TimeSpan.FromSeconds(
                    Math.Pow(httpClientConfig.RetryDelaySeconds, retryAttempt)));

        // Create circuit breaker policy
        var circuitBreakerPolicy = HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: httpClientConfig.CircuitBreakerFailureThreshold,
                durationOfBreak: TimeSpan.FromSeconds(httpClientConfig.CircuitBreakerBreakDurationSeconds));

        // Combine policies
        var combinedPolicy = Policy.WrapAsync(retryPolicy, circuitBreakerPolicy);

        // Register the combined policy as a singleton
        services.AddSingleton<IAsyncPolicy<HttpResponseMessage>>(combinedPolicy);

        // Register HttpClient factory for CustomSoapClient
        services.AddHttpClient<CustomSoapClient>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(httpClientConfig.TimeoutSeconds);
        })
        .AddPolicyHandler(combinedPolicy);

        // Register HttpClient factory for CustomHTTPClient
        services.AddHttpClient<CustomHTTPClient>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(httpClientConfig.TimeoutSeconds);
        })
        .AddPolicyHandler(combinedPolicy);

        // Register Middlewares
        services.AddScoped<ExecutionTimingMiddleware>();
        services.AddScoped<ExceptionHandlerMiddleware>();
        services.AddScoped<MRIAuthenticationMiddleware>();

        // Register MRI Handlers
        services.AddScoped<CreateWorkOrderMRIHandler>();
        services.AddScoped<GetBreakdownTaskMRIHandler>();
        services.AddScoped<GetLocationMRIHandler>();
        services.AddScoped<GetInstructionSetsMRIHandler>();

        // Register Atomic Handlers
        services.AddScoped<CreateWorkOrderAtomicHandler>();
        services.AddScoped<GetBreakdownTaskAtomicHandler>();
        services.AddScoped<GetLocationAtomicHandler>();
        services.AddScoped<GetInstructionSetsAtomicHandler>();
    })
    .ConfigureLogging((context, logging) =>
    {
        logging.AddConsole();
        logging.SetMinimumLevel(LogLevel.Information);
    })
    .Build();

host.Run();
