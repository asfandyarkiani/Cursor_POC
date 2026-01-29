using Core.DTOs;
using Core.Extensions;
using Core.SystemLayer.Exceptions;
using Core.SystemLayer.Handlers;
using FsiCafmSystem.ConfigModels;
using FsiCafmSystem.Constants;
using FsiCafmSystem.DTO.AtomicHandlerDTOs;
using FsiCafmSystem.DTO.HandlerDTOs.SendEmailDTO;
using FsiCafmSystem.Implementations.FsiCafm.AtomicHandlers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;

namespace FsiCafmSystem.Implementations.FsiCafm.Handlers
{
    public class SendEmailHandler : IBaseHandler<SendEmailReqDTO>
    {
        private readonly ILogger<SendEmailHandler> _logger;
        private readonly SendEmailAtomicHandler _sendEmailAtomicHandler;
        private readonly AppConfigs _appConfigs;
        
        public SendEmailHandler(
            ILogger<SendEmailHandler> logger,
            SendEmailAtomicHandler sendEmailAtomicHandler,
            IOptions<AppConfigs> options)
        {
            _logger = logger;
            _sendEmailAtomicHandler = sendEmailAtomicHandler;
            _appConfigs = options.Value;
        }
        
        public async Task<BaseResponseDTO> HandleAsync(SendEmailReqDTO request)
        {
            _logger.Info("[System Layer]-Initiating Send Email");
            
            SendEmailHandlerReqDTO atomicRequest = new SendEmailHandlerReqDTO
            {
                FromAddress = _appConfigs.SmtpFromEmail,
                ToAddress = request.ToAddress,
                Subject = request.Subject,
                Body = request.Body,
                HasAttachment = request.HasAttachment,
                AttachmentContent = request.AttachmentContent,
                AttachmentFileName = request.AttachmentFileName
            };
            
            Core.SystemLayer.Middlewares.HttpResponseSnapshot response = await _sendEmailAtomicHandler.Handle(atomicRequest);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.Error($"Send email failed: {response.StatusCode}");
                throw new DownStreamApiFailureException(
                    statusCode: (HttpStatusCode)response.StatusCode,
                    error: ErrorConstants.EML_SEND_0001,
                    errorDetails: new List<string> { $"Failed to send email. Response: {response.Content}" },
                    stepName: "SendEmailHandler / HandleAsync");
            }
            
            _logger.Info("[System Layer]-Completed Send Email");
            
            return new BaseResponseDTO(
                message: InfoConstants.SEND_EMAIL_SUCCESS,
                data: SendEmailResDTO.Map(true, "Email sent successfully"),
                errorCode: null);
        }
    }
}
