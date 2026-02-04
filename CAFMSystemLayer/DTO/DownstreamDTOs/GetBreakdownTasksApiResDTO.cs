namespace CAFMSystemLayer.DTO.DownstreamDTOs
{
    public class GetBreakdownTasksApiResDTO
    {
        public string? TaskId { get; set; }
        public string? ServiceRequestNumber { get; set; }
        public string? TaskStatus { get; set; }
        public List<BreakdownTaskItemDTO>? Tasks { get; set; }
    }
    
    public class BreakdownTaskItemDTO
    {
        public string? TaskId { get; set; }
        public string? ServiceRequestNumber { get; set; }
        public string? TaskStatus { get; set; }
        public string? Description { get; set; }
    }
}
