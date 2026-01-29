namespace SysCafmMgmt.DTOs.Downstream
{
    /// <summary>
    /// Response DTO for FSI GetInstructionSetsByDto SOAP response
    /// </summary>
    public class GetInstructionSetsByDtoResponseDto
    {
        public List<InstructionSetDto>? InstructionSets { get; set; }
        public bool IsSuccess => InstructionSets != null && InstructionSets.Count > 0;
    }

    public class InstructionSetDto
    {
        public string? InstructionId { get; set; }
        public string? Code { get; set; }
        public string? Description { get; set; }
        public string? CategoryId { get; set; }
        public string? DisciplineId { get; set; }
        public string? PriorityId { get; set; }
    }
}
