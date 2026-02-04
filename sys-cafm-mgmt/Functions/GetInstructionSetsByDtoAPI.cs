using CAFMSystem.Abstractions;
using CAFMSystem.Attributes;
using CAFMSystem.DTO.HandlerDTOs.GetInstructionSetsByDtoDTO;
using Core.Constants;
using Core.DTOs;
using Core.Exceptions;
using Core.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace CAFMSystem.Functions
{
    public class GetInstructionSetsByDtoAPI
    {
        private readonly ILogger<GetInstructionSetsByDtoAPI> _logger;
        private readonly ICAFMMgmt _cafmMgmt;

        public GetInstructionSetsByDtoAPI(ILogger<GetInstructionSetsByDtoAPI> logger, ICAFMMgmt cafmMgmt)
        {
            _logger = logger;
            _cafmMgmt = cafmMgmt;
        }

        [CAFMAuthentication]
        [Function("GetInstructionSetsByDto")]
        public async Task<BaseResponseDTO> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "cafm/instructionsets")] HttpRequest req,
            FunctionContext context)
        {
            _logger.Info("HTTP trigger received for Get Instruction Sets By Dto.");
            
            GetInstructionSetsByDtoReqDTO? request = await req.ReadBodyAsync<GetInstructionSetsByDtoReqDTO>();
            
            if (request == null)
            {
                _logger.Error("Request body is null or invalid.");
                throw new NoRequestBodyException(
                    errorDetails: [ErrorCodes.REQ_BODY_MISSING_OR_EMPTY.Message],
                    stepName: "GetInstructionSetsByDtoAPI.cs / Executing Run");
            }
            
            request.ValidateAPIRequestParameters();
            
            BaseResponseDTO result = await _cafmMgmt.GetInstructionSetsByDto(request);
            
            return result;
        }
    }
}
