using System.Text.Json.Serialization;

namespace SysCafmMgmt.DTOs.API;

/// <summary>
/// Response DTO for Create Work Order API endpoint
/// </summary>
public class CreateWorkOrderResponseDTO
{
    [JsonPropertyName("workOrderId")]
    public string? WorkOrderId { get; set; }

    [JsonPropertyName("taskNumber")]
    public string? TaskNumber { get; set; }

    [JsonPropertyName("sourceServiceRequestNumber")]
    public string? SourceServiceRequestNumber { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("eventId")]
    public string? EventId { get; set; }

    [JsonPropertyName("isLinked")]
    public bool IsLinked { get; set; }
}
