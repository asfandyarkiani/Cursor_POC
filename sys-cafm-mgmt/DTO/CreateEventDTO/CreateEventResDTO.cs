using CAFMSystem.DTO.DownstreamDTOs;

namespace CAFMSystem.DTO.CreateEventDTO
{
    public class CreateEventResDTO
    {
        public string EventId { get; set; } = string.Empty;
        public string BreakdownTaskId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;

        public static CreateEventResDTO Map(CreateEventApiResDTO? apiResponse, string breakdownTaskId)
        {
            return new CreateEventResDTO
            {
                EventId = apiResponse?.PrimaryKeyId ?? string.Empty,
                BreakdownTaskId = breakdownTaskId ?? string.Empty,
                Status = "Success",
                Message = "Event created successfully"
            };
        }
    }
}
