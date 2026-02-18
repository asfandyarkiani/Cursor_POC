using System.Text.Json.Serialization;

namespace HcmLeaveProcessLayer.DTOs.CreateLeave
{
    public class CreateLeaveResDTO
    {
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("personAbsenceEntryId")]
        public long PersonAbsenceEntryId { get; set; }

        [JsonPropertyName("success")]
        public string Success { get; set; } = string.Empty;
    }
}
