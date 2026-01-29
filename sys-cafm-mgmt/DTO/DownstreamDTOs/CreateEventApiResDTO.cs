namespace sys_cafm_mgmt.DTO.DownstreamDTOs
{
    public class CreateEventApiResDTO
    {
        public CreateEventEnvelopeDTO? Envelope { get; set; }
    }

    public class CreateEventEnvelopeDTO
    {
        public CreateEventBodyDTO? Body { get; set; }
    }

    public class CreateEventBodyDTO
    {
        public CreateEventResponseDTO? CreateEventResponse { get; set; }
    }

    public class CreateEventResponseDTO
    {
        public CreateEventResultDTO? CreateEventResult { get; set; }
    }

    public class CreateEventResultDTO
    {
        public int PrimaryKeyId { get; set; }
        public int BreakdownTaskId { get; set; }
        public string? EventDescription { get; set; }
    }
}
