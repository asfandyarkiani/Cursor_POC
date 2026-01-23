using CAFMSystem.DTO.DownstreamDTOs;

namespace CAFMSystem.DTO.HandlerDTOs.CreateBreakdownTaskDTO
{
    public class CreateBreakdownTaskResDTO
    {
        public int TaskId { get; set; }
        public int PrimaryKeyId { get; set; }
        public int FilterQueryId { get; set; }
        public string TaskNumber { get; set; } = string.Empty;
        public string CallId { get; set; } = string.Empty;

        public static CreateBreakdownTaskResDTO Map(CreateBreakdownTaskApiResDTO apiResponse)
        {
            CreateBreakdownTaskResultDto? result = apiResponse?.Envelope?.Body?.CreateBreakdownTaskResponse?.CreateBreakdownTaskResult;
            
            return new CreateBreakdownTaskResDTO
            {
                TaskId = result?.TaskId ?? 0,
                PrimaryKeyId = result?.PrimaryKeyId ?? 0,
                FilterQueryId = result?.FilterQueryId ?? 0,
                TaskNumber = result?.TaskNumber ?? string.Empty,
                CallId = result?.CallId ?? string.Empty
            };
        }
    }
}
