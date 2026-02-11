using Core.Constants;
using Core.DTOs;
using Core.Exceptions;
using Core.Extensions;
using FsiCafmSystem.Abstractions;
using FsiCafmSystem.Attributes;
using FsiCafmSystem.DTO.HandlerDTOs.CreateWorkOrderDTO;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace FsiCafmSystem.Functions
{
    public class CreateWorkOrderAPI
    {
        private readonly ILogger<CreateWorkOrderAPI> _logger;
        private readonly IWorkOrderMgmt _workOrderMgmt;
        
        public CreateWorkOrderAPI(
            ILogger<CreateWorkOrderAPI> logger,
            IWorkOrderMgmt workOrderMgmt)
        {
            _logger = logger;
            _workOrderMgmt = workOrderMgmt;
        }
        
        [CustomAuthentication]
        [Function("CreateWorkOrder")]
        public async Task<BaseResponseDTO> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "workorder/create")] HttpRequest req)
        {
            _logger.Info("HTTP trigger received for Create Work Order.");
            
            CreateWorkOrderReqDTO? request = await req.ReadBodyAsync<CreateWorkOrderReqDTO>();
            
            if (request == null)
            {
                _logger.Error("Request body is null or invalid.");
                throw new NoRequestBodyException(
                    errorDetails: new List<string> { ErrorCodes.REQ_BODY_MISSING_OR_EMPTY.Message },
                    stepName: "CreateWorkOrderAPI.cs / Executing Run");
            }
            
            request.ValidateAPIRequestParameters();
            
            BaseResponseDTO result = await _workOrderMgmt.CreateWorkOrder(request);
            
            return result;
        }
    }
}
