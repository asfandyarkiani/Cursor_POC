using System.Collections.Generic;

namespace CAFMSystem.DTO.DownstreamDTOs
{
    /// <summary>
    /// Response DTO for CAFM GetBreakdownTasksByDto operation (downstream SOAP API).
    /// Deserializes SOAP response from FSI GetBreakdownTasksByDto API.
    /// </summary>
    public class GetBreakdownTasksByDtoApiResDTO
    {
        public List<BreakdownTaskApiInfo>? Tasks { get; set; }
    }

    public class BreakdownTaskApiInfo
    {
        public string? TaskId { get; set; }
        public string? CallId { get; set; }
        public string? Status { get; set; }
    }
}
