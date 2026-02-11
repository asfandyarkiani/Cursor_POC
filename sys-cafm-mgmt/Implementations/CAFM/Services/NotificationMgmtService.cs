using Core.DTOs;
using Core.Extensions;
using Microsoft.Extensions.Logging;
using sys_cafm_mgmt.Abstractions;
using sys_cafm_mgmt.DTO.SendEmailDTO;
using sys_cafm_mgmt.Implementations.CAFM.Handlers;

namespace sys_cafm_mgmt.Implementations.CAFM.Services
{
    public class NotificationMgmtService : INotificationMgmt
    {
        private readonly ILogger<NotificationMgmtService> _logger;
        private readonly SendEmailHandler _sendEmailHandler;

        public NotificationMgmtService(
            ILogger<NotificationMgmtService> logger,
            SendEmailHandler sendEmailHandler)
        {
            _logger = logger;
            _sendEmailHandler = sendEmailHandler;
        }

        public async Task<BaseResponseDTO> SendEmail(SendEmailReqDTO request)
        {
            _logger.Info("NotificationMgmtService.SendEmail called");
            return await _sendEmailHandler.HandleAsync(request);
        }
    }
}
