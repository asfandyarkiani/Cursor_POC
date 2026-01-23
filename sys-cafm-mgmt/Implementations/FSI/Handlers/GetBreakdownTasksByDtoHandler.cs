using CAFMSystem.Constants;
using CAFMSystem.DTO.DownstreamDTOs;
using CAFMSystem.DTO.HandlerDTOs.GetBreakdownTasksByDtoDTO;
using CAFMSystem.DTO.AtomicHandlerDTOs;
using CAFMSystem.Helper;
using CAFMSystem.Implementations.FSI.AtomicHandlers;
using Core.DTOs;
using Core.Extensions;
using Core.SystemLayer.Exceptions;
using Core.SystemLayer.Handlers;
using Microsoft.Extensions.Logging;
using System.Net;

namespace CAFMSystem.Implementations.FSI.Handlers
{
    public class GetBreakdownTasksByDtoHandler : IBaseHandler<GetBreakdownTasksByDtoReqDTO>
    {
        private readonly ILogger<GetBreakdownTasksByDtoHandler> _logger;
        private readonly GetBreakdownTasksByDtoAtomicHandler _getBreakdownTasksByDtoAtomicHandler;

        public GetBreakdownTasksByDtoHandler(
            ILogger<GetBreakdownTasksByDtoHandler> logger,
            GetBreakdownTasksByDtoAtomicHandler getBreakdownTasksByDtoAtomicHandler)
        {
            _logger = logger;
            _getBreakdownTasksByDtoAtomicHandler = getBreakdownTasksByDtoAtomicHandler;
        }

        public async Task<BaseResponseDTO> HandleAsync(GetBreakdownTasksByDtoReqDTO request)
        {
            _logger.Info("[System Layer]-Initiating Get Breakdown Tasks By Dto");
            
            string? sessionId = RequestContext.GetSessionId();
            if (string.IsNullOrEmpty(sessionId))
            {
                _logger.Error("SessionId not found in RequestContext");
                throw new DownStreamApiFailureException(
                    statusCode: HttpStatusCode.Unauthorized,
                    error: ErrorConstants.SYS_AUTHENT_0002,
                    errorDetails: ["SessionId is required but not found in context"],
                    stepName: "GetBreakdownTasksByDtoHandler.cs / HandleAsync");
            }
            
            GetBreakdownTasksByDtoHandlerReqDTO atomicRequest = new GetBreakdownTasksByDtoHandlerReqDTO
            {
                SessionId = sessionId,
                ServiceRequestNumber = request.ServiceRequestNumber
            };
            
            HttpResponseSnapshot response = await _getBreakdownTasksByDtoAtomicHandler.Handle(atomicRequest);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.Error($"GetBreakdownTasksByDto failed with status: {response.StatusCode}");
                throw new DownStreamApiFailureException(
                    statusCode: (HttpStatusCode)response.StatusCode,
                    error: ErrorConstants.SYS_TSKGET_0001,
                    errorDetails: [$"GetBreakdownTasksByDto API failed. Status: {response.StatusCode}. Response: {response.Content}"],
                    stepName: "GetBreakdownTasksByDtoHandler.cs / HandleAsync");
            }
            
            GetBreakdownTasksByDtoApiResDTO? apiResponse = SOAPHelper.DeserializeSoapResponse<GetBreakdownTasksByDtoApiResDTO>(response.Content!);
            
            _logger.Info("[System Layer]-Completed Get Breakdown Tasks By Dto");
            
            return new BaseResponseDTO(
                message: InfoConstants.GET_BREAKDOWN_TASK_SUCCESS,
                data: GetBreakdownTasksByDtoResDTO.Map(apiResponse),
                errorCode: null);
        }
    }
}
