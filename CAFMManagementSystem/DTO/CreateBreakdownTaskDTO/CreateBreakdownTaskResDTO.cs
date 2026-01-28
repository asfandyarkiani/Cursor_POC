using CAFMManagementSystem.DTO.DownstreamDTOs;

namespace CAFMManagementSystem.DTO.CreateBreakdownTaskDTO
{
    public class CreateBreakdownTaskResDTO
    {
        public string BreakdownTaskId { get; set; } = string.Empty;
        public string CallId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        
        public static CreateBreakdownTaskResDTO Map(CreateBreakdownTaskApiResDTO apiResponse)
        {
            return new CreateBreakdownTaskResDTO
            {
                BreakdownTaskId = apiResponse.BreakdownTaskId ?? string.Empty,
                CallId = apiResponse.CallId ?? string.Empty,
                Status = "Success",
                Message = "Breakdown task created successfully"
            };
        }
    }
}
