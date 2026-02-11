namespace sys_cafm_mgmt.DTO.DownstreamDTOs
{
    public class AuthenticateApiResDTO
    {
        public EnvelopeDTO? Envelope { get; set; }
    }

    public class EnvelopeDTO
    {
        public BodyDTO? Body { get; set; }
    }

    public class BodyDTO
    {
        public AuthenticateResponseDTO? AuthenticateResponse { get; set; }
    }

    public class AuthenticateResponseDTO
    {
        public AuthenticateResultDTO? AuthenticateResult { get; set; }
    }

    public class AuthenticateResultDTO
    {
        public string? SessionId { get; set; }
        public string? UserId { get; set; }
        public string? UserName { get; set; }
    }
}
