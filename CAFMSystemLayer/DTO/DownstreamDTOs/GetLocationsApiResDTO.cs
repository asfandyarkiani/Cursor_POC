namespace CAFMSystemLayer.DTO.DownstreamDTOs
{
    public class GetLocationsApiResDTO
    {
        public string? LocationId { get; set; }
        public string? PropertyName { get; set; }
        public string? UnitCode { get; set; }
        public string? LocationName { get; set; }
        public List<LocationItemDTO>? Locations { get; set; }
    }
    
    public class LocationItemDTO
    {
        public string? LocationId { get; set; }
        public string? LocationName { get; set; }
        public string? PropertyName { get; set; }
        public string? UnitCode { get; set; }
    }
}
