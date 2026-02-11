using CAFMSystem.Abstractions;
using CAFMSystem.Attributes;
using CAFMSystem.DTO.HandlerDTOs.CreateEventDTO;
using Core.Constants;
using Core.DTOs;
using Core.Exceptions;
using Core.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace CAFMSystem.Functions
{
    public class CreateEventAPI
    {
        private readonly ILogger<CreateEventAPI> _logger;
        private readonly ICAFMMgmt _cafmMgmt;

        public CreateEventAPI(ILogger<CreateEventAPI> logger, ICAFMMgmt cafmMgmt)
        {
            _logger = logger;
            _cafmMgmt = cafmMgmt;
        }

        [CAFMAuthentication]
        [Function("CreateEvent")]
        public async Task<BaseResponseDTO> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "cafm/event")] HttpRequest req,
            FunctionContext context)
        {
            _logger.Info("HTTP trigger received for Create Event.");
            
            CreateEventReqDTO? request = await req.ReadBodyAsync<CreateEventReqDTO>();
            
            if (request == null)
            {
                _logger.Error("Request body is null or invalid.");
                throw new NoRequestBodyException(
                    errorDetails: [ErrorCodes.REQ_BODY_MISSING_OR_EMPTY.Message],
                    stepName: "CreateEventAPI.cs / Executing Run");
            }
            
            request.ValidateAPIRequestParameters();
            
            BaseResponseDTO result = await _cafmMgmt.CreateEvent(request);
            
            return result;
        }
    }
}
