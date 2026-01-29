namespace FacilitiesMgmtSystem.DTO;

/// <summary>
/// Base response DTO for all API responses.
/// </summary>
public class BaseResponseDTO
{
    /// <summary>
    /// Indicates whether the operation was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Response message providing additional context.
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Error properties containing error codes or details.
    /// </summary>
    public string[]? ErrorProperties { get; set; }

    /// <summary>
    /// Creates a success response.
    /// </summary>
    public static BaseResponseDTO SuccessResponse(string? message = null)
    {
        return new BaseResponseDTO
        {
            Success = true,
            Message = message ?? "Operation completed successfully."
        };
    }

    /// <summary>
    /// Creates an error response.
    /// </summary>
    public static BaseResponseDTO ErrorResponse(string message, string[]? errorProperties = null)
    {
        return new BaseResponseDTO
        {
            Success = false,
            Message = message,
            ErrorProperties = errorProperties
        };
    }
}

/// <summary>
/// Generic base response DTO with typed data payload.
/// </summary>
/// <typeparam name="T">The type of the data payload.</typeparam>
public class BaseResponseDTO<T> : BaseResponseDTO
{
    /// <summary>
    /// The response data payload.
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// Creates a success response with data.
    /// </summary>
    public static BaseResponseDTO<T> SuccessResponse(T data, string? message = null)
    {
        return new BaseResponseDTO<T>
        {
            Success = true,
            Message = message ?? "Operation completed successfully.",
            Data = data
        };
    }

    /// <summary>
    /// Creates an error response.
    /// </summary>
    public new static BaseResponseDTO<T> ErrorResponse(string message, string[]? errorProperties = null)
    {
        return new BaseResponseDTO<T>
        {
            Success = false,
            Message = message,
            ErrorProperties = errorProperties
        };
    }
}
