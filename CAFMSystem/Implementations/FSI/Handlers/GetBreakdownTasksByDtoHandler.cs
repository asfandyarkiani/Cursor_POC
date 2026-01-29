using CAFMSystem.Constants;
using CAFMSystem.DTO.AtomicHandlerDTOs;
using CAFMSystem.DTO.DownstreamDTOs;
using CAFMSystem.DTO.GetBreakdownTasksByDtoDTO;
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
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CAFMSystem.Implementations.FSI.Handlers
{
    /// <summary>
    /// Handler for GetBreakdownTasksByDto operation.
    /// Checks if breakdown task already exists in CAFM based on CallId (serviceRequestNumber).
    /// </summary>
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
            _logger.Info("[System Layer]-Initiating GetBreakdownTasksByDto");

            // Get SessionId from RequestContext (set by middleware)
            string? sessionId = RequestContext.GetSessionId();
            if (string.IsNullOrEmpty(sessionId))
            {
                throw new BaseException(ErrorConstants.FSI_SESSIO_0001);
            }
            else
            {
                // Call atomic handler
                HttpResponseSnapshot response = await GetBreakdownTasksByDtoFromDownstream(sessionId, request.ServiceRequestNumber);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.Error($"GetBreakdownTasksByDto failed with status: {response.StatusCode}");
                    throw new DownStreamApiFailureException(
                        statusCode: (HttpStatusCode)response.StatusCode,
                        error: ErrorConstants.FSI_TSKGET_0001,
                        errorDetails: new List<string> { $"CAFM API returned status {response.StatusCode}. Response: {response.Content}" },
                        stepName: "GetBreakdownTasksByDtoHandler.cs / HandleAsync"
                    );
                }
                else
                {
                    // Deserialize SOAP response
                    GetBreakdownTasksByDtoApiResDTO? apiResponse = DeserializeGetBreakdownTasksResponse(response.Content!);

                    if (apiResponse == null)
                    {
                        _logger.Warn("GetBreakdownTasksByDto returned null response");
                        apiResponse = new GetBreakdownTasksByDtoApiResDTO { Tasks = new List<BreakdownTaskApiInfo>() };
                    }
                    else
                    {
                        // Response is valid
                    }

                    _logger.Info("[System Layer]-Completed GetBreakdownTasksByDto");

                    return new BaseResponseDTO(
                        message: InfoConstants.GET_TASKS_SUCCESS,
                        data: GetBreakdownTasksByDtoResDTO.Map(apiResponse),
                        errorCode: null
                    );
                }
            }
        }

        private async Task<HttpResponseSnapshot> GetBreakdownTasksByDtoFromDownstream(string sessionId, string callId)
        {
            GetBreakdownTasksByDtoHandlerReqDTO atomicRequest = new GetBreakdownTasksByDtoHandlerReqDTO
            {
                SessionId = sessionId,
                CallId = callId
            };

            return await _getBreakdownTasksByDtoAtomicHandler.Handle(atomicRequest);
        }

        private GetBreakdownTasksByDtoApiResDTO? DeserializeGetBreakdownTasksResponse(string xmlContent)
        {
            try
            {
                XDocument xdoc = XDocument.Parse(xmlContent);
                XNamespace soapNs = "http://schemas.xmlsoap.org/soap/envelope/";
                XNamespace ns = "http://www.fsi.co.uk/services/evolution/04/09";

                XElement? body = xdoc.Root?.Element(soapNs + "Body");
                XElement? response = body?.Element(ns + "GetBreakdownTasksByDtoResponse");
                XElement? result = response?.Element(ns + "GetBreakdownTasksByDtoResult");

                if (result == null)
                {
                    return new GetBreakdownTasksByDtoApiResDTO { Tasks = new List<BreakdownTaskApiInfo>() };
                }
                else
                {
                    List<BreakdownTaskApiInfo> tasks = new List<BreakdownTaskApiInfo>();

                    foreach (XElement taskElement in result.Elements(ns + "BreakdownTask"))
                    {
                        tasks.Add(new BreakdownTaskApiInfo
                        {
                            TaskId = taskElement.Element(ns + "TaskId")?.Value,
                            CallId = taskElement.Element(ns + "CallId")?.Value,
                            Status = taskElement.Element(ns + "Status")?.Value
                        });
                    }

                    return new GetBreakdownTasksByDtoApiResDTO { Tasks = tasks };
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to deserialize GetBreakdownTasksByDto SOAP response");
                throw new NoResponseBodyException(
                    error: ErrorConstants.FSI_TSKGET_0002,
                    errorDetails: new List<string> { $"Failed to parse SOAP response: {ex.Message}" },
                    stepName: "GetBreakdownTasksByDtoHandler.cs / DeserializeGetBreakdownTasksResponse"
                );
            }
        }
    }
}
