using Core.DTOs;
using FsiCafmSystem.DTO.HandlerDTOs.SendEmailDTO;

namespace FsiCafmSystem.Abstractions
{
    /// <summary>
    /// Email management interface for SMTP operations.
    /// </summary>
    public interface IEmailMgmt
    {
        /// <summary>
        /// Sends email with optional attachment.
        /// </summary>
        /// <param name="request">Email send request</param>
        /// <returns>Email send response</returns>
        Task<BaseResponseDTO> SendEmail(SendEmailReqDTO request);
    }
}
