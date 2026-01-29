using CAFMSystem.DTO.DownstreamDTOs;

namespace CAFMSystem.DTO.CreateEventDTO
{
    /// <summary>
    /// Response DTO for CreateEvent API.
    /// Returns created event ID and status.
    /// </summary>
    public class CreateEventResDTO
    {
        public string EventId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string TaskId { get; set; } = string.Empty;

        public static CreateEventResDTO Map(CreateEventApiResDTO apiResponse, string taskId)
        {
            return new CreateEventResDTO
            {
                EventId = apiResponse?.EventId ?? string.Empty,
                Status = apiResponse?.Status ?? string.Empty,
                TaskId = taskId
            };
        }
    }
}
