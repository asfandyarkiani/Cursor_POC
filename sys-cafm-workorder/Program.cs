using Core.Middlewares;
using Core.SystemLayer.Middlewares;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;
using sys_cafm_workorder.Abstractions;
using sys_cafm_workorder.ConfigModels;
using sys_cafm_workorder.Implementations.FSI.AtomicHandlers;
using sys_cafm_workorder.Implementations.FSI.Handlers;
using sys_cafm_workorder.Implementations.FSI.Services;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults(worker =>
    {
        worker.UseMiddleware<ExceptionHandlerMiddleware>();
        worker.UseMiddleware<ExecutionTimingMiddleware>();
    })
    .ConfigureAppConfiguration((context, config) =>
    {
        config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
        config.AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true);
        config.AddEnvironmentVariables();
    })
    .ConfigureServices((context, services) =>
    {
        // Configuration
        services.Configure<AppConfigs>(context.Configuration.GetSection("AppConfigs"));

        // HTTP Client with Polly retry policy
        var retryPolicy = HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

        services.AddHttpClient("CAFMClient")
            .AddPolicyHandler(retryPolicy);

        services.AddSingleton<IAsyncPolicy<HttpResponseMessage>>(retryPolicy);

        services.AddSingleton<CustomHTTPClient>(sp =>
        {
            var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient("CAFMClient");
            var logger = sp.GetRequiredService<ILogger<CustomHTTPClient>>();
            var policy = sp.GetRequiredService<IAsyncPolicy<HttpResponseMessage>>();
            return new CustomHTTPClient(httpClient, logger, policy);
        });

        // Register Atomic Handlers
        services.AddScoped<CAFMAuthenticateAtomicHandler>();
        services.AddScoped<CAFMLogoutAtomicHandler>();
        services.AddScoped<CAFMGetBreakdownTasksAtomicHandler>();
        services.AddScoped<CAFMGetLocationsAtomicHandler>();
        services.AddScoped<CAFMGetInstructionSetsAtomicHandler>();
        services.AddScoped<CAFMCreateBreakdownTaskAtomicHandler>();
        services.AddScoped<CAFMCreateEventAtomicHandler>();

        // Register Handlers
        services.AddScoped<CAFMAuthenticateHandler>();
        services.AddScoped<CAFMLogoutHandler>();
        services.AddScoped<CAFMGetBreakdownTasksHandler>();
        services.AddScoped<CAFMGetLocationsHandler>();
        services.AddScoped<CAFMGetInstructionSetsHandler>();
        services.AddScoped<CAFMCreateBreakdownTaskHandler>();
        services.AddScoped<CAFMCreateEventHandler>();

        // Register Services
        services.AddScoped<ICAFMAuthenticationService, CAFMAuthenticationService>();
        services.AddScoped<ICAFMWorkOrderService, CAFMWorkOrderService>();

        // Logging
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
    })
    .Build();

host.Run();
