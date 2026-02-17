using Core.DTOs;
using Core.Exceptions;
using Core.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ProcHcmLeave.ConfigModels;
using ProcHcmLeave.Constants;
using ProcHcmLeave.Domains.HumanResource;
using ProcHcmLeave.DTOs.CreateLeave;
using ProcHcmLeave.Helper;
using ProcHcmLeave.Services;
using System.Net;

namespace ProcHcmLeave.Functions.LeaveFunctions
{
    public class CreateLeaveFunction
    {
        private readonly ILogger<CreateLeaveFunction> _logger;
        private readonly LeaveService _service;
        private readonly AppConfigs _options;

        public CreateLeaveFunction(ILogger<CreateLeaveFunction> logger, LeaveService service, IOptions<AppConfigs> options)
        {
            _logger = logger;
            _service = service;
            _options = options.Value;
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

            HttpResponseMessage response = await _service.CreateLeave(dto);

            if (response.IsSuccessStatusCode)
            {
                string responseContent = await response.Content.ReadAsStringAsync();
                
                BaseResponseDTO? systemLayerResponse = await response.ExtractBaseResponseAsync();
                
                if (systemLayerResponse?.Data != null)
                {
                    string dataJson = systemLayerResponse.Data.ToString() ?? string.Empty;
                    
                    CreateLeaveResDTO resDto = new CreateLeaveResDTO
                    {
                        Status = InfoConstants.SUCCESS_STATUS,
                        Message = InfoConstants.SUCCESS_MESSAGE,
                        Success = InfoConstants.TRUE_STRING
                    };
                    
                    ResponseDTOHelper.PopulateCreateLeaveRes(dataJson, resDto);
                    
                    return new BaseResponseDTO(
                        InfoConstants.CREATE_LEAVE_SUCCESS,
                        string.Empty,
                        resDto
                    );
                }
                else
                {
                    CreateLeaveResDTO resDto = new CreateLeaveResDTO
                    {
                        Status = InfoConstants.SUCCESS_STATUS,
                        Message = InfoConstants.SUCCESS_MESSAGE,
                        Success = InfoConstants.TRUE_STRING
                    };
                    
                    return new BaseResponseDTO(
                        InfoConstants.CREATE_LEAVE_SUCCESS,
                        string.Empty,
                        resDto
                    );
                }
            }
            else
            {
                BaseResponseDTO errorResponse = await response.ExtractBaseResponseAsync();
                string errorMessage = errorResponse.Message;
                
                _logger.Error($"System Layer call failed: {errorMessage}");

                CreateLeaveResDTO errorResDto = new CreateLeaveResDTO
                {
                    Status = InfoConstants.FAILURE_STATUS,
                    Message = errorMessage,
                    Success = InfoConstants.FALSE_STRING
                };

                throw new PassThroughHttpException(
                    new BaseResponseDTO("Error creating leave", errorMessage, errorResDto),
                    (HttpStatusCode)response.StatusCode
                );
            }
        }
    }
}
