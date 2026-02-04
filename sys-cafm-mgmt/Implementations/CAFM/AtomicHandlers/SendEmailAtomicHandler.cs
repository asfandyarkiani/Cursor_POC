using Core.Extensions;
using Core.SystemLayer.DTOs;
using Core.SystemLayer.Handlers;
using Core.SystemLayer.Middlewares;
using Microsoft.Extensions.Logging;
using sys_cafm_mgmt.Constants;
using sys_cafm_mgmt.DTO.AtomicHandlerDTOs;
using sys_cafm_mgmt.Helper;

namespace sys_cafm_mgmt.Implementations.CAFM.AtomicHandlers
{
    public class SendEmailAtomicHandler : IAtomicHandler<HttpResponseSnapshot>
    {
        private readonly ILogger<SendEmailAtomicHandler> _logger;
        private readonly CustomSmtpClient _customSmtpClient;

        public SendEmailAtomicHandler(
            ILogger<SendEmailAtomicHandler> logger,
            CustomSmtpClient customSmtpClient)
        {
            _logger = logger;
            _customSmtpClient = customSmtpClient;
        }

        public async Task<HttpResponseSnapshot> Handle(IDownStreamRequestDTO downStreamRequestDTO)
        {
            SendEmailHandlerReqDTO requestDTO = downStreamRequestDTO as SendEmailHandlerReqDTO 
                ?? throw new ArgumentException("Invalid DTO type");
            
            requestDTO.ValidateDownStreamRequestParameters();
            
            _logger.Info($"SendEmailAtomicHandler: Sending email to {requestDTO.To}");
            
            bool emailSent = await _customSmtpClient.SendEmailAsync(
                operationName: OperationNames.SEND_EMAIL,
                to: requestDTO.To,
                subject: requestDTO.Subject,
                body: requestDTO.Body,
                attachmentFileName: requestDTO.AttachmentFileName,
                attachmentContent: requestDTO.AttachmentContent);
            
            HttpResponseSnapshot response = new HttpResponseSnapshot
            {
                StatusCode = emailSent ? 200 : 500,
                Content = emailSent ? "Email sent successfully" : "Failed to send email",
                IsSuccessStatusCode = emailSent
            };
            
            _logger.Info($"SendEmail completed - Success: {emailSent}");
            
            return response;
        }
    }
}
