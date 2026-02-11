using Core.DTOs;

namespace AGI.SysD365DriverLateLoginMgmt.DTO.SubmitDriverLateLoginDTO;

/// <summary>
/// Response DTO for submitting driver late login request to D365
/// Returned to Process Layer
/// </summary>
public class SubmitDriverLateLoginResDTO
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
    /// Maps D365 API response to System Layer response
    /// </summary>
    /// <param name="message">Message from D365</param>
    /// <param name="reference">Reference ID</param>
    /// <param name="inputReference">Input reference</param>
    /// <returns>Response DTO</returns>
    public static SubmitDriverLateLoginResDTO Map(string message, string? reference = null, string? inputReference = null)
    {
        return new SubmitDriverLateLoginResDTO
        {
            Success = true,
            Message = message,
            Reference = reference,
            InputReference = inputReference
        };
    }
}
