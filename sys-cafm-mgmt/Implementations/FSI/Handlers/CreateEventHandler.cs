using CAFMSystem.Constants;
using CAFMSystem.DTO.DownstreamDTOs;
using CAFMSystem.DTO.HandlerDTOs.CreateEventDTO;
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
            
            string? sessionId = RequestContext.GetSessionId();
            if (string.IsNullOrEmpty(sessionId))
            {
                _logger.Error("SessionId not found in RequestContext");
                throw new DownStreamApiFailureException(
                    statusCode: HttpStatusCode.Unauthorized,
                    error: ErrorConstants.SYS_AUTHENT_0002,
                    errorDetails: ["SessionId is required but not found in context"],
                    stepName: "CreateEventHandler.cs / HandleAsync");
            }
            
            CreateEventHandlerReqDTO atomicRequest = new CreateEventHandlerReqDTO
            {
                SessionId = sessionId,
                TaskId = request.TaskId,
                Comments = request.Comments
            };
            
            HttpResponseSnapshot response = await _createEventAtomicHandler.Handle(atomicRequest);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.Error($"CreateEvent failed with status: {response.StatusCode}");
                throw new DownStreamApiFailureException(
                    statusCode: (HttpStatusCode)response.StatusCode,
                    error: ErrorConstants.SYS_EVTCRT_0001,
                    errorDetails: [$"CreateEvent API failed. Status: {response.StatusCode}. Response: {response.Content}"],
                    stepName: "CreateEventHandler.cs / HandleAsync");
            }
            
            CreateEventApiResDTO? apiResponse = SOAPHelper.DeserializeSoapResponse<CreateEventApiResDTO>(response.Content!);
            
            _logger.Info("[System Layer]-Completed Create Event");
            
            return new BaseResponseDTO(
                message: InfoConstants.CREATE_EVENT_SUCCESS,
                data: CreateEventResDTO.Map(apiResponse),
                errorCode: null);
        }
    }
}
