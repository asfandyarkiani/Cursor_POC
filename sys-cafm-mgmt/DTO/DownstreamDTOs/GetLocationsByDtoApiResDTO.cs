namespace CAFMSystem.DTO.DownstreamDTOs
{
    public class GetLocationsByDtoApiResDTO
    {
        public LocationEnvelopeDto? Envelope { get; set; }
    }

    public class LocationEnvelopeDto
    {
        public LocationBodyDto? Body { get; set; }
    }

    public class LocationBodyDto
    {
        public GetLocationsByDtoResponseDto? GetLocationsByDtoResponse { get; set; }
    }

    public class GetLocationsByDtoResponseDto
    {
        public GetLocationsByDtoResultDto? GetLocationsByDtoResult { get; set; }
    }

    public class GetLocationsByDtoResultDto
    {
        public List<LocationDtoItem>? LocationDto { get; set; }
    }

    public class LocationDtoItem
    {
        public int? BuildingId { get; set; }
        public int? LocationId { get; set; }
        public int? PrimaryKeyId { get; set; }
        public string? BarCode { get; set; }
        public string? LocationName { get; set; }
        public int? AreaId { get; set; }
        public string? AreaName { get; set; }
    }
}
