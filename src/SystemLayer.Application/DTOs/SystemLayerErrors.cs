namespace SystemLayer.Application.DTOs;

/// <summary>
/// Standardized error responses for System Layer
/// </summary>
public class SystemLayerError
{
    public string ErrorCode { get; set; } = string.Empty;
    
    public string Message { get; set; } = string.Empty;
    
    public string? Details { get; set; }
    
    public bool IsRetryable { get; set; }
    
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    public string? CorrelationId { get; set; }
}

public class SystemLayerResult<T>
{
    public bool Success { get; set; }
    
    public T? Data { get; set; }
    
    public SystemLayerError? Error { get; set; }
    
    public string? CorrelationId { get; set; }
    
    public static SystemLayerResult<T> Ok(T data, string? correlationId = null)
    {
        return new SystemLayerResult<T>
        {
            Success = true,
            Data = data,
            CorrelationId = correlationId
        };
    }
    
    public static SystemLayerResult<T> Fail(SystemLayerError error, string? correlationId = null)
    {
        return new SystemLayerResult<T>
        {
            Success = false,
            Error = error,
            CorrelationId = correlationId
        };
    }
    
    public static SystemLayerResult<T> Fail(string errorCode, string message, bool isRetryable = false, string? correlationId = null)
    {
        return Fail(new SystemLayerError
        {
            ErrorCode = errorCode,
            Message = message,
            IsRetryable = isRetryable,
            CorrelationId = correlationId
        }, correlationId);
    }
}

/// <summary>
/// Standard error codes for System Layer
/// </summary>
public static class SystemLayerErrorCodes
{
    // Authentication/Authorization errors
    public const string AuthenticationFailed = "SL_AUTH_001";
    public const string AuthorizationFailed = "SL_AUTH_002";
    public const string TokenExpired = "SL_AUTH_003";
    
    // CAFM/External system errors
    public const string CafmConnectionFailed = "SL_CAFM_001";
    public const string CafmTimeout = "SL_CAFM_002";
    public const string CafmSoapFault = "SL_CAFM_003";
    public const string CafmInvalidResponse = "SL_CAFM_004";
    public const string CafmServiceUnavailable = "SL_CAFM_005";
    
    // Validation errors
    public const string InvalidRequest = "SL_VAL_001";
    public const string MissingRequiredField = "SL_VAL_002";
    public const string InvalidFieldValue = "SL_VAL_003";
    
    // Business logic errors
    public const string WorkOrderNotFound = "SL_BIZ_001";
    public const string LocationNotFound = "SL_BIZ_002";
    public const string TaskNotFound = "SL_BIZ_003";
    public const string InstructionSetNotFound = "SL_BIZ_004";
    public const string DuplicateWorkOrder = "SL_BIZ_005";
    
    // System errors
    public const string InternalError = "SL_SYS_001";
    public const string ConfigurationError = "SL_SYS_002";
    public const string MappingError = "SL_SYS_003";
    public const string CircuitBreakerOpen = "SL_SYS_004";
}