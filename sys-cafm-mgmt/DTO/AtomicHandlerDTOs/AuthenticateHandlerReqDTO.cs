using Core.Exceptions;
using Core.SystemLayer.DTOs;

namespace sys_cafm_mgmt.DTO.AtomicHandlerDTOs
{
    public class AuthenticateHandlerReqDTO : IDownStreamRequestDTO
    {
        public string? Username { get; set; }
        public string? Password { get; set; }

        public void ValidateDownStreamRequestParameters()
        {
            // Username and Password will be retrieved from KeyVault in AtomicHandler
            // No validation needed here
        }
    }
}
