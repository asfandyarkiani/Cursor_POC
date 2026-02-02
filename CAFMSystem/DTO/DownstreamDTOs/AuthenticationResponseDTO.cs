namespace CAFMSystem.DTO.DownstreamDTOs
{
    /// <summary>
    /// Response DTO for CAFM Authenticate operation (downstream SOAP API).
    /// Deserializes SOAP response from FSI Login API.
    /// </summary>
    public class AuthenticationResponseDTO
    {
        public string? SessionId { get; set; }
    }
}
