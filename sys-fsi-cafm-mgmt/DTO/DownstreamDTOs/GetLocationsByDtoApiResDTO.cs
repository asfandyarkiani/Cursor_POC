namespace FsiCafmSystem.DTO.DownstreamDTOs
{
    public class GetLocationsByDtoApiResDTO
    {
        public List<LocationDtoItem>? LocationDto { get; set; }
    }
    
    public class LocationDtoItem
    {
        public string? BuildingId { get; set; }
        public string? LocationId { get; set; }
        public string? BarCode { get; set; }
        public string? Description { get; set; }
    }
}
