using CAFMManagementSystem.Constants;
using CAFMManagementSystem.DTO.AtomicHandlerDTOs;
using CAFMManagementSystem.DTO.DownstreamDTOs;
using CAFMManagementSystem.DTO.GetBreakdownTasksByDtoDTO;
using CAFMManagementSystem.Helper;
using CAFMManagementSystem.Implementations.FSIConcept.AtomicHandlers;
using Core.DTOs;
using Core.Extensions;
using Core.Helpers;
using Core.SystemLayer.Exceptions;
using Core.SystemLayer.Handlers;
using Core.SystemLayer.Middlewares;
using Microsoft.Extensions.Logging;
using System.Net;

namespace CAFMManagementSystem.Implementations.FSIConcept.Handlers
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
            
            HttpResponseSnapshot response = await GetBreakdownTasksByDtoFromDownstream(request);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.Error($"GetBreakdownTasksByDto failed: {response.StatusCode}");
                throw new DownStreamApiFailureException(
                    statusCode: (HttpStatusCode)response.StatusCode,
                    error: ErrorConstants.CAF_GETTSK_0001,
                    errorDetails: [$"CAFM GetBreakdownTasksByDto API failed. Status: {response.StatusCode}. Response: {response.Content}"],
                    stepName: "GetBreakdownTasksByDtoHandler.cs / HandleAsync"
                );
            }
            else
            {
                GetBreakdownTasksByDtoApiResDTO? apiResponse = SOAPHelper.DeserializeSoapResponse<GetBreakdownTasksByDtoApiResDTO>(response.Content!);
                
                if (apiResponse == null)
                {
                    throw new NoResponseBodyException(
                        error: ErrorConstants.CAF_GETTSK_0002,
                        errorDetails: ["CAFM GetBreakdownTasksByDto returned empty response"],
                        stepName: "GetBreakdownTasksByDtoHandler.cs / HandleAsync"
                    );
                }
                else
                {
                    _logger.Info("[System Layer]-Completed Get Breakdown Tasks By Dto");
                    return new BaseResponseDTO(
                        message: InfoConstants.GET_BREAKDOWN_TASKS_SUCCESS,
                        data: GetBreakdownTasksByDtoResDTO.Map(apiResponse),
                        errorCode: null
                    );
                }
            }
        }
        
        private async Task<HttpResponseSnapshot> GetBreakdownTasksByDtoFromDownstream(GetBreakdownTasksByDtoReqDTO request)
        {
            string? sessionId = RequestContext.GetSessionId();
            
            if (string.IsNullOrEmpty(sessionId))
            {
                throw new BaseException(ErrorConstants.CAF_SESSIO_0001);
            }
            
            GetBreakdownTasksByDtoHandlerReqDTO atomicRequest = new GetBreakdownTasksByDtoHandlerReqDTO
            {
                SessionId = sessionId,
                CallId = request.ServiceRequestNumber
            };
            
            return await _getBreakdownTasksByDtoAtomicHandler.Handle(atomicRequest);
        }
    }
}
