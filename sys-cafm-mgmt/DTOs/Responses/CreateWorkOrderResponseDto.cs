using System.Text.Json.Serialization;

namespace SysCafmMgmt.DTOs.Responses
{
    /// <summary>
    /// Response DTO for work order creation result
    /// Based on Boomi profile: WorkOrder_Create_Success and WorkOrder_Create_Error
    /// </summary>
    public class CreateWorkOrderResponseDto
    {
        [JsonPropertyName("workOrder")]
        public List<WorkOrderResponseItem>? WorkOrder { get; set; }
    }

    public class WorkOrderResponseItem
    {
        [JsonPropertyName("cafmSRNumber")]
        public string? CafmSrNumber { get; set; }

        [JsonPropertyName("sourceSRNumber")]
        public string? SourceSrNumber { get; set; }

        [JsonPropertyName("sourceOrgId")]
        public string? SourceOrgId { get; set; }

        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }
    }
}
