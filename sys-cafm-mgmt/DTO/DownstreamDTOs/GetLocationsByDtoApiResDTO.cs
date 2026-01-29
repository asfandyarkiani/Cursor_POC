namespace CAFMSystem.DTO.DownstreamDTOs
{
    /// <summary>
    /// Response DTO for CAFM GetLocationsByDto operation.
    /// Maps to SOAP response: Envelope/Body/GetLocationsByDtoResponse/GetLocationsByDtoResult/LocationDto
    /// </summary>
    public class GetLocationsByDtoApiResDTO
    {
        public string? BuildingId { get; set; }
        public string? LocationId { get; set; }
        public string? BarCode { get; set; }
        public string? LocationName { get; set; }
    }
}
