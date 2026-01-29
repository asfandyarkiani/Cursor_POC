using Core.DTOs;
using Core.Extensions;
using FsiCafmSystem.Abstractions;
using FsiCafmSystem.DTO.HandlerDTOs.SendEmailDTO;
using FsiCafmSystem.Implementations.FsiCafm.Handlers;
using Microsoft.Extensions.Logging;

namespace FsiCafmSystem.Implementations.FsiCafm.Services
{
    public class EmailMgmtService : IEmailMgmt
    {
        private readonly ILogger<EmailMgmtService> _logger;
        private readonly SendEmailHandler _sendEmailHandler;
        
        public EmailMgmtService(
            ILogger<EmailMgmtService> logger,
            SendEmailHandler sendEmailHandler)
        {
            _logger = logger;
            _sendEmailHandler = sendEmailHandler;
        }
        
        public async Task<BaseResponseDTO> SendEmail(SendEmailReqDTO request)
        {
            _logger.Info("EmailMgmtService.SendEmail called");
            return await _sendEmailHandler.HandleAsync(request);
        }
    }
}
