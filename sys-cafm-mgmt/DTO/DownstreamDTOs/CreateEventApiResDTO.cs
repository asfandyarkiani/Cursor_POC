namespace CAFMSystem.DTO.DownstreamDTOs
{
    /// <summary>
    /// Response DTO for CAFM CreateEvent operation.
    /// Maps to SOAP response: Envelope/Body/CreateEventResponse/CreateEventResult
    /// </summary>
    public class CreateEventApiResDTO
    {
        public string? EventId { get; set; }
        public string? OperationResult { get; set; }
    }
}
