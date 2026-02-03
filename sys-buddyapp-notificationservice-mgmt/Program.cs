using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using AGI.Enterprise.Automotive.BuddyApp.NotificationService.Mgmt.ConfigModels;
using AGI.Enterprise.Automotive.BuddyApp.NotificationService.Mgmt.Abstractions;
using AGI.Enterprise.Automotive.BuddyApp.NotificationService.Mgmt.Implementations.BuddyAppMicroservice.Services;
using Core.Middlewares;
using Core.SystemLayer.Middlewares;

IHost host = new HostBuilder()
    .ConfigureFunctionsWebApplication(builder =>
    {
        // Register middleware in EXACT order: ExecutionTiming → ExceptionHandler
        builder.UseMiddleware<ExecutionTimingMiddleware>();
        builder.UseMiddleware<ExceptionHandlerMiddleware>();
    })
    .ConfigureAppConfiguration((context, config) =>
    {
        string environmentName = context.HostingEnvironment.EnvironmentName;
        
        config
            .SetBasePath(context.HostingEnvironment.ContentRootPath)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{environmentName}.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables();
    })
    .ConfigureServices((context, services) =>
    {
        // Register Application Insights
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        // Register configuration
        services.Configure<AppConfigs>(context.Configuration.GetSection("AppConfigs"));

        // Register HttpClient with Polly policies
        services.AddHttpClient<CustomHTTPClient>()
            .AddPolicyHandler(CustomHTTPClient.GetRetryPolicy())
            .AddPolicyHandler(CustomHTTPClient.GetCircuitBreakerPolicy());

        // Register CustomHTTPClient
        services.AddScoped<CustomHTTPClient>();

        // Register Atomic Handlers
        services.AddScoped<AGI.Enterprise.Automotive.BuddyApp.NotificationService.Mgmt.Implementations.BuddyAppMicroservice.AtomicHandlers.SendPushNotificationAtomicHandler>();

        // Register Handlers
        services.AddScoped<AGI.Enterprise.Automotive.BuddyApp.NotificationService.Mgmt.Implementations.BuddyAppMicroservice.Handlers.SendPushNotificationHandler>();

        // Register Services
        services.AddScoped<INotificationMgmt, NotificationMgmtService>();
    })
    .Build();

host.Run();
