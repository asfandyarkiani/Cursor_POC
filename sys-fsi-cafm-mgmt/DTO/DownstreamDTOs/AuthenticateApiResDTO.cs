namespace FsiCafmSystem.DTO.DownstreamDTOs
{
    public class AuthenticateApiResDTO
    {
        public string? SessionId { get; set; }
        public string? FilterQueryId { get; set; }
        public string? PrimaryKeyId { get; set; }
        public string? PrimaryTableCatalogId { get; set; }
        public string? ValidateOnly { get; set; }
        public string? EvolutionVersion { get; set; }
        public string? OperationResult { get; set; }
    }
}
