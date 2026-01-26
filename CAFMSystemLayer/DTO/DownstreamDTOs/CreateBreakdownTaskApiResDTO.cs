namespace CAFMSystemLayer.DTO.DownstreamDTOs
{
    public class CreateBreakdownTaskApiResDTO
    {
        public string? TaskId { get; set; }
        public string? ServiceRequestNumber { get; set; }
        public string? CreateBreakdownTaskResult { get; set; }
        public bool? Success { get; set; }
    }
}
