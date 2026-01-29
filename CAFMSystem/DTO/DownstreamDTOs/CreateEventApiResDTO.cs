namespace CAFMSystem.DTO.DownstreamDTOs
{
    /// <summary>
    /// Response DTO for CAFM CreateEvent operation (downstream SOAP API).
    /// Deserializes SOAP response from FSI CreateEvent API.
    /// </summary>
    public class CreateEventApiResDTO
    {
        public string? EventId { get; set; }
        public string? Status { get; set; }
    }
}
