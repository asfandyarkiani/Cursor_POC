using CAFMManagementSystem.DTO.DownstreamDTOs;

namespace CAFMManagementSystem.DTO.GetBreakdownTasksByDtoDTO
{
    public class GetBreakdownTasksByDtoResDTO
    {
        public string CallId { get; set; } = string.Empty;
        public string BreakdownTaskId { get; set; } = string.Empty;
        public bool Exists { get; set; }
        
        public static GetBreakdownTasksByDtoResDTO Map(GetBreakdownTasksByDtoApiResDTO apiResponse)
        {
            return new GetBreakdownTasksByDtoResDTO
            {
                CallId = apiResponse.CallId ?? string.Empty,
                BreakdownTaskId = apiResponse.BreakdownTaskId ?? string.Empty,
                Exists = !string.IsNullOrEmpty(apiResponse.CallId)
            };
        }
    }
}
