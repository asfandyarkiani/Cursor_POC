using SystemLayer.Infrastructure.Configuration;
using SystemLayer.Infrastructure.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Configuration - Add Azure Key Vault if enabled
var keyVaultOptions = builder.Configuration.GetSection(KeyVaultOptions.SectionName).Get<KeyVaultOptions>() ?? new KeyVaultOptions();
if (keyVaultOptions.Enabled)
{
    builder.Configuration.AddAzureKeyVaultIfEnabled(keyVaultOptions);
}

// Add services to the container
builder.Services.AddControllers();

// Add System Layer services
builder.Services.AddSystemLayerInfrastructure(builder.Configuration);

// Add health checks
builder.Services.AddHealthChecks();

// API Documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "System Layer API",
        Version = "v1",
        Description = "CAFM Integration System Layer API for Process Layer communication"
    });
    
    // Include XML comments if available
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// CORS (if needed for cross-origin requests)
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "System Layer API v1");
        c.RoutePrefix = string.Empty; // Serve Swagger UI at root
    });
}

// Security headers
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    await next();
});

app.UseCors();

// Health checks
app.MapHealthChecks("/health");

app.UseRouting();
app.UseAuthorization();
app.MapControllers();

// Startup logging
var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("System Layer API starting up...");

app.Run();