using System.Text.Json.Serialization;

namespace FacilitiesMgmtSystem.DTO;

/// <summary>
/// Base response DTO for all API responses.
/// </summary>
public class BaseResponseDTO
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("data")]
    public object? Data { get; set; }

    [JsonPropertyName("errors")]
    public string[]? Errors { get; set; }

    public static BaseResponseDTO CreateSuccess(string? message = null, object? data = null)
    {
        return new BaseResponseDTO
        {
            Success = true,
            Message = message,
            Data = data
        };
    }

    public static BaseResponseDTO CreateFailure(string? message = null, string[]? errors = null)
    {
        return new BaseResponseDTO
        {
            Success = false,
            Message = message,
            Errors = errors
        };
    }
}
