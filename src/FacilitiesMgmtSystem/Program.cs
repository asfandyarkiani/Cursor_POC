using FacilitiesMgmtSystem.ConfigModels;
using FacilitiesMgmtSystem.Helper;
using FacilitiesMgmtSystem.Implementations.MRI.Handlers;
using FacilitiesMgmtSystem.Middlewares;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Polly;
using Polly.Extensions.Http;

var builder = FunctionsApplication.CreateBuilder(args);

// Detect environment
var environment = Environment.GetEnvironmentVariable("ENVIRONMENT")
    ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
    ?? "dev";

// Configure application settings
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

builder.ConfigureFunctionsWebApplication();

// Register Application Insights
builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

// Register configuration
builder.Services.Configure<AppConfigs>(builder.Configuration.GetSection("AppVariables"));

// Register Polly retry policy
var retryPolicy = HttpPolicyExtensions
    .HandleTransientHttpError()
    .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

builder.Services.AddSingleton<IAsyncPolicy<HttpResponseMessage>>(retryPolicy);

// Register HTTP clients
builder.Services.AddHttpClient<CustomHTTPClient>();
builder.Services.AddHttpClient<CustomSoapClient>();

// Register handlers
builder.Services.AddScoped<NetworkTestMRIHandler>();

// Register middleware
builder.UseMiddleware<ExecutionTimingMiddleware>();
builder.UseMiddleware<ExceptionHandlerMiddleware>();
builder.UseMiddleware<MRIAuthenticationMiddleware>();

builder.Build().Run();
