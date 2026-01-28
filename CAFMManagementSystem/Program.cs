using Cache.Extensions;
using CAFMManagementSystem.Abstractions;
using CAFMManagementSystem.ConfigModels;
using CAFMManagementSystem.Helper;
using CAFMManagementSystem.Implementations.FSIConcept.AtomicHandlers;
using CAFMManagementSystem.Implementations.FSIConcept.Handlers;
using CAFMManagementSystem.Implementations.FSIConcept.Services;
using CAFMManagementSystem.Middleware;
using Core.DI;
using Core.Middlewares;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.ApplicationInsights;
using Polly;

FunctionsApplicationBuilder builder = FunctionsApplication.CreateBuilder(args);

// 1. Environment & Configuration Loading
string environment = Environment.GetEnvironmentVariable("ENVIRONMENT") 
    ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") 
    ?? "dev";

builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// 2. Application Insights & Logging (FIRST)
builder.Services.AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();
builder.Logging.AddConsole();
builder.Logging.AddFilter<ApplicationInsightsLoggerProvider>("", LogLevel.Information);

// 3. Configuration Binding
builder.Services.Configure<AppConfigs>(builder.Configuration.GetSection(AppConfigs.SectionName));
builder.Services.Configure<KeyVaultConfigs>(builder.Configuration.GetSection(KeyVaultConfigs.SectionName));

// 4. Configure Functions Web Application
builder.ConfigureFunctionsWebApplication();

// 5. HTTP Clients
builder.Services.AddHttpClient<CustomHTTPClient>();

// 6. Redis Caching
builder.Services.AddRedisCacheLibrary(builder.Configuration);

// 7. Helpers (Singletons)
builder.Services.AddSingleton<KeyVaultReader>();
builder.Services.AddScoped<CustomSoapClient>();

// 8. Services (WITH interfaces)
builder.Services.AddScoped<IBreakdownTaskMgmt, BreakdownTaskMgmtService>();

// 9. Handlers (CONCRETE)
builder.Services.AddScoped<GetBreakdownTasksByDtoHandler>();
builder.Services.AddScoped<CreateBreakdownTaskHandler>();

// 10. Atomic Handlers (CONCRETE)
builder.Services.AddScoped<AuthenticateAtomicHandler>();
builder.Services.AddScoped<LogoutAtomicHandler>();
builder.Services.AddScoped<GetBreakdownTasksByDtoAtomicHandler>();
builder.Services.AddScoped<CreateBreakdownTaskAtomicHandler>();
builder.Services.AddScoped<GetLocationsByDtoAtomicHandler>();
builder.Services.AddScoped<GetInstructionSetsByDtoAtomicHandler>();
builder.Services.AddScoped<CreateEventAtomicHandler>();

// 11. CustomHTTPClient
builder.Services.AddScoped<CustomHTTPClient>();

// 12. Polly Policies (Singleton)
builder.Services.AddSingleton<IAsyncPolicy<HttpResponseMessage>>(sp =>
{
    IConfiguration config = sp.GetRequiredService<IConfiguration>();
    
    IAsyncPolicy<HttpResponseMessage> retryPolicy = Policy
        .Handle<HttpRequestException>()
        .OrResult<HttpResponseMessage>(r => (int)r.StatusCode >= 500)
        .WaitAndRetryAsync(
            retryCount: config.GetValue<int>("HttpClientPolicy:RetryCount", 0),
            sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(retryAttempt * 5)
        );
    
    IAsyncPolicy<HttpResponseMessage> timeoutPolicy = Policy
        .TimeoutAsync<HttpResponseMessage>(config.GetValue<int>("HttpClientPolicy:TimeoutSeconds", 60));
    
    return Policy.WrapAsync(retryPolicy, timeoutPolicy);
});

// 13. Middleware Registration (STRICT ORDER)
builder.UseMiddleware<ExecutionTimingMiddleware>();
builder.UseMiddleware<ExceptionHandlerMiddleware>();
builder.UseMiddleware<CustomAuthenticationMiddleware>();

// 14. Service Locator (For static helpers)
ServiceLocator.ServiceProvider = builder.Services.BuildServiceProvider();

builder.Build().Run();
