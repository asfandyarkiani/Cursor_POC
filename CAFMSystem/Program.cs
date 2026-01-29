using Cache.Extensions;
using CAFMSystem.Abstractions;
using CAFMSystem.ConfigModels;
using CAFMSystem.Helper;
using CAFMSystem.Implementations.FSI.AtomicHandlers;
using CAFMSystem.Implementations.FSI.Handlers;
using CAFMSystem.Implementations.FSI.Services;
using CAFMSystem.Middleware;
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
using System;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;

FunctionsApplicationBuilder builder = FunctionsApplication.CreateBuilder(args);

// 0. Logging (FIRST)
builder.Services.AddApplicationInsightsTelemetryWorkerService().ConfigureFunctionsApplicationInsights();
builder.Logging.AddConsole();
builder.Logging.AddFilter<ApplicationInsightsLoggerProvider>("", LogLevel.Information);

// 1. Environment Detection
string environment = Environment.GetEnvironmentVariable("ENVIRONMENT") 
    ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") 
    ?? "dev";

// 2. Configuration Loading
builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// 3. Configuration Binding
builder.Services.Configure<AppConfigs>(builder.Configuration.GetSection(AppConfigs.SectionName));
builder.Services.Configure<KeyVaultConfigs>(builder.Configuration.GetSection(KeyVaultConfigs.SectionName));

// 4. Configure Functions Web Application
builder.ConfigureFunctionsWebApplication();

// 5. HTTP Client
builder.Services.AddHttpClient<CustomHTTPClient>();

// 6. JSON Options
builder.Services.Configure<JsonSerializerOptions>(options =>
{
    options.Converters.Add(new JsonStringEnumConverter());
    options.PropertyNameCaseInsensitive = true;
});

// 7. Singletons
builder.Services.AddSingleton<KeyVaultReader>();

// 8. HTTP/SOAP Clients
builder.Services.AddScoped<CustomHTTPClient>();
builder.Services.AddScoped<CustomSoapClient>();

// 9. Services (WITH interfaces)
builder.Services.AddScoped<IWorkOrderMgmt, WorkOrderMgmtService>();

// 10. Handlers (CONCRETE)
builder.Services.AddScoped<GetBreakdownTasksByDtoHandler>();
builder.Services.AddScoped<CreateBreakdownTaskHandler>();
builder.Services.AddScoped<CreateEventHandler>();

// 11. Atomic Handlers (CONCRETE)
builder.Services.AddScoped<AuthenticateAtomicHandler>();
builder.Services.AddScoped<LogoutAtomicHandler>();
builder.Services.AddScoped<GetBreakdownTasksByDtoAtomicHandler>();
builder.Services.AddScoped<GetLocationsByDtoAtomicHandler>();
builder.Services.AddScoped<GetInstructionSetsByDtoAtomicHandler>();
builder.Services.AddScoped<CreateBreakdownTaskAtomicHandler>();
builder.Services.AddScoped<CreateEventAtomicHandler>();

// 12. Redis Cache
builder.Services.AddRedisCacheLibrary(builder.Configuration);

// 13. Polly Policies
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

// 14. Middleware (ORDER CRITICAL)
builder.UseMiddleware<ExecutionTimingMiddleware>();
builder.UseMiddleware<ExceptionHandlerMiddleware>();
builder.UseMiddleware<CustomAuthenticationMiddleware>();

// 15. Service Locator (LAST)
ServiceLocator.ServiceProvider = builder.Services.BuildServiceProvider();

builder.Build().Run();
