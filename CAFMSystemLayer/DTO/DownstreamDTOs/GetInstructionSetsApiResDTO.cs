namespace CAFMSystemLayer.DTO.DownstreamDTOs
{
    public class GetInstructionSetsApiResDTO
    {
        public string? InstructionSetId { get; set; }
        public string? CategoryName { get; set; }
        public string? SubCategory { get; set; }
        public string? InstructionSetName { get; set; }
        public List<InstructionSetItemDTO>? InstructionSets { get; set; }
    }
    
    public class InstructionSetItemDTO
    {
        public string? InstructionSetId { get; set; }
        public string? InstructionSetName { get; set; }
        public string? CategoryName { get; set; }
        public string? SubCategory { get; set; }
    }
}
