using CAFMSystem.Abstractions;
using CAFMSystem.Attributes;
using CAFMSystem.DTO.HandlerDTOs.GetBreakdownTasksByDtoDTO;
using Core.Constants;
using Core.DTOs;
using Core.Exceptions;
using Core.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace CAFMSystem.Functions
{
    public class GetBreakdownTasksByDtoAPI
    {
        private readonly ILogger<GetBreakdownTasksByDtoAPI> _logger;
        private readonly ICAFMMgmt _cafmMgmt;

        public GetBreakdownTasksByDtoAPI(ILogger<GetBreakdownTasksByDtoAPI> logger, ICAFMMgmt cafmMgmt)
        {
            _logger = logger;
            _cafmMgmt = cafmMgmt;
        }

        [CAFMAuthentication]
        [Function("GetBreakdownTasksByDto")]
        public async Task<BaseResponseDTO> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "cafm/breakdowntasks")] HttpRequest req,
            FunctionContext context)
        {
            _logger.Info("HTTP trigger received for Get Breakdown Tasks By Dto.");
            
            GetBreakdownTasksByDtoReqDTO? request = await req.ReadBodyAsync<GetBreakdownTasksByDtoReqDTO>();
            
            if (request == null)
            {
                _logger.Error("Request body is null or invalid.");
                throw new NoRequestBodyException(
                    errorDetails: [ErrorCodes.REQ_BODY_MISSING_OR_EMPTY.Message],
                    stepName: "GetBreakdownTasksByDtoAPI.cs / Executing Run");
            }
            
            request.ValidateAPIRequestParameters();
            
            BaseResponseDTO result = await _cafmMgmt.GetBreakdownTasksByDto(request);
            
            return result;
        }
    }
}
