using Core.DTOs;
using Core.Exceptions;
using Core.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using sys_cafm_mgmt.Abstractions;
using sys_cafm_mgmt.DTO.SendEmailDTO;

namespace sys_cafm_mgmt.Functions
{
    public class SendEmailAPI
    {
        private readonly ILogger<SendEmailAPI> _logger;
        private readonly INotificationMgmt _notificationMgmt;

        public SendEmailAPI(
            ILogger<SendEmailAPI> logger,
            INotificationMgmt notificationMgmt)
        {
            _logger = logger;
            _notificationMgmt = notificationMgmt;
        }

        [Function("SendEmail")]
        public async Task<BaseResponseDTO> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "notification/email")] HttpRequest req,
            FunctionContext context)
        {
            _logger.Info("HTTP trigger received for SendEmail.");
            
            SendEmailReqDTO? request = await req.ReadBodyAsync<SendEmailReqDTO>();
            
            if (request == null)
            {
                _logger.Error("Request body is null or invalid.");
                throw new NoRequestBodyException(
                    errorDetails: [Core.Constants.ErrorCodes.REQ_BODY_MISSING_OR_EMPTY.Message],
                    stepName: "SendEmailAPI.cs / Executing Run");
            }
            
            request.ValidateAPIRequestParameters();
            
            BaseResponseDTO result = await _notificationMgmt.SendEmail(request);
            
            return result;
        }
    }
}
