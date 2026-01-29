namespace SysCafmMgmt.DTOs.Downstream
{
    /// <summary>
    /// Response DTO for FSI GetLocationsByDto SOAP response
    /// </summary>
    public class GetLocationsByDtoResponseDto
    {
        public List<LocationDto>? Locations { get; set; }
        public bool IsSuccess => Locations != null && Locations.Count > 0;
    }

    public class LocationDto
    {
        public string? LocationId { get; set; }
        public string? Code { get; set; }
        public string? Description { get; set; }
        public string? BuildingId { get; set; }
        public string? BuildingName { get; set; }
    }
}
