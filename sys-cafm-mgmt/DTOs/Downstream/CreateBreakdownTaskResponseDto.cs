namespace SysCafmMgmt.DTOs.Downstream
{
    /// <summary>
    /// Response DTO for FSI CreateBreakdownTask SOAP response
    /// Based on Boomi profile: CreateBreakdownTask response
    /// </summary>
    public class CreateBreakdownTaskResponseDto
    {
        public string? TaskId { get; set; }
        public string? OperationResult { get; set; }
        public bool IsSuccess => !string.IsNullOrWhiteSpace(TaskId);
    }
}
