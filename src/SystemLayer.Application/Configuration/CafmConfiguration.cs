using System.ComponentModel.DataAnnotations;

namespace SystemLayer.Application.Configuration;

public class CafmConfiguration
{
    public const string SectionName = "Cafm";

    [Required]
    public string BaseUrl { get; set; } = string.Empty;

    [Required]
    public string Username { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;

    [Required]
    public string Database { get; set; } = string.Empty;

    public int TimeoutSeconds { get; set; } = 30;

    public RetryConfiguration Retry { get; set; } = new();

    public CircuitBreakerConfiguration CircuitBreaker { get; set; } = new();

    public SoapConfiguration Soap { get; set; } = new();
}

public class RetryConfiguration
{
    public int MaxAttempts { get; set; } = 3;
    public int BaseDelaySeconds { get; set; } = 2;
    public int MaxDelaySeconds { get; set; } = 30;
}

public class CircuitBreakerConfiguration
{
    public int FailureThreshold { get; set; } = 5;
    public int SamplingDurationSeconds { get; set; } = 60;
    public int MinimumThroughput { get; set; } = 3;
    public int DurationOfBreakSeconds { get; set; } = 30;
}

public class SoapConfiguration
{
    public string LoginAction { get; set; } = "Login";
    public string LogoutAction { get; set; } = "Logout";
    public string CreateWorkOrderAction { get; set; } = "CreateWorkOrder";
    public string GetBreakdownTaskAction { get; set; } = "GetBreakdownTask";
    public string GetLocationAction { get; set; } = "GetLocation";
    public string GetInstructionSetsAction { get; set; } = "GetInstructionSets";
    public string SoapNamespace { get; set; } = "http://tempuri.org/";
    public string ContentType { get; set; } = "text/xml; charset=utf-8";
}