using System.Text.Json.Serialization;

namespace AGI.SysD365DriverLateLoginMgmt.DTO.DownstreamDTOs;

/// <summary>
/// Response DTO from D365 late login API
/// Represents the actual response structure from D365 F&O
/// </summary>
public class SubmitLateLoginApiResDTO
{
    /// <summary>
    /// Unique identifier for the response
    /// </summary>
    [JsonPropertyName("$id")]
    public string? Id { get; set; }

    /// <summary>
    /// Array of messages from D365 API
    /// </summary>
    [JsonPropertyName("Messages")]
    public List<D365MessageDTO> Messages { get; set; } = new List<D365MessageDTO>();
}

/// <summary>
/// Message DTO from D365 API response
/// </summary>
public class D365MessageDTO
{
    /// <summary>
    /// Unique identifier for the message
    /// </summary>
    [JsonPropertyName("$id")]
    public string? Id { get; set; }

    /// <summary>
    /// Success status ("true" or "false" as string)
    /// </summary>
    [JsonPropertyName("success")]
    public string Success { get; set; } = string.Empty;

    /// <summary>
    /// Message text
    /// </summary>
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Reference ID
    /// </summary>
    [JsonPropertyName("reference")]
    public string? Reference { get; set; }

    /// <summary>
    /// Input reference
    /// </summary>
    [JsonPropertyName("InputReference")]
    public string? InputReference { get; set; }
}
