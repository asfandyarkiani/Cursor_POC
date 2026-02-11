using CAFMSystem.DTO.DownstreamDTOs;

namespace CAFMSystem.DTO.GetBreakdownTasksByDtoDTO
{
    public class GetBreakdownTasksByDtoResDTO
    {
        public string BreakdownTaskId { get; set; } = string.Empty;
        public string ServiceRequestNumber { get; set; } = string.Empty;
        public bool TaskExists { get; set; }

        public static GetBreakdownTasksByDtoResDTO Map(GetBreakdownTasksByDtoApiResDTO? apiResponse, string serviceRequestNumber)
        {
            bool taskExists = apiResponse != null && !string.IsNullOrWhiteSpace(apiResponse.PrimaryKeyId);
            
            return new GetBreakdownTasksByDtoResDTO
            {
                BreakdownTaskId = apiResponse?.PrimaryKeyId ?? string.Empty,
                ServiceRequestNumber = serviceRequestNumber ?? string.Empty,
                TaskExists = taskExists
            };
        }
    }
}
