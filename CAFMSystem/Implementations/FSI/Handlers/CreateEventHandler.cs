using CAFMSystem.Constants;
using CAFMSystem.DTO.AtomicHandlerDTOs;
using CAFMSystem.DTO.CreateEventDTO;
using CAFMSystem.DTO.DownstreamDTOs;
using CAFMSystem.Helper;
using CAFMSystem.Implementations.FSI.AtomicHandlers;
using Core.DTOs;
using Core.Exceptions;
using Core.Extensions;
using Core.SystemLayer.Exceptions;
using Core.SystemLayer.Handlers;
using Core.SystemLayer.Middlewares;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CAFMSystem.Implementations.FSI.Handlers
{
    /// <summary>
    /// Handler for CreateEvent operation.
    /// Links a recurring event to an existing breakdown task in CAFM.
    /// </summary>
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
            _logger.Info("[System Layer]-Initiating CreateEvent");

            // Get SessionId from RequestContext (set by middleware)
            string? sessionId = RequestContext.GetSessionId();
            if (string.IsNullOrEmpty(sessionId))
            {
                throw new BaseException(ErrorConstants.FSI_SESSIO_0001);
            }
            else
            {
                // Call atomic handler
                HttpResponseSnapshot response = await CreateEventInDownstream(sessionId, request.TaskId, request.EventType);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.Error($"CreateEvent failed with status: {response.StatusCode}");
                    throw new DownStreamApiFailureException(
                        statusCode: (HttpStatusCode)response.StatusCode,
                        error: ErrorConstants.FSI_EVTCRT_0001,
                        errorDetails: new List<string> { $"CAFM API returned status {response.StatusCode}. Response: {response.Content}" },
                        stepName: "CreateEventHandler.cs / HandleAsync"
                    );
                }
                else
                {
                    // Deserialize SOAP response
                    CreateEventApiResDTO? apiResponse = DeserializeCreateEventResponse(response.Content!);

                    if (apiResponse == null)
                    {
                        throw new NoResponseBodyException(
                            error: ErrorConstants.FSI_EVTCRT_0002,
                            errorDetails: new List<string> { "CAFM returned blank or invalid response" },
                            stepName: "CreateEventHandler.cs / HandleAsync"
                        );
                    }
                    else
                    {
                        _logger.Info($"[System Layer]-Completed CreateEvent - EventId: {apiResponse.EventId}");

                        return new BaseResponseDTO(
                            message: InfoConstants.CREATE_EVENT_SUCCESS,
                            data: CreateEventResDTO.Map(apiResponse, request.TaskId),
                            errorCode: null
                        );
                    }
                }
            }
        }

        private async Task<HttpResponseSnapshot> CreateEventInDownstream(string sessionId, string taskId, string eventType)
        {
            CreateEventHandlerReqDTO atomicRequest = new CreateEventHandlerReqDTO
            {
                SessionId = sessionId,
                TaskId = taskId,
                EventType = eventType
            };

            return await _createEventAtomicHandler.Handle(atomicRequest);
        }

        private CreateEventApiResDTO? DeserializeCreateEventResponse(string xmlContent)
        {
            try
            {
                XDocument xdoc = XDocument.Parse(xmlContent);
                XNamespace soapNs = "http://schemas.xmlsoap.org/soap/envelope/";
                XNamespace ns = "http://www.fsi.co.uk/services/evolution/04/09";

                XElement? body = xdoc.Root?.Element(soapNs + "Body");
                XElement? response = body?.Element(ns + "CreateEventResponse");
                XElement? result = response?.Element(ns + "CreateEventResult");

                if (result == null)
                {
                    return null;
                }
                else
                {
                    return new CreateEventApiResDTO
                    {
                        EventId = result.Element(ns + "EventId")?.Value,
                        Status = result.Element(ns + "Status")?.Value
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to deserialize CreateEvent SOAP response");
                throw new NoResponseBodyException(
                    error: ErrorConstants.FSI_EVTCRT_0002,
                    errorDetails: new List<string> { $"Failed to parse SOAP response: {ex.Message}" },
                    stepName: "CreateEventHandler.cs / DeserializeCreateEventResponse"
                );
            }
        }
    }
}
