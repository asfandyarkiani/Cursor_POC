namespace SysCafmMgmt.DTOs.Downstream
{
    /// <summary>
    /// Response DTO for FSI Authenticate SOAP response
    /// Based on Boomi profile: Login_Response
    /// </summary>
    public class AuthenticateResponseDto
    {
        public string? SessionId { get; set; }
        public string? OperationResult { get; set; }
        public string? EvolutionVersion { get; set; }
        public bool IsSuccess => !string.IsNullOrWhiteSpace(SessionId);
    }
}
