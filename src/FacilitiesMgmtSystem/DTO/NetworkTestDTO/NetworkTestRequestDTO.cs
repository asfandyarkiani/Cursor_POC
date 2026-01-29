using System.Text.Json.Serialization;

namespace FacilitiesMgmtSystem.DTO.NetworkTestDTO;

/// <summary>
/// Request DTO for Network Test API.
/// Note: The Network Test operation expects no input (inputType: "none"),
/// but this DTO is provided for extensibility and consistency with the architecture.
/// </summary>
public class NetworkTestRequestDTO
{
    /// <summary>
    /// Optional correlation ID for tracing.
    /// </summary>
    [JsonPropertyName("correlationId")]
    public string? CorrelationId { get; set; }
}
