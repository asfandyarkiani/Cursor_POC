namespace FsiCafmSystem.DTO.DownstreamDTOs
{
    public class GetBreakdownTasksByDtoApiResDTO
    {
        public List<BreakdownTaskDtoItem>? BreakdownTaskDtoV3 { get; set; }
    }
    
    public class BreakdownTaskDtoItem
    {
        public string? CallId { get; set; }
        public string? TaskId { get; set; }
        public string? Description { get; set; }
        public string? Status { get; set; }
    }
}
