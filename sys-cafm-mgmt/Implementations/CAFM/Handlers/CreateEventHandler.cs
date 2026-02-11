using Core.DTOs;
using Core.Exceptions;
using Core.Extensions;
using Core.SystemLayer.Exceptions;
using Core.SystemLayer.Handlers;
using Core.SystemLayer.Middlewares;
using Microsoft.Extensions.Logging;
using sys_cafm_mgmt.Constants;
using sys_cafm_mgmt.DTO.AtomicHandlerDTOs;
using sys_cafm_mgmt.DTO.CreateEventDTO;
using sys_cafm_mgmt.DTO.DownstreamDTOs;
using sys_cafm_mgmt.Helper;
using sys_cafm_mgmt.Implementations.CAFM.AtomicHandlers;
using System.Net;

namespace sys_cafm_mgmt.Implementations.CAFM.Handlers
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
                throw new BusinessCaseFailureException(
                    error: ErrorConstants.CAF_AUTHEN_0002,
                    errorDetails: ["SessionId not found in RequestContext"],
                    stepName: "CreateEventHandler.cs / HandleAsync");
            }
            
            HttpResponseSnapshot createEventResponse = await CreateEventInDownstream(request, sessionId);
            if (!createEventResponse.IsSuccessStatusCode)
            {
                _logger.Error($"CreateEvent failed: {createEventResponse.StatusCode}");
                throw new DownStreamApiFailureException(
                    statusCode: (HttpStatusCode)createEventResponse.StatusCode,
                    error: ErrorConstants.CAF_EVTCRT_0001,
                    errorDetails: [$"Failed to create event. Status: {createEventResponse.StatusCode}. Response: {createEventResponse.Content}"],
                    stepName: "CreateEventHandler.cs / HandleAsync");
            }
            else
            {
                CreateEventApiResDTO? createEventApiResponse = SOAPHelper.DeserializeSoapResponse<CreateEventApiResDTO>(createEventResponse.Content!);
                
                if (createEventApiResponse?.Envelope?.Body?.CreateEventResponse?.CreateEventResult == null)
                {
                    throw new NoResponseBodyException(
                        error: ErrorConstants.CAF_EVTCRT_0002,
                        errorDetails: ["CreateEvent returned empty result"],
                        stepName: "CreateEventHandler.cs / HandleAsync");
                }
                else
                {
                    _logger.Info("[System Layer]-Completed Create Event");
                    return new BaseResponseDTO(
                        message: InfoConstants.CREATE_EVENT_SUCCESS,
                        data: CreateEventResDTO.Map(createEventApiResponse),
                        errorCode: null);
                }
            }
        }

        private async Task<HttpResponseSnapshot> CreateEventInDownstream(CreateEventReqDTO request, string sessionId)
        {
            CreateEventHandlerReqDTO atomicRequest = new CreateEventHandlerReqDTO
            {
                SessionId = sessionId,
                BreakdownTaskId = request.BreakdownTaskId,
                EventDescription = request.EventDescription
            };
            return await _createEventAtomicHandler.Handle(atomicRequest);
        }
    }
}
