using CAFMSystem.DTO.DownstreamDTOs;

namespace CAFMSystem.DTO.HandlerDTOs.CreateEventDTO
{
    public class CreateEventResDTO
    {
        public int EventId { get; set; }
        public int PrimaryKeyId { get; set; }
        public string EventNumber { get; set; } = string.Empty;

        public static CreateEventResDTO Map(CreateEventApiResDTO apiResponse)
        {
            CreateEventResultDto? result = apiResponse?.Envelope?.Body?.CreateEventResponse?.CreateEventResult;
            
            return new CreateEventResDTO
            {
                EventId = result?.EventId ?? 0,
                PrimaryKeyId = result?.PrimaryKeyId ?? 0,
                EventNumber = result?.EventNumber ?? string.Empty
            };
        }
    }
}
