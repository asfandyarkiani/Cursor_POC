using AGI.ApiEcoSys.Core.DTOs;

namespace AGI.SysD365DriverLateLoginMgmt.DTO.SubmitDriverLateLoginDTO;

/// <summary>
/// Response DTO for submitting driver late login request to D365
/// Returned to Process Layer
/// </summary>
public class SubmitDriverLateLoginResDTO : BaseResponseDTO
{
    /// <summary>
    /// Success status from D365 API
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Message from D365 API
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Reference ID from D365 (if available)
    /// </summary>
    public string? Reference { get; set; }

    /// <summary>
    /// Input reference from D365 (if available)
    /// </summary>
    public string? InputReference { get; set; }

    /// <summary>
    /// Creates a success response
    /// </summary>
    /// <param name="message">Success message</param>
    /// <param name="reference">Reference ID</param>
    /// <param name="inputReference">Input reference</param>
    /// <returns>Success response DTO</returns>
    public static SubmitDriverLateLoginResDTO CreateSuccessResponse(string message, string? reference = null, string? inputReference = null)
    {
        return new SubmitDriverLateLoginResDTO
        {
            Success = true,
            Message = message,
            Reference = reference,
            InputReference = inputReference
        };
    }

    /// <summary>
    /// Creates an error response
    /// </summary>
    /// <param name="message">Error message</param>
    /// <returns>Error response DTO</returns>
    public static SubmitDriverLateLoginResDTO CreateErrorResponse(string message)
    {
        return new SubmitDriverLateLoginResDTO
        {
            Success = false,
            Message = message
        };
    }
}
