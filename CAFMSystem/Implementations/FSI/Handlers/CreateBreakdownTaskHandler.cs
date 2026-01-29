using CAFMSystem.ConfigModels;
using CAFMSystem.Constants;
using CAFMSystem.DTO.AtomicHandlerDTOs;
using CAFMSystem.DTO.CreateBreakdownTaskDTO;
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
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CAFMSystem.Implementations.FSI.Handlers
{
    /// <summary>
    /// Handler for CreateBreakdownTask operation.
    /// Orchestrates lookups (locations, instructions) and creates breakdown task in CAFM.
    /// Implements best-effort lookup pattern: lookups may fail, but creation continues.
    /// </summary>
    public class CreateBreakdownTaskHandler : IBaseHandler<CreateBreakdownTaskReqDTO>
    {
        private readonly ILogger<CreateBreakdownTaskHandler> _logger;
        private readonly GetLocationsByDtoAtomicHandler _getLocationsByDtoAtomicHandler;
        private readonly GetInstructionSetsByDtoAtomicHandler _getInstructionSetsByDtoAtomicHandler;
        private readonly CreateBreakdownTaskAtomicHandler _createBreakdownTaskAtomicHandler;
        private readonly AppConfigs _appConfigs;

        public CreateBreakdownTaskHandler(
            ILogger<CreateBreakdownTaskHandler> logger,
            GetLocationsByDtoAtomicHandler getLocationsByDtoAtomicHandler,
            GetInstructionSetsByDtoAtomicHandler getInstructionSetsByDtoAtomicHandler,
            CreateBreakdownTaskAtomicHandler createBreakdownTaskAtomicHandler,
            IOptions<AppConfigs> options)
        {
            _logger = logger;
            _getLocationsByDtoAtomicHandler = getLocationsByDtoAtomicHandler;
            _getInstructionSetsByDtoAtomicHandler = getInstructionSetsByDtoAtomicHandler;
            _createBreakdownTaskAtomicHandler = createBreakdownTaskAtomicHandler;
            _appConfigs = options.Value;
        }

        public async Task<BaseResponseDTO> HandleAsync(CreateBreakdownTaskReqDTO request)
        {
            _logger.Info("[System Layer]-Initiating CreateBreakdownTask");

            // Get SessionId from RequestContext (set by middleware)
            string? sessionId = RequestContext.GetSessionId();
            if (string.IsNullOrEmpty(sessionId))
            {
                throw new BaseException(ErrorConstants.FSI_SESSIO_0001);
            }
            else
            {
                // Step 1: Get Locations (BEST-EFFORT LOOKUP)
                string locationId = string.Empty;
                string buildingId = string.Empty;

                if (!string.IsNullOrWhiteSpace(request.PropertyName) && !string.IsNullOrWhiteSpace(request.UnitCode))
                {
                    HttpResponseSnapshot locationsResponse = await GetLocationsByDtoFromDownstream(sessionId, request.PropertyName, request.UnitCode);

                    if (!locationsResponse.IsSuccessStatusCode)
                    {
                        _logger.Warn($"GetLocationsByDto failed with status {locationsResponse.StatusCode} - Continuing with empty values");
                        locationId = string.Empty;
                        buildingId = string.Empty;
                    }
                    else
                    {
                        GetLocationsByDtoApiResDTO? locationsData = DeserializeGetLocationsResponse(locationsResponse.Content!);
                        if (locationsData != null)
                        {
                            locationId = locationsData.LocationId ?? string.Empty;
                            buildingId = locationsData.BuildingId ?? string.Empty;
                            _logger.Info($"Retrieved LocationId: {locationId}, BuildingId: {buildingId}");
                        }
                        else
                        {
                            _logger.Warn("GetLocationsByDto returned null - Continuing with empty values");
                            locationId = string.Empty;
                            buildingId = string.Empty;
                        }
                    }
                }
                else
                {
                    _logger.Warn("PropertyName or UnitCode not provided - Skipping location lookup");
                }

                // Step 2: Get Instruction Sets (BEST-EFFORT LOOKUP)
                string instructionId = string.Empty;

                if (!string.IsNullOrWhiteSpace(request.CategoryName))
                {
                    HttpResponseSnapshot instructionsResponse = await GetInstructionSetsByDtoFromDownstream(sessionId, request.CategoryName, request.SubCategory);

                    if (!instructionsResponse.IsSuccessStatusCode)
                    {
                        _logger.Warn($"GetInstructionSetsByDto failed with status {instructionsResponse.StatusCode} - Continuing with empty value");
                        instructionId = string.Empty;
                    }
                    else
                    {
                        GetInstructionSetsByDtoApiResDTO? instructionsData = DeserializeGetInstructionsResponse(instructionsResponse.Content!);
                        if (instructionsData != null)
                        {
                            instructionId = instructionsData.InstructionId ?? string.Empty;
                            _logger.Info($"Retrieved InstructionId: {instructionId}");
                        }
                        else
                        {
                            _logger.Warn("GetInstructionSetsByDto returned null - Continuing with empty value");
                            instructionId = string.Empty;
                        }
                    }
                }
                else
                {
                    _logger.Warn("CategoryName not provided - Skipping instruction lookup");
                }

                // Step 3: Format dates
                string scheduledDateUtc = FormatScheduledDate(request.ScheduledDate, request.ScheduledTimeStart);
                string raisedDateUtc = FormatRaisedDate(request.RaisedDateUtc);

                // Step 4: Create Breakdown Task (MAIN OPERATION)
                HttpResponseSnapshot createResponse = await CreateBreakdownTaskInDownstream(
                    sessionId,
                    request,
                    locationId,
                    buildingId,
                    instructionId,
                    scheduledDateUtc,
                    raisedDateUtc
                );

                if (!createResponse.IsSuccessStatusCode)
                {
                    _logger.Error($"CreateBreakdownTask failed with status: {createResponse.StatusCode}");
                    throw new DownStreamApiFailureException(
                        statusCode: (HttpStatusCode)createResponse.StatusCode,
                        error: ErrorConstants.FSI_TSKCRT_0001,
                        errorDetails: new List<string> { $"CAFM API returned status {createResponse.StatusCode}. Response: {createResponse.Content}" },
                        stepName: "CreateBreakdownTaskHandler.cs / HandleAsync"
                    );
                }
                else
                {
                    // Deserialize response
                    CreateBreakdownTaskApiResDTO? apiResponse = DeserializeCreateBreakdownTaskResponse(createResponse.Content!);

                    if (apiResponse == null || string.IsNullOrWhiteSpace(apiResponse.TaskId))
                    {
                        throw new NoResponseBodyException(
                            error: ErrorConstants.FSI_TSKCRT_0002,
                            errorDetails: new List<string> { "CAFM returned blank or invalid TaskId" },
                            stepName: "CreateBreakdownTaskHandler.cs / HandleAsync"
                        );
                    }
                    else
                    {
                        _logger.Info($"[System Layer]-Completed CreateBreakdownTask - TaskId: {apiResponse.TaskId}");

                        return new BaseResponseDTO(
                            message: InfoConstants.CREATE_TASK_SUCCESS,
                            data: CreateBreakdownTaskResDTO.Map(apiResponse, request.ServiceRequestNumber, request.SourceOrgId),
                            errorCode: null
                        );
                    }
                }
            }
        }

        private async Task<HttpResponseSnapshot> GetLocationsByDtoFromDownstream(string sessionId, string propertyName, string unitCode)
        {
            GetLocationsByDtoHandlerReqDTO atomicRequest = new GetLocationsByDtoHandlerReqDTO
            {
                SessionId = sessionId,
                PropertyName = propertyName,
                UnitCode = unitCode
            };

            return await _getLocationsByDtoAtomicHandler.Handle(atomicRequest);
        }

        private async Task<HttpResponseSnapshot> GetInstructionSetsByDtoFromDownstream(string sessionId, string categoryName, string subCategory)
        {
            GetInstructionSetsByDtoHandlerReqDTO atomicRequest = new GetInstructionSetsByDtoHandlerReqDTO
            {
                SessionId = sessionId,
                CategoryName = categoryName,
                SubCategory = subCategory
            };

            return await _getInstructionSetsByDtoAtomicHandler.Handle(atomicRequest);
        }

        private async Task<HttpResponseSnapshot> CreateBreakdownTaskInDownstream(
            string sessionId,
            CreateBreakdownTaskReqDTO request,
            string locationId,
            string buildingId,
            string instructionId,
            string scheduledDateUtc,
            string raisedDateUtc)
        {
            CreateBreakdownTaskHandlerReqDTO atomicRequest = new CreateBreakdownTaskHandlerReqDTO
            {
                SessionId = sessionId,
                ReporterName = request.ReporterName,
                ReporterEmail = request.ReporterEmail,
                ReporterPhoneNumber = request.ReporterPhoneNumber,
                CallId = request.ServiceRequestNumber,
                CategoryId = string.Empty, // TODO: Map from categoryName
                DisciplineId = string.Empty, // TODO: Map from subCategory
                PriorityId = string.Empty, // TODO: Map from priority
                BuildingId = buildingId,
                LocationId = locationId,
                InstructionId = instructionId,
                LongDescription = request.Description,
                ScheduledDateUtc = scheduledDateUtc,
                RaisedDateUtc = raisedDateUtc,
                ContractId = _appConfigs.ContractId,
                CallerSourceId = _appConfigs.CallerSourceId
            };

            return await _createBreakdownTaskAtomicHandler.Handle(atomicRequest);
        }

        private string FormatScheduledDate(string scheduledDate, string scheduledTimeStart)
        {
            if (string.IsNullOrWhiteSpace(scheduledDate) || string.IsNullOrWhiteSpace(scheduledTimeStart))
            {
                return string.Empty;
            }
            else
            {
                try
                {
                    string fullDateTime = $"{scheduledDate}T{scheduledTimeStart}Z";
                    DateTime dateTime = DateTime.Parse(fullDateTime, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
                    string formattedDate = dateTime.ToString("yyyy-MM-ddTHH:mm:ss.0208713Z");
                    return formattedDate;
                }
                catch (Exception ex)
                {
                    _logger.Warn($"Failed to format scheduled date: {ex.Message} - Using empty value");
                    return string.Empty;
                }
            }
        }

        private string FormatRaisedDate(string raisedDateUtc)
        {
            if (string.IsNullOrWhiteSpace(raisedDateUtc))
            {
                return DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.0208713Z");
            }
            else
            {
                try
                {
                    DateTime dateTime = DateTime.Parse(raisedDateUtc, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
                    string formattedDate = dateTime.ToString("yyyy-MM-ddTHH:mm:ss.0208713Z");
                    return formattedDate;
                }
                catch (Exception ex)
                {
                    _logger.Warn($"Failed to format raised date: {ex.Message} - Using current UTC time");
                    return DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.0208713Z");
                }
            }
        }

        private GetLocationsByDtoApiResDTO? DeserializeGetLocationsResponse(string xmlContent)
        {
            try
            {
                XDocument xdoc = XDocument.Parse(xmlContent);
                XNamespace soapNs = "http://schemas.xmlsoap.org/soap/envelope/";
                XNamespace ns = "http://www.fsi.co.uk/services/evolution/04/09";

                XElement? body = xdoc.Root?.Element(soapNs + "Body");
                XElement? response = body?.Element(ns + "GetLocationsByDtoResponse");
                XElement? result = response?.Element(ns + "GetLocationsByDtoResult");
                XElement? location = result?.Element(ns + "Location");

                if (location == null)
                {
                    return null;
                }
                else
                {
                    return new GetLocationsByDtoApiResDTO
                    {
                        LocationId = location.Element(ns + "LocationId")?.Value,
                        BuildingId = location.Element(ns + "BuildingId")?.Value,
                        PropertyName = location.Element(ns + "PropertyName")?.Value,
                        UnitCode = location.Element(ns + "UnitCode")?.Value
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to deserialize GetLocationsByDto SOAP response");
                return null;
            }
        }

        private GetInstructionSetsByDtoApiResDTO? DeserializeGetInstructionsResponse(string xmlContent)
        {
            try
            {
                XDocument xdoc = XDocument.Parse(xmlContent);
                XNamespace soapNs = "http://schemas.xmlsoap.org/soap/envelope/";
                XNamespace ns = "http://www.fsi.co.uk/services/evolution/04/09";

                XElement? body = xdoc.Root?.Element(soapNs + "Body");
                XElement? response = body?.Element(ns + "GetInstructionSetsByDtoResponse");
                XElement? result = response?.Element(ns + "GetInstructionSetsByDtoResult");
                XElement? instruction = result?.Element(ns + "Instruction");

                if (instruction == null)
                {
                    return null;
                }
                else
                {
                    return new GetInstructionSetsByDtoApiResDTO
                    {
                        InstructionId = instruction.Element(ns + "InstructionId")?.Value,
                        CategoryName = instruction.Element(ns + "CategoryName")?.Value,
                        SubCategory = instruction.Element(ns + "SubCategory")?.Value
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to deserialize GetInstructionSetsByDto SOAP response");
                return null;
            }
        }

        private CreateBreakdownTaskApiResDTO? DeserializeCreateBreakdownTaskResponse(string xmlContent)
        {
            try
            {
                XDocument xdoc = XDocument.Parse(xmlContent);
                XNamespace soapNs = "http://schemas.xmlsoap.org/soap/envelope/";
                XNamespace ns = "http://www.fsi.co.uk/services/evolution/04/09";

                XElement? body = xdoc.Root?.Element(soapNs + "Body");
                XElement? response = body?.Element(ns + "CreateBreakdownTaskResponse");
                XElement? result = response?.Element(ns + "CreateBreakdownTaskResult");

                if (result == null)
                {
                    return null;
                }
                else
                {
                    return new CreateBreakdownTaskApiResDTO
                    {
                        TaskId = result.Element(ns + "TaskId")?.Value,
                        Status = result.Element(ns + "Status")?.Value
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to deserialize CreateBreakdownTask SOAP response");
                throw new NoResponseBodyException(
                    error: ErrorConstants.FSI_TSKCRT_0002,
                    errorDetails: new List<string> { $"Failed to parse SOAP response: {ex.Message}" },
                    stepName: "CreateBreakdownTaskHandler.cs / DeserializeCreateBreakdownTaskResponse"
                );
            }
        }
    }
}
