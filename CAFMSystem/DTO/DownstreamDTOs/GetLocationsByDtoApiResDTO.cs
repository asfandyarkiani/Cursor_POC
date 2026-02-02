namespace CAFMSystem.DTO.DownstreamDTOs
{
    /// <summary>
    /// Response DTO for CAFM GetLocationsByDto operation (downstream SOAP API).
    /// Deserializes SOAP response from FSI GetLocationsByDto API.
    /// </summary>
    public class GetLocationsByDtoApiResDTO
    {
        public string? LocationId { get; set; }
        public string? BuildingId { get; set; }
        public string? PropertyName { get; set; }
        public string? UnitCode { get; set; }
    }
}
