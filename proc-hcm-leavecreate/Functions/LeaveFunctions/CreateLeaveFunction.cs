using Core.DTOs;
using Core.Exceptions;
using Core.Extensions;
using Core.Middlewares;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using ProcHcmLeaveCreate.Constants;
using ProcHcmLeaveCreate.Domains;
using ProcHcmLeaveCreate.DTOs.CreateLeave;
using ProcHcmLeaveCreate.Helper;
using ProcHcmLeaveCreate.Services;
using System.Net;

namespace ProcHcmLeaveCreate.Functions.LeaveFunctions
{
    public class CreateLeaveFunction
    {
        private readonly ILogger<CreateLeaveFunction> _logger;
        private readonly LeaveService _leaveService;

        public CreateLeaveFunction(ILogger<CreateLeaveFunction> logger, LeaveService leaveService)
        {
            _logger = logger;
            _leaveService = leaveService;
        }

        [Function("CreateLeave")]
        public async Task<BaseResponseDTO> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "hcm/leave/create")] HttpRequest req)
        {
            _logger.Info("HTTP Request received for CreateLeave.");

            CreateLeaveReqDTO? dto = await req.ReadBodyAsync<CreateLeaveReqDTO>();
            
            if (dto == null)
            {
                throw new NoRequestBodyException(
                    errorDetails: [ErrorCodes.REQ_BODY_MISSING_OR_EMPTY.Message],
                    stepName: "CreateLeaveFunction.cs / Executing Run"
                );
            }

            dto.Validate();

            Leave domain = new Leave();
            dto.Populate(domain);

            HttpResponseMessage response = await _leaveService.CreateLeave(domain);

            if (response.IsSuccessStatusCode)
            {
                BaseResponseDTO? systemLayerResponse = await response.ExtractBaseResponseAsync();
                
                if (systemLayerResponse?.Data != null)
                {
                    string dataJson = System.Text.Json.JsonSerializer.Serialize(systemLayerResponse.Data);
                    CreateLeaveResDTO resDto = new CreateLeaveResDTO();
                    ResponseDTOHelper.PopulateCreateLeaveRes(dataJson, resDto);
                    
                    return new BaseResponseDTO(InfoConstants.CREATE_LEAVE_SUCCESS, string.Empty, resDto);
                }
                else
                {
                    throw new NoResponseBodyException(
                        errorDetails: ["System Layer response data is null or empty"],
                        stepName: "CreateLeaveFunction.cs / Executing Run / ExtractData"
                    );
                }
            }
            else
            {
                BaseResponseDTO errorResponse = await response.ExtractBaseResponseAsync();
                throw new PassThroughHttpException(errorResponse, (HttpStatusCode)response.StatusCode);
            }
        }
    }
}
