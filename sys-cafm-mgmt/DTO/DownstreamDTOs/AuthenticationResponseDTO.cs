namespace CAFMSystem.DTO.DownstreamDTOs
{
    /// <summary>
    /// Response DTO for CAFM Authenticate operation.
    /// Maps to SOAP response: Envelope/Body/AuthenticateResponse/AuthenticateResult
    /// </summary>
    public class AuthenticationResponseDTO
    {
        public string? SessionId { get; set; }
        public string? OperationResult { get; set; }
        public string? EvolutionVersion { get; set; }
    }
}
