using Cache.Extensions;
using Core.DI;
using Core.Middlewares;
using Core.SystemLayer.Middlewares;
using FsiCafmSystem.Abstractions;
using FsiCafmSystem.ConfigModels;
using FsiCafmSystem.Helper;
using FsiCafmSystem.Implementations.FsiCafm.AtomicHandlers;
using FsiCafmSystem.Implementations.FsiCafm.Handlers;
using FsiCafmSystem.Implementations.FsiCafm.Services;
using FsiCafmSystem.Middleware;
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

// 3. Application Insights & Logging
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

// 6. JSON Options
builder.Services.Configure<JsonSerializerOptions>(options =>
{
    options.Converters.Add(new JsonStringEnumConverter());
    options.PropertyNameCaseInsensitive = true;
});

// 7. Services (WITH interfaces)
builder.Services.AddScoped<IWorkOrderMgmt, WorkOrderMgmtService>();
builder.Services.AddScoped<IEmailMgmt, EmailMgmtService>();

// 8. HTTP/SOAP/SMTP Clients
builder.Services.AddScoped<CustomHTTPClient>();
builder.Services.AddScoped<CustomSoapClient>();
builder.Services.AddScoped<CustomSmtpClient>();

// 9. Singletons/Helpers
builder.Services.AddSingleton<KeyVaultReader>();

// 10. Handlers
builder.Services.AddScoped<CreateWorkOrderHandler>();
builder.Services.AddScoped<SendEmailHandler>();

// 11. Atomic Handlers
builder.Services.AddScoped<AuthenticateAtomicHandler>();
builder.Services.AddScoped<LogoutAtomicHandler>();
builder.Services.AddScoped<GetLocationsByDtoAtomicHandler>();
builder.Services.AddScoped<GetInstructionSetsByDtoAtomicHandler>();
builder.Services.AddScoped<GetBreakdownTasksByDtoAtomicHandler>();
builder.Services.AddScoped<CreateBreakdownTaskAtomicHandler>();
builder.Services.AddScoped<CreateEventAtomicHandler>();
builder.Services.AddScoped<SendEmailAtomicHandler>();

// 12. Redis Caching (optional)
// builder.Services.AddRedisCacheLibrary(builder.Configuration);

// 13. Polly Policies
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

// 14. Middleware Registration (CRITICAL ORDER)
builder.UseMiddleware<ExecutionTimingMiddleware>();
builder.UseMiddleware<ExceptionHandlerMiddleware>();
builder.UseMiddleware<CustomAuthenticationMiddleware>();

// 15. Service Locator
ServiceLocator.ServiceProvider = builder.Services.BuildServiceProvider();

builder.Build().Run();
