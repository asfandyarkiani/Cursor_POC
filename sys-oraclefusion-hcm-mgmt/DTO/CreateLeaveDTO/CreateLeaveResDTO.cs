using OracleFusionHcmMgmt.DTO.DownstreamDTOs;
using System.Text.Json.Serialization;

namespace OracleFusionHcmMgmt.DTO.CreateLeaveDTO
{
    /// <summary>
    /// Response DTO for Create Leave API.
    /// Returns leave creation result to Process Layer (D365 format).
    /// </summary>
    public class CreateLeaveResDTO
    {
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;
        
        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;
        
        [JsonPropertyName("personAbsenceEntryId")]
        public long? PersonAbsenceEntryId { get; set; }
        
        [JsonPropertyName("success")]
        public string Success { get; set; } = string.Empty;
        
        /// <summary>
        /// Maps Oracle Fusion HCM API response to D365 response format.
        /// </summary>
        public static CreateLeaveResDTO Map(CreateLeaveApiResDTO apiResponse)
        {
            return new CreateLeaveResDTO
            {
                Status = "success",
                Message = "Data successfully sent to Oracle Fusion",
                PersonAbsenceEntryId = apiResponse.PersonAbsenceEntryId,
                Success = "true"
            };
        }
        
        /// <summary>
        /// Creates error response with message.
        /// </summary>
        public static CreateLeaveResDTO MapError(string errorMessage)
        {
            return new CreateLeaveResDTO
            {
                Status = "failure",
                Message = errorMessage,
                PersonAbsenceEntryId = null,
                Success = "false"
            };
        }
    }
}
