using CAFMSystem.Constants;
using CAFMSystem.DTO.AtomicHandlerDTOs;
using CAFMSystem.DTO.CreateEventDTO;
using CAFMSystem.DTO.DownstreamDTOs;
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
    public class CreateEventHandler : IBaseHandler<CreateEventReqDTO>
    {
        private readonly ILogger<CreateEventHandler> _logger;
        private readonly CreateEventAtomicHandler _createEventAtomicHandler;

        public CreateEventHandler(
            ILogger<CreateEventHandler> logger,
            CreateEventAtomicHandler createEventAtomicHandler)
        {
            _logger = logger;
            _createEventAtomicHandler = createEventAtomicHandler;
        }

        public async Task<BaseResponseDTO> HandleAsync(CreateEventReqDTO request)
        {
            _logger.Info("[System Layer]-Initiating Create Event");
            
            HttpResponseSnapshot response = await CreateEventInDownstream(request);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.Warn($"CreateEvent failed with status: {response.StatusCode} - Continuing (task already created)");
                
                return new BaseResponseDTO(
                    message: InfoConstants.CREATE_EVENT_SUCCESS,
                    data: CreateEventResDTO.Map(null, request.BreakdownTaskId),
                    errorCode: null);
            }
            else
            {
                CreateEventApiResDTO? apiResponse = SOAPHelper.DeserializeSoapResponse<CreateEventApiResDTO>(response.Content!);
                
                _logger.Info("[System Layer]-Completed Create Event");
                
                return new BaseResponseDTO(
                    message: InfoConstants.CREATE_EVENT_SUCCESS,
                    data: CreateEventResDTO.Map(apiResponse, request.BreakdownTaskId),
                    errorCode: null);
            }
        }

        private async Task<HttpResponseSnapshot> CreateEventInDownstream(CreateEventReqDTO request)
        {
            CreateEventHandlerReqDTO atomicRequest = new CreateEventHandlerReqDTO
            {
                BreakdownTaskId = request.BreakdownTaskId
            };
            
            return await _createEventAtomicHandler.Handle(atomicRequest);
        }
    }
}
