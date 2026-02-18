using Core.Middlewares;
using HcmLeaveProcessLayer.ConfigModels;
using HcmLeaveProcessLayer.Constants;
using HcmLeaveProcessLayer.Services;
using HcmLeaveProcessLayer.SystemAbstractions.HcmMgmt;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;

FunctionsHostBuilder builder = FunctionsApplication.CreateBuilder(args);

builder.Services.AddHttpClient<CustomHTTPClient>();

string environment = Environment.GetEnvironmentVariable("ENVIRONMENT")
    ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
    ?? InfoConstants.DEFAULT_ENVIRONMENT;

builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

builder.Services.AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

builder.Logging.AddConsole();
builder.Logging.AddFilter<ApplicationInsightsLoggerProvider>("", LogLevel.Information);

builder.Services.Configure<AppConfigs>(builder.Configuration.GetSection(AppConfigs.SectionName));

builder.Services.AddRedisCacheLibrary(builder.Configuration);

builder.Services.AddScoped<ILeaveMgmt, LeaveMgmtSys>();

builder.Services.AddScoped<LeaveService>();

builder.Services.AddScoped<CustomHTTPClient>();

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

builder.ConfigureFunctionsWebApplication(app =>
{
    app.UseMiddleware<ExecutionTimingMiddleware>();
    app.UseMiddleware<ExceptionHandlerMiddleware>();
});

ServiceLocator.ServiceProvider = builder.Services.BuildServiceProvider();

builder.Build().Run();
