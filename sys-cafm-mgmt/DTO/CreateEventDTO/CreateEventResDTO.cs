using sys_cafm_mgmt.DTO.DownstreamDTOs;

namespace sys_cafm_mgmt.DTO.CreateEventDTO
{
    public class CreateEventResDTO
    {
        public int EventId { get; set; }
        public int BreakdownTaskId { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;

        public static CreateEventResDTO Map(CreateEventApiResDTO apiResponse)
        {
            return new CreateEventResDTO
            {
                EventId = apiResponse?.Envelope?.Body?.CreateEventResponse?.CreateEventResult?.PrimaryKeyId ?? 0,
                BreakdownTaskId = apiResponse?.Envelope?.Body?.CreateEventResponse?.CreateEventResult?.BreakdownTaskId ?? 0,
                Status = "Created",
                Message = "Event created successfully"
            };
        }
    }
}
