using CAFMSystemLayer.Abstractions;
using CAFMSystemLayer.ConfigModels;
using CAFMSystemLayer.Helper;
using CAFMSystemLayer.Implementations.CAFM.AtomicHandlers;
using CAFMSystemLayer.Implementations.CAFM.Handlers;
using CAFMSystemLayer.Implementations.CAFM.Services;
using CAFMSystemLayer.Middleware;
using Core.DI;
using Core.Middlewares;
using Core.SystemLayer.Middlewares;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.ApplicationInsights;
using Polly;

FunctionsApplicationBuilder builder = FunctionsApplication.CreateBuilder(args);

// 1. Environment Detection
string environment = Environment.GetEnvironmentVariable("ENVIRONMENT") 
    ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") 
    ?? "dev";

// 2. Configuration Loading
builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// 3. Application Insights & Logging (FIRST service registration)
builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

builder.Logging.AddConsole();
builder.Logging.AddFilter<ApplicationInsightsLoggerProvider>("", LogLevel.Information);

// 4. Configuration Binding
builder.Services.Configure<AppConfigs>(builder.Configuration.GetSection(AppConfigs.SectionName));

// 5. Configure Functions Web Application
builder.ConfigureFunctionsWebApplication();

// 6. HTTP Client
builder.Services.AddHttpClient<CustomHTTPClient>();

// 7. Services (WITH interfaces)
builder.Services.AddScoped<IWorkOrderMgmt, WorkOrderMgmtService>();

// 8. HTTP/SOAP Clients
builder.Services.AddScoped<CustomHTTPClient>();
builder.Services.AddScoped<CustomSoapClient>();

// 9. Handlers (CONCRETE only)
builder.Services.AddScoped<CreateWorkOrderHandler>();

// 10. Atomic Handlers (CONCRETE only + auth handlers)
builder.Services.AddScoped<AuthenticateAtomicHandler>();
builder.Services.AddScoped<LogoutAtomicHandler>();
builder.Services.AddScoped<GetLocationsAtomicHandler>();
builder.Services.AddScoped<GetInstructionSetsAtomicHandler>();
builder.Services.AddScoped<GetBreakdownTasksAtomicHandler>();
builder.Services.AddScoped<CreateBreakdownTaskAtomicHandler>();
builder.Services.AddScoped<CreateEventLinkTaskAtomicHandler>();

// 11. Polly Policy (MANDATORY)
builder.Services.AddSingleton<IAsyncPolicy<HttpResponseMessage>>(sp =>
{
    IConfiguration config = sp.GetRequiredService<IConfiguration>();
    
    IAsyncPolicy<HttpResponseMessage> retryPolicy = Policy
        .Handle<HttpRequestException>()
        .OrResult<HttpResponseMessage>(r => (int)r.StatusCode >= 500)
        .WaitAndRetryAsync(
            retryCount: config.GetValue<int>("HttpClientPolicy:RetryCount"),
            sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(retryAttempt * 5)
        );
    
    IAsyncPolicy<HttpResponseMessage> timeoutPolicy = Policy
        .TimeoutAsync<HttpResponseMessage>(config.GetValue<int>("HttpClientPolicy:TimeoutSeconds"));
    
    return Policy.WrapAsync(retryPolicy, timeoutPolicy);
});

// 12. Middleware Registration (EXACT ORDER - NON-NEGOTIABLE)
builder.UseMiddleware<ExecutionTimingMiddleware>();      // 1. FIRST - timing
builder.UseMiddleware<ExceptionHandlerMiddleware>();     // 2. SECOND - exception handling
builder.UseMiddleware<CustomAuthenticationMiddleware>(); // 3. THIRD - auth lifecycle

// 13. Service Locator (LAST - for static helpers)
ServiceLocator.ServiceProvider = builder.Services.BuildServiceProvider();

builder.Build().Run();
