using CAFMSystem.DTO.DownstreamDTOs;

namespace CAFMSystem.DTO.HandlerDTOs.GetBreakdownTasksByDtoDTO
{
    public class GetBreakdownTasksByDtoResDTO
    {
        public string CallId { get; set; } = string.Empty;
        public int TaskId { get; set; }
        public int PrimaryKeyId { get; set; }
        public string TaskNumber { get; set; } = string.Empty;
        public string LongDescription { get; set; } = string.Empty;
        public List<BreakdownTaskDtoV3Item> Tasks { get; set; } = new List<BreakdownTaskDtoV3Item>();

        public static GetBreakdownTasksByDtoResDTO Map(GetBreakdownTasksByDtoApiResDTO apiResponse)
        {
            List<BreakdownTaskDtoV3Item>? tasks = apiResponse?.Envelope?.Body?.GetBreakdownTasksByDtoResponse?.GetBreakdownTasksByDtoResult?.BreakdownTaskDtoV3;
            BreakdownTaskDtoV3Item? firstTask = tasks?.FirstOrDefault();
            
            return new GetBreakdownTasksByDtoResDTO
            {
                CallId = firstTask?.CallId ?? string.Empty,
                TaskId = firstTask?.TaskId ?? 0,
                PrimaryKeyId = firstTask?.PrimaryKeyId ?? 0,
                TaskNumber = firstTask?.TaskNumber ?? string.Empty,
                LongDescription = firstTask?.LongDescription ?? string.Empty,
                Tasks = tasks ?? new List<BreakdownTaskDtoV3Item>()
            };
        }
    }
}
