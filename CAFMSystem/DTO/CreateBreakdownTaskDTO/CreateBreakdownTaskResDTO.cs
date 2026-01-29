using CAFMSystem.DTO.DownstreamDTOs;

namespace CAFMSystem.DTO.CreateBreakdownTaskDTO
{
    /// <summary>
    /// Response DTO for CreateBreakdownTask API.
    /// Returns created task ID and status.
    /// </summary>
    public class CreateBreakdownTaskResDTO
    {
        public string TaskId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string ServiceRequestNumber { get; set; } = string.Empty;
        public string SourceOrgId { get; set; } = string.Empty;

        public static CreateBreakdownTaskResDTO Map(CreateBreakdownTaskApiResDTO apiResponse, string serviceRequestNumber, string sourceOrgId)
        {
            return new CreateBreakdownTaskResDTO
            {
                TaskId = apiResponse?.TaskId ?? string.Empty,
                Status = apiResponse?.Status ?? string.Empty,
                ServiceRequestNumber = serviceRequestNumber,
                SourceOrgId = sourceOrgId
            };
        }
    }
}
