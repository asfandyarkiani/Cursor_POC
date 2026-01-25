using Core.Constants;
using Core.DTOs;
using Core.Exceptions;
using Core.Extensions;
using FsiCafmSystem.Abstractions;
using FsiCafmSystem.DTO.HandlerDTOs.SendEmailDTO;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace FsiCafmSystem.Functions
{
    public class SendEmailAPI
    {
        private readonly ILogger<SendEmailAPI> _logger;
        private readonly IEmailMgmt _emailMgmt;
        
        public SendEmailAPI(
            ILogger<SendEmailAPI> logger,
            IEmailMgmt emailMgmt)
        {
            _logger = logger;
            _emailMgmt = emailMgmt;
        }
        
        [Function("SendEmail")]
        public async Task<BaseResponseDTO> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "email/send")] HttpRequest req)
        {
            _logger.Info("HTTP trigger received for Send Email.");
            
            SendEmailReqDTO? request = await req.ReadBodyAsync<SendEmailReqDTO>();
            
            if (request == null)
            {
                _logger.Error("Request body is null or invalid.");
                throw new NoRequestBodyException(
                    errorDetails: new List<string> { ErrorCodes.REQ_BODY_MISSING_OR_EMPTY.Message },
                    stepName: "SendEmailAPI.cs / Executing Run");
            }
            
            request.ValidateAPIRequestParameters();
            
            BaseResponseDTO result = await _emailMgmt.SendEmail(request);
            
            return result;
        }
    }
}
