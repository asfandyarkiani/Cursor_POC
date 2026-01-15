using System.Text.Json.Serialization;

namespace FacilitiesMgmtSystem.DTO.NetworkTestDTO;

/// <summary>
/// Response DTO for Network Test API.
/// </summary>
public class NetworkTestResponseDTO
{
    /// <summary>
    /// The test result message.
    /// </summary>
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp of the test execution.
    /// </summary>
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// The correlation ID if provided in the request.
    /// </summary>
    [JsonPropertyName("correlationId")]
    public string? CorrelationId { get; set; }
}
