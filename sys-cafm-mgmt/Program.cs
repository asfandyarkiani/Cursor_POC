using Cache.Extensions;
using CAFMSystem.Abstractions;
using CAFMSystem.ConfigModels;
using CAFMSystem.Helpers;
using CAFMSystem.Implementations.FSI.AtomicHandlers;
using CAFMSystem.Implementations.FSI.Handlers;
using CAFMSystem.Implementations.FSI.Services;
using CAFMSystem.Middleware;
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
using System.Text.Json;
using System.Text.Json.Serialization;

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
builder.Services.AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();
builder.Logging.AddConsole();
builder.Logging.AddFilter<ApplicationInsightsLoggerProvider>("", LogLevel.Information);

// 4. Configuration Binding
builder.Services.Configure<AppConfigs>(builder.Configuration.GetSection(AppConfigs.SectionName));
builder.Services.Configure<KeyVaultConfigs>(builder.Configuration.GetSection(KeyVaultConfigs.SectionName));

// 5. Configure Functions Web Application
builder.ConfigureFunctionsWebApplication();
builder.Services.AddHttpClient<CustomHTTPClient>();

// 6. JSON Options (for enum serialization)
builder.Services.Configure<JsonSerializerOptions>(options =>
{
    options.Converters.Add(new JsonStringEnumConverter());
    options.PropertyNameCaseInsensitive = true;
});

// 7. Services (WITH interfaces)
builder.Services.AddScoped<IWorkOrderMgmt, WorkOrderMgmtService>();

// 8. HTTP/SOAP Clients
builder.Services.AddScoped<CustomHTTPClient>();
builder.Services.AddScoped<CustomSoapClient>();

// 9. Handlers (CONCRETE only)
builder.Services.AddScoped<CreateWorkOrderHandler>();

// 10. Atomic Handlers (CONCRETE only)
builder.Services.AddScoped<AuthenticateAtomicHandler>();
builder.Services.AddScoped<LogoutAtomicHandler>();
builder.Services.AddScoped<GetLocationsByDtoAtomicHandler>();
builder.Services.AddScoped<GetInstructionSetsByDtoAtomicHandler>();
builder.Services.AddScoped<CreateBreakdownTaskAtomicHandler>();
builder.Services.AddScoped<GetBreakdownTasksByDtoAtomicHandler>();
builder.Services.AddScoped<CreateEventAtomicHandler>();

// 11. Redis Cache Library
builder.Services.AddRedisCacheLibrary(builder.Configuration);

// 12. Polly Policy (Retry + Timeout)
builder.Services.AddSingleton<IAsyncPolicy<HttpResponseMessage>>(sp =>
{
    IConfiguration config = sp.GetRequiredService<IConfiguration>();
    
    IAsyncPolicy<HttpResponseMessage> retryPolicy = Policy
        .Handle<HttpRequestException>()
        .OrResult<HttpResponseMessage>(r => (int)r.StatusCode >= 500)
        .WaitAndRetryAsync(
            retryCount: config.GetValue<int>("HttpClientPolicy:RetryCount"),
            sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(retryAttempt * 5));
    
    IAsyncPolicy<HttpResponseMessage> timeoutPolicy = Policy
        .TimeoutAsync<HttpResponseMessage>(config.GetValue<int>("HttpClientPolicy:TimeoutSeconds"));
    
    return Policy.WrapAsync(retryPolicy, timeoutPolicy);
});

// 13. Middleware Registration (CRITICAL ORDER: ExecutionTiming → Exception → CustomAuth)
builder.UseMiddleware<ExecutionTimingMiddleware>();
builder.UseMiddleware<ExceptionHandlerMiddleware>();
builder.UseMiddleware<CustomAuthenticationMiddleware>();

// 14. Service Locator (LAST - after all registrations)
ServiceLocator.ServiceProvider = builder.Services.BuildServiceProvider();

builder.Build().Run();
