using OracleFusionHCM.LeaveManagement.DTO.DownstreamDTOs;

namespace OracleFusionHCM.LeaveManagement.DTO.CreateLeaveDTO
{
    /// <summary>
    /// Response DTO for Create Leave API (to D365)
    /// Maps to Leave D365 Response (f4ca3a70-114a-4601-bad8-44a3eb20e2c0)
    /// </summary>
    public class CreateLeaveResDTO
    {
        public string Status { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public long? PersonAbsenceEntryId { get; set; }
        public string Success { get; set; } = string.Empty;
        
        /// <summary>
        /// Maps Oracle Fusion API response to D365 response format
        /// Based on map_e4fd3f59 (Oracle Fusion Leave Response Map)
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
    }
}
