using Cache.Extensions;
using CAFMSystem.Abstractions;
using CAFMSystem.ConfigModels;
using CAFMSystem.Helper;
using CAFMSystem.Implementations.CAFM.AtomicHandlers;
using CAFMSystem.Implementations.CAFM.Handlers;
using CAFMSystem.Implementations.CAFM.Services;
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
using System;
using System.Net.Http;
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

// 4. Configuration Models
builder.Services.Configure<AppConfigs>(builder.Configuration.GetSection(AppConfigs.SectionName));
builder.Services.Configure<KeyVaultConfigs>(builder.Configuration.GetSection(KeyVaultConfigs.SectionName));

// 5. Configure Functions Web Application
builder.ConfigureFunctionsWebApplication();

// 6. HTTP Client
builder.Services.AddHttpClient<CustomHTTPClient>();

// 7. JSON Options
builder.Services.Configure<JsonSerializerOptions>(options =>
{
    options.Converters.Add(new JsonStringEnumConverter());
    options.PropertyNameCaseInsensitive = true;
});

// 8. Services (WITH interfaces)
builder.Services.AddScoped<IBreakdownTaskMgmt, BreakdownTaskMgmtService>();

// 9. HTTP/SOAP Clients
builder.Services.AddScoped<CustomHTTPClient>();
builder.Services.AddScoped<CustomSoapClient>();

// 10. Singletons/Helpers
builder.Services.AddSingleton<KeyVaultReader>();

// 11. Handlers (CONCRETE)
builder.Services.AddScoped<GetBreakdownTasksByDtoHandler>();
builder.Services.AddScoped<CreateBreakdownTaskHandler>();
builder.Services.AddScoped<CreateEventHandler>();

// 12. Atomic Handlers (CONCRETE)
builder.Services.AddScoped<AuthenticateAtomicHandler>();
builder.Services.AddScoped<LogoutAtomicHandler>();
builder.Services.AddScoped<GetLocationsByDtoAtomicHandler>();
builder.Services.AddScoped<GetInstructionSetsByDtoAtomicHandler>();
builder.Services.AddScoped<GetBreakdownTasksByDtoAtomicHandler>();
builder.Services.AddScoped<CreateBreakdownTaskAtomicHandler>();
builder.Services.AddScoped<CreateEventAtomicHandler>();

// 13. Redis Cache Library
builder.Services.AddRedisCacheLibrary(builder.Configuration);

// 14. Polly Policy
builder.Services.AddSingleton<IAsyncPolicy<HttpResponseMessage>>(sp =>
{
    IConfiguration config = sp.GetRequiredService<IConfiguration>();
    
    IAsyncPolicy<HttpResponseMessage> retryPolicy = Policy
        .Handle<HttpRequestException>()
        .OrResult<HttpResponseMessage>(r => (int)r.StatusCode >= 500)
        .WaitAndRetryAsync(
            retryCount: config.GetValue<int>("HttpClientPolicy:RetryCount", 0),
            sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(retryAttempt * 5));
    
    IAsyncPolicy<HttpResponseMessage> timeoutPolicy = Policy
        .TimeoutAsync<HttpResponseMessage>(config.GetValue<int>("HttpClientPolicy:TimeoutSeconds", 60));
    
    return Policy.WrapAsync(retryPolicy, timeoutPolicy);
});

// 15. Middleware Registration (ORDER CRITICAL)
builder.UseMiddleware<ExecutionTimingMiddleware>();
builder.UseMiddleware<ExceptionHandlerMiddleware>();
builder.UseMiddleware<CustomAuthenticationMiddleware>();

// 16. Service Locator (LAST before Build)
ServiceLocator.ServiceProvider = builder.Services.BuildServiceProvider();

builder.Build().Run();
