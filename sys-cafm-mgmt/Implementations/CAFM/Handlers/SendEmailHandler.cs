using Core.DTOs;
using Core.Exceptions;
using Core.Extensions;
using Core.SystemLayer.Exceptions;
using Core.SystemLayer.Handlers;
using Core.SystemLayer.Middlewares;
using Microsoft.Extensions.Logging;
using sys_cafm_mgmt.Constants;
using sys_cafm_mgmt.DTO.AtomicHandlerDTOs;
using sys_cafm_mgmt.DTO.SendEmailDTO;
using sys_cafm_mgmt.Implementations.CAFM.AtomicHandlers;
using System.Net;

namespace sys_cafm_mgmt.Implementations.CAFM.Handlers
{
    public class SendEmailHandler : IBaseHandler<SendEmailReqDTO>
    {
        private readonly ILogger<SendEmailHandler> _logger;
        private readonly SendEmailAtomicHandler _sendEmailAtomicHandler;

        public SendEmailHandler(
            ILogger<SendEmailHandler> logger,
            SendEmailAtomicHandler sendEmailAtomicHandler)
        {
            _logger = logger;
            _sendEmailAtomicHandler = sendEmailAtomicHandler;
        }

        public async Task<BaseResponseDTO> HandleAsync(SendEmailReqDTO request)
        {
            _logger.Info("[System Layer]-Initiating Send Email");
            
            HttpResponseSnapshot sendEmailResponse = await SendEmailToDownstream(request);
            if (!sendEmailResponse.IsSuccessStatusCode)
            {
                _logger.Error($"SendEmail failed: {sendEmailResponse.StatusCode}");
                throw new DownStreamApiFailureException(
                    statusCode: (HttpStatusCode)sendEmailResponse.StatusCode,
                    error: ErrorConstants.EML_SEND_0001,
                    errorDetails: [$"Failed to send email. Status: {sendEmailResponse.StatusCode}. Response: {sendEmailResponse.Content}"],
                    stepName: "SendEmailHandler.cs / HandleAsync");
            }
            else
            {
                _logger.Info("[System Layer]-Completed Send Email");
                return new BaseResponseDTO(
                    message: InfoConstants.SEND_EMAIL_SUCCESS,
                    data: SendEmailResDTO.Map(true, "Email sent successfully"),
                    errorCode: null);
            }
        }

        private async Task<HttpResponseSnapshot> SendEmailToDownstream(SendEmailReqDTO request)
        {
            SendEmailHandlerReqDTO atomicRequest = new SendEmailHandlerReqDTO
            {
                To = request.To,
                Subject = request.Subject,
                Body = request.Body,
                AttachmentFileName = request.AttachmentFileName,
                AttachmentContent = request.AttachmentContent
            };
            return await _sendEmailAtomicHandler.Handle(atomicRequest);
        }
    }
}
