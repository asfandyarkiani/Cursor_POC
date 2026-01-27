namespace sys_cafm_mgmt.DTO.DownstreamDTOs
{
    public class GetLocationsByDtoApiResDTO
    {
        public LocationEnvelopeDTO? Envelope { get; set; }
    }

    public class LocationEnvelopeDTO
    {
        public LocationBodyDTO? Body { get; set; }
    }

    public class LocationBodyDTO
    {
        public GetLocationsByDtoResponseDTO? GetLocationsByDtoResponse { get; set; }
    }

    public class GetLocationsByDtoResponseDTO
    {
        public GetLocationsByDtoResultDTO? GetLocationsByDtoResult { get; set; }
    }

    public class GetLocationsByDtoResultDTO
    {
        public List<LocationItemDTO>? Locations { get; set; }
    }

    public class LocationItemDTO
    {
        public int LocationId { get; set; }
        public string? LocationName { get; set; }
        public string? LocationCode { get; set; }
        public int BuildingId { get; set; }
        public string? BuildingName { get; set; }
    }
}
