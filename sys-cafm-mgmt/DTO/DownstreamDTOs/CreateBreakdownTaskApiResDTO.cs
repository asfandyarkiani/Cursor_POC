namespace CAFMSystem.DTO.DownstreamDTOs
{
    /// <summary>
    /// Response DTO for CAFM CreateBreakdownTask operation.
    /// Maps to SOAP response: Envelope/Body/CreateBreakdownTaskResponse/CreateBreakdownTaskResult
    /// </summary>
    public class CreateBreakdownTaskApiResDTO
    {
        public string? TaskId { get; set; }
        public string? CallId { get; set; }
        public string? OperationResult { get; set; }
    }
}
