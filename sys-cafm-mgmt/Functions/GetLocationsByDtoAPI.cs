using CAFMSystem.Abstractions;
using CAFMSystem.Attributes;
using CAFMSystem.DTO.HandlerDTOs.GetLocationsByDtoDTO;
using Core.Constants;
using Core.DTOs;
using Core.Exceptions;
using Core.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace CAFMSystem.Functions
{
    public class GetLocationsByDtoAPI
    {
        private readonly ILogger<GetLocationsByDtoAPI> _logger;
        private readonly ICAFMMgmt _cafmMgmt;

        public GetLocationsByDtoAPI(ILogger<GetLocationsByDtoAPI> logger, ICAFMMgmt cafmMgmt)
        {
            _logger = logger;
            _cafmMgmt = cafmMgmt;
        }

        [CAFMAuthentication]
        [Function("GetLocationsByDto")]
        public async Task<BaseResponseDTO> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "cafm/locations")] HttpRequest req,
            FunctionContext context)
        {
            _logger.Info("HTTP trigger received for Get Locations By Dto.");
            
            GetLocationsByDtoReqDTO? request = await req.ReadBodyAsync<GetLocationsByDtoReqDTO>();
            
            if (request == null)
            {
                _logger.Error("Request body is null or invalid.");
                throw new NoRequestBodyException(
                    errorDetails: [ErrorCodes.REQ_BODY_MISSING_OR_EMPTY.Message],
                    stepName: "GetLocationsByDtoAPI.cs / Executing Run");
            }
            
            request.ValidateAPIRequestParameters();
            
            BaseResponseDTO result = await _cafmMgmt.GetLocationsByDto(request);
            
            return result;
        }
    }
}
