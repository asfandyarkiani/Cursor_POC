using CAFMSystem.DTO.DownstreamDTOs;

namespace CAFMSystem.DTO.HandlerDTOs.GetLocationsByDtoDTO
{
    public class GetLocationsByDtoResDTO
    {
        public int BuildingId { get; set; }
        public int LocationId { get; set; }
        public int PrimaryKeyId { get; set; }
        public string BarCode { get; set; } = string.Empty;
        public string LocationName { get; set; } = string.Empty;
        public int? AreaId { get; set; }
        public string AreaName { get; set; } = string.Empty;

        public static GetLocationsByDtoResDTO Map(GetLocationsByDtoApiResDTO apiResponse)
        {
            LocationDtoItem? location = apiResponse?.Envelope?.Body?.GetLocationsByDtoResponse?.GetLocationsByDtoResult?.LocationDto?.FirstOrDefault();
            
            return new GetLocationsByDtoResDTO
            {
                BuildingId = location?.BuildingId ?? 0,
                LocationId = location?.LocationId ?? 0,
                PrimaryKeyId = location?.PrimaryKeyId ?? 0,
                BarCode = location?.BarCode ?? string.Empty,
                LocationName = location?.LocationName ?? string.Empty,
                AreaId = location?.AreaId,
                AreaName = location?.AreaName ?? string.Empty
            };
        }
    }
}
