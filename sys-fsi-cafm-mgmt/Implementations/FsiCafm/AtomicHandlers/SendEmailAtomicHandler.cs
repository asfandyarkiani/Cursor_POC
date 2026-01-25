using Core.Extensions;
using Core.SystemLayer.DTOs;
using Core.SystemLayer.Handlers;
using Core.SystemLayer.Middlewares;
using FsiCafmSystem.Constants;
using FsiCafmSystem.DTO.AtomicHandlerDTOs;
using FsiCafmSystem.Helper;
using Microsoft.Extensions.Logging;

namespace FsiCafmSystem.Implementations.FsiCafm.AtomicHandlers
{
    public class SendEmailAtomicHandler : IAtomicHandler<HttpResponseSnapshot>
    {
        private readonly ILogger<SendEmailAtomicHandler> _logger;
        private readonly CustomSmtpClient _smtpClient;
        
        public SendEmailAtomicHandler(
            ILogger<SendEmailAtomicHandler> logger,
            CustomSmtpClient smtpClient)
        {
            _logger = logger;
            _smtpClient = smtpClient;
        }
        
        public async Task<HttpResponseSnapshot> Handle(IDownStreamRequestDTO downStreamRequestDTO)
        {
            SendEmailHandlerReqDTO requestDTO = downStreamRequestDTO as SendEmailHandlerReqDTO 
                ?? throw new ArgumentException("Invalid DTO type");
            
            _logger.Info($"SendEmailAtomicHandler processing email to: {requestDTO.ToAddress}");
            
            requestDTO.ValidateDownStreamRequestParameters();
            
            try
            {
                await _smtpClient.SendEmailAsync(
                    operationName: OperationNames.SEND_EMAIL,
                    fromAddress: requestDTO.FromAddress,
                    toAddress: requestDTO.ToAddress,
                    subject: requestDTO.Subject,
                    body: requestDTO.Body,
                    isHtml: true,
                    attachmentContent: requestDTO.AttachmentContent,
                    attachmentFileName: requestDTO.AttachmentFileName);
                
                _logger.Info("Email sent successfully");
                
                // Return success response
                return new HttpResponseSnapshot
                {
                    StatusCode = 200,
                    Content = "{\"success\": true, \"message\": \"Email sent successfully\"}",
                    IsSuccessStatusCode = true
                };
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to send email");
                
                // Return error response
                return new HttpResponseSnapshot
                {
                    StatusCode = 500,
                    Content = $"{{\"success\": false, \"message\": \"Failed to send email: {ex.Message}\"}}",
                    IsSuccessStatusCode = false
                };
            }
        }
    }
}
