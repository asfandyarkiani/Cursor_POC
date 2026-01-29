using CAFMSystem.Constants;
using CAFMSystem.DTO.AtomicHandlerDTOs;
using CAFMSystem.DTO.DownstreamDTOs;
using CAFMSystem.DTO.GetBreakdownTasksByDtoDTO;
using CAFMSystem.Helper;
using CAFMSystem.Implementations.CAFM.AtomicHandlers;
using Core.DTOs;
using Core.Exceptions;
using Core.Extensions;
using Core.SystemLayer.Exceptions;
using Core.SystemLayer.Handlers;
using Core.SystemLayer.Middlewares;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Threading.Tasks;

namespace CAFMSystem.Implementations.CAFM.Handlers
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
                _logger.Error($"GetBreakdownTasksByDto failed with status: {response.StatusCode}");
                throw new DownStreamApiFailureException(
                    statusCode: (HttpStatusCode)response.StatusCode,
                    error: ErrorConstants.CAF_TSKGET_0001,
                    errorDetails: new string[] { $"CAFM API failed. Status: {response.StatusCode}. Response: {response.Content}" },
                    stepName: "GetBreakdownTasksByDtoHandler.cs / HandleAsync");
            }
            else
            {
                GetBreakdownTasksByDtoApiResDTO? apiResponse = SOAPHelper.DeserializeSoapResponse<GetBreakdownTasksByDtoApiResDTO>(response.Content!);
                
                _logger.Info("[System Layer]-Completed Get Breakdown Tasks By Dto");
                
                return new BaseResponseDTO(
                    message: InfoConstants.GET_BREAKDOWN_TASKS_SUCCESS,
                    data: GetBreakdownTasksByDtoResDTO.Map(apiResponse, request.ServiceRequestNumber),
                    errorCode: null);
            }
        }

        private async Task<HttpResponseSnapshot> GetBreakdownTasksByDtoFromDownstream(GetBreakdownTasksByDtoReqDTO request)
        {
            GetBreakdownTasksByDtoHandlerReqDTO atomicRequest = new GetBreakdownTasksByDtoHandlerReqDTO
            {
                ServiceRequestNumber = request.ServiceRequestNumber
            };
            
            return await _getBreakdownTasksByDtoAtomicHandler.Handle(atomicRequest);
        }
    }
}
