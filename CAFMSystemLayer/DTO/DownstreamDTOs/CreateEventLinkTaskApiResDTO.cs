namespace CAFMSystemLayer.DTO.DownstreamDTOs
{
    public class CreateEventLinkTaskApiResDTO
    {
        public string? EventId { get; set; }
        public string? TaskId { get; set; }
        public string? CreateEventResult { get; set; }
        public bool? Success { get; set; }
    }
}
