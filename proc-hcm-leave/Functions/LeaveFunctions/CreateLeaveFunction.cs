using Core.DTOs;
using Core.Exceptions;
using Core.Extensions;
using Core.Middlewares;
using HcmLeaveProcessLayer.Constants;
using HcmLeaveProcessLayer.Domains;
using HcmLeaveProcessLayer.DTOs.CreateLeave;
using HcmLeaveProcessLayer.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Net;

namespace HcmLeaveProcessLayer.Functions.LeaveFunctions
{
    public class CreateLeaveFunction
    {
        private readonly ILogger<CreateLeaveFunction> _logger;
        private readonly LeaveService _service;

        public CreateLeaveFunction(ILogger<CreateLeaveFunction> logger, LeaveService service)
        {
            _logger = logger;
            _service = service;
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

            HttpResponseMessage response = await _service.CreateLeave(domain);

            if (response.IsSuccessStatusCode)
            {
                string responseContent = await response.Content.ReadAsStringAsync();
                
                BaseResponseDTO? systemLayerResponse = await response.ExtractBaseResponseAsync();
                
                if (systemLayerResponse?.Data != null)
                {
                    string dataJson = System.Text.Json.JsonSerializer.Serialize(systemLayerResponse.Data);
                    CreateLeaveResDTO resDto = new CreateLeaveResDTO();
                    Helper.ResponseDTOHelper.PopulateCreateLeaveRes(dataJson, resDto);
                    
                    return new BaseResponseDTO(InfoConstants.CREATE_LEAVE_SUCCESS, string.Empty, resDto);
                }
                else
                {
                    throw new NoResponseBodyException(
                        errorDetails: ["System Layer returned empty response data"],
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
