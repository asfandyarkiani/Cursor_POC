namespace CAFMSystem.DTO.DownstreamDTOs
{
    public class AuthenticateCAFMApiResDTO
    {
        public EnvelopeDto? Envelope { get; set; }
    }

    public class EnvelopeDto
    {
        public BodyDto? Body { get; set; }
    }

    public class BodyDto
    {
        public AuthenticateResponseDto? AuthenticateResponse { get; set; }
    }

    public class AuthenticateResponseDto
    {
        public AuthenticateResultDto? AuthenticateResult { get; set; }
    }

    public class AuthenticateResultDto
    {
        public string? SessionId { get; set; }
        public string? OperationResult { get; set; }
        public string? EvolutionVersion { get; set; }
    }
}
