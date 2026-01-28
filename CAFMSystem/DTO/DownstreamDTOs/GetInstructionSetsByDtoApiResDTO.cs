namespace CAFMSystem.DTO.DownstreamDTOs
{
    /// <summary>
    /// Response DTO for CAFM GetInstructionSetsByDto operation (downstream SOAP API).
    /// Deserializes SOAP response from FSI GetInstructionSetsByDto API.
    /// </summary>
    public class GetInstructionSetsByDtoApiResDTO
    {
        public string? InstructionId { get; set; }
        public string? CategoryName { get; set; }
        public string? SubCategory { get; set; }
    }
}
