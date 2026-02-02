namespace CAFMSystem.DTO.DownstreamDTOs
{
    /// <summary>
    /// Response DTO for CAFM CreateBreakdownTask operation (downstream SOAP API).
    /// Deserializes SOAP response from FSI CreateBreakdownTask API.
    /// </summary>
    public class CreateBreakdownTaskApiResDTO
    {
        public string? TaskId { get; set; }
        public string? Status { get; set; }
    }
}
