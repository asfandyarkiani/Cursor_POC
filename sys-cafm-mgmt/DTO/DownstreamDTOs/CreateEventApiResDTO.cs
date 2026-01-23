namespace CAFMSystem.DTO.DownstreamDTOs
{
    public class CreateEventApiResDTO
    {
        public CreateEventEnvelopeDto? Envelope { get; set; }
    }

    public class CreateEventEnvelopeDto
    {
        public CreateEventBodyDto? Body { get; set; }
    }

    public class CreateEventBodyDto
    {
        public CreateEventResponseDto? CreateEventResponse { get; set; }
    }

    public class CreateEventResponseDto
    {
        public CreateEventResultDto? CreateEventResult { get; set; }
    }

    public class CreateEventResultDto
    {
        public int? EventId { get; set; }
        public int? PrimaryKeyId { get; set; }
        public string? EventNumber { get; set; }
    }
}
