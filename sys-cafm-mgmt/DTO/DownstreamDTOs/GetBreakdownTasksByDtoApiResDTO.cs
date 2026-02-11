namespace CAFMSystem.DTO.DownstreamDTOs
{
    /// <summary>
    /// Response DTO for CAFM GetBreakdownTasksByDto operation.
    /// Maps to SOAP response: Envelope/Body/GetBreakdownTasksByDtoResponse/GetBreakdownTasksByDtoResult/BreakdownTaskDtoV3
    /// </summary>
    public class GetBreakdownTasksByDtoApiResDTO
    {
        public string? CallId { get; set; }
        public string? TaskId { get; set; }
        public string? Status { get; set; }
    }
}
