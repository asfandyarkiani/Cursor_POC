using Core.DTOs;
using Core.Exceptions;
using Core.Extensions;
using Core.SystemLayer.Exceptions;
using Core.SystemLayer.Handlers;
using FsiCafmSystem.ConfigModels;
using FsiCafmSystem.Constants;
using FsiCafmSystem.DTO.AtomicHandlerDTOs;
using FsiCafmSystem.DTO.DownstreamDTOs;
using FsiCafmSystem.DTO.HandlerDTOs.CreateWorkOrderDTO;
using FsiCafmSystem.Helper;
using FsiCafmSystem.Implementations.FsiCafm.AtomicHandlers;
using FsiCafmSystem.Middleware;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;

namespace FsiCafmSystem.Implementations.FsiCafm.Handlers
{
    public class CreateWorkOrderHandler : IBaseHandler<CreateWorkOrderReqDTO>
    {
        private readonly ILogger<CreateWorkOrderHandler> _logger;
        private readonly GetLocationsByDtoAtomicHandler _getLocationsByDtoAtomicHandler;
        private readonly GetInstructionSetsByDtoAtomicHandler _getInstructionSetsByDtoAtomicHandler;
        private readonly GetBreakdownTasksByDtoAtomicHandler _getBreakdownTasksByDtoAtomicHandler;
        private readonly CreateBreakdownTaskAtomicHandler _createBreakdownTaskAtomicHandler;
        private readonly CreateEventAtomicHandler _createEventAtomicHandler;
        private readonly AppConfigs _appConfigs;
        
        public CreateWorkOrderHandler(
            ILogger<CreateWorkOrderHandler> logger,
            GetLocationsByDtoAtomicHandler getLocationsByDtoAtomicHandler,
            GetInstructionSetsByDtoAtomicHandler getInstructionSetsByDtoAtomicHandler,
            GetBreakdownTasksByDtoAtomicHandler getBreakdownTasksByDtoAtomicHandler,
            CreateBreakdownTaskAtomicHandler createBreakdownTaskAtomicHandler,
            CreateEventAtomicHandler createEventAtomicHandler,
            IOptions<AppConfigs> options)
        {
            _logger = logger;
            _getLocationsByDtoAtomicHandler = getLocationsByDtoAtomicHandler;
            _getInstructionSetsByDtoAtomicHandler = getInstructionSetsByDtoAtomicHandler;
            _getBreakdownTasksByDtoAtomicHandler = getBreakdownTasksByDtoAtomicHandler;
            _createBreakdownTaskAtomicHandler = createBreakdownTaskAtomicHandler;
            _createEventAtomicHandler = createEventAtomicHandler;
            _appConfigs = options.Value;
        }
        
        public async Task<BaseResponseDTO> HandleAsync(CreateWorkOrderReqDTO request)
        {
            _logger.Info("[System Layer]-Initiating Create Work Order");
            
            string? sessionId = RequestContext.GetSessionId();
            
            if (string.IsNullOrEmpty(sessionId))
            {
                throw new BusinessCaseFailureException(
                    error: ErrorConstants.FSI_AUTHENT_0002,
                    errorDetails: new List<string> { "SessionId not found in context" },
                    stepName: "CreateWorkOrderHandler / HandleAsync");
            }
            
            List<CreateBreakdownTaskApiResDTO> results = new List<CreateBreakdownTaskApiResDTO>();
            List<string> serviceRequestNumbers = new List<string>();
            
            foreach (WorkOrderItemDTO workOrder in request.WorkOrders)
            {
                try
                {
                    _logger.Info($"Processing work order: {workOrder.ServiceRequestNumber}");
                    
                    // Step 1: Check if task already exists (check-before-create pattern)
                    _logger.Info($"Checking if task exists for: {workOrder.ServiceRequestNumber}");
                    
                    GetBreakdownTasksByDtoHandlerReqDTO checkRequest = new GetBreakdownTasksByDtoHandlerReqDTO
                    {
                        SessionId = sessionId,
                        CallId = workOrder.ServiceRequestNumber
                    };
                    
                    Core.SystemLayer.Middlewares.HttpResponseSnapshot checkResponse = await _getBreakdownTasksByDtoAtomicHandler.Handle(checkRequest);
                    
                    if (checkResponse.IsSuccessStatusCode)
                    {
                        GetBreakdownTasksByDtoApiResDTO? checkData = SOAPHelper.DeserializeSoapResponse<GetBreakdownTasksByDtoApiResDTO>(checkResponse.Content!);
                        
                        // If CallId is not empty, task already exists - skip creation
                        if (checkData?.BreakdownTaskDtoV3 != null && checkData.BreakdownTaskDtoV3.Any() && !string.IsNullOrEmpty(checkData.BreakdownTaskDtoV3[0].CallId))
                        {
                            _logger.Info($"Task already exists for {workOrder.ServiceRequestNumber}, skipping creation");
                            serviceRequestNumbers.Add(workOrder.ServiceRequestNumber);
                            results.Add(new CreateBreakdownTaskApiResDTO { TaskId = checkData.BreakdownTaskDtoV3[0].TaskId });
                            continue;
                        }
                    }
                    
                    // Step 2: Get location details (parallel with instruction sets)
                    _logger.Info($"Getting location details for UnitCode: {workOrder.UnitCode}");
                    
                    GetLocationsByDtoHandlerReqDTO locationRequest = new GetLocationsByDtoHandlerReqDTO
                    {
                        SessionId = sessionId,
                        UnitCode = workOrder.UnitCode
                    };
                    
                    Core.SystemLayer.Middlewares.HttpResponseSnapshot locationResponse = await _getLocationsByDtoAtomicHandler.Handle(locationRequest);
                    
                    if (!locationResponse.IsSuccessStatusCode)
                    {
                        _logger.Error($"GetLocationsByDto failed: {locationResponse.StatusCode}");
                        throw new DownStreamApiFailureException(
                            statusCode: (HttpStatusCode)locationResponse.StatusCode,
                            error: ErrorConstants.FSI_LOCGET_0001,
                            errorDetails: new List<string> { $"Failed to get location. Response: {locationResponse.Content}" },
                            stepName: "CreateWorkOrderHandler / HandleAsync / GetLocationsByDto");
                    }
                    
                    GetLocationsByDtoApiResDTO? locationData = SOAPHelper.DeserializeSoapResponse<GetLocationsByDtoApiResDTO>(locationResponse.Content!);
                    
                    if (locationData?.LocationDto == null || !locationData.LocationDto.Any())
                    {
                        throw new NotFoundException(
                            error: ErrorConstants.FSI_LOCGET_0002,
                            errorDetails: new List<string> { $"Location not found for UnitCode: {workOrder.UnitCode}" },
                            stepName: "CreateWorkOrderHandler / HandleAsync / GetLocationsByDto");
                    }
                    
                    LocationDtoItem location = locationData.LocationDto[0];
                    
                    // Step 3: Get instruction sets (parallel with location)
                    _logger.Info($"Getting instruction sets for SubCategory: {workOrder.SubCategory}");
                    
                    GetInstructionSetsByDtoHandlerReqDTO instructionRequest = new GetInstructionSetsByDtoHandlerReqDTO
                    {
                        SessionId = sessionId,
                        Description = workOrder.SubCategory
                    };
                    
                    Core.SystemLayer.Middlewares.HttpResponseSnapshot instructionResponse = await _getInstructionSetsByDtoAtomicHandler.Handle(instructionRequest);
                    
                    if (!instructionResponse.IsSuccessStatusCode)
                    {
                        _logger.Error($"GetInstructionSetsByDto failed: {instructionResponse.StatusCode}");
                        throw new DownStreamApiFailureException(
                            statusCode: (HttpStatusCode)instructionResponse.StatusCode,
                            error: ErrorConstants.FSI_INSGET_0001,
                            errorDetails: new List<string> { $"Failed to get instruction sets. Response: {instructionResponse.Content}" },
                            stepName: "CreateWorkOrderHandler / HandleAsync / GetInstructionSetsByDto");
                    }
                    
                    GetInstructionSetsByDtoApiResDTO? instructionData = SOAPHelper.DeserializeSoapResponse<GetInstructionSetsByDtoApiResDTO>(instructionResponse.Content!);
                    
                    if (instructionData?.FINFILEDto == null || !instructionData.FINFILEDto.Any())
                    {
                        throw new NotFoundException(
                            error: ErrorConstants.FSI_INSGET_0002,
                            errorDetails: new List<string> { $"Instruction set not found for SubCategory: {workOrder.SubCategory}" },
                            stepName: "CreateWorkOrderHandler / HandleAsync / GetInstructionSetsByDto");
                    }
                    
                    FINFILEDtoItem instruction = instructionData.FINFILEDto[0];
                    
                    // Step 4: Create breakdown task
                    _logger.Info($"Creating breakdown task for: {workOrder.ServiceRequestNumber}");
                    
                    // Build ScheduledDateUtc from ScheduledDate + ScheduledTimeStart (matching Boomi scripting function)
                    string scheduledDateUtc = string.Empty;
                    if (!string.IsNullOrWhiteSpace(workOrder.TicketDetails?.ScheduledDate) && 
                        !string.IsNullOrWhiteSpace(workOrder.TicketDetails?.ScheduledTimeStart))
                    {
                        scheduledDateUtc = $"{workOrder.TicketDetails.ScheduledDate}T{workOrder.TicketDetails.ScheduledTimeStart}Z";
                    }
                    
                    CreateBreakdownTaskHandlerReqDTO createRequest = new CreateBreakdownTaskHandlerReqDTO
                    {
                        SessionId = sessionId,
                        BuildingId = location.BuildingId ?? string.Empty,
                        LocationId = location.LocationId ?? string.Empty,
                        CategoryId = instruction.IN_FKEY_CAT_SEQ ?? string.Empty,
                        DisciplineId = instruction.IN_FKEY_LAB_SEQ ?? string.Empty,
                        PriorityId = instruction.IN_FKEY_PRI_SEQ ?? string.Empty,
                        InstructionId = instruction.IN_SEQ ?? string.Empty,
                        Description = workOrder.Description,
                        ReporterName = workOrder.ReporterName,
                        ReporterEmail = workOrder.ReporterEmail,
                        ReporterPhoneNumber = workOrder.ReporterPhoneNumber,
                        ServiceRequestNumber = workOrder.ServiceRequestNumber,
                        PropertyName = workOrder.PropertyName,
                        Technician = workOrder.Technician,
                        SourceOrgId = workOrder.SourceOrgId,
                        Status = workOrder.TicketDetails?.Status ?? string.Empty,
                        SubStatus = workOrder.TicketDetails?.SubStatus ?? string.Empty,
                        Priority = workOrder.TicketDetails?.Priority ?? string.Empty,
                        ScheduledDate = workOrder.TicketDetails?.ScheduledDate ?? string.Empty,
                        ScheduledTimeStart = workOrder.TicketDetails?.ScheduledTimeStart ?? string.Empty,
                        ScheduledTimeEnd = workOrder.TicketDetails?.ScheduledTimeEnd ?? string.Empty,
                        ScheduledDateUtc = scheduledDateUtc,
                        RaisedDateUtc = workOrder.TicketDetails?.RaisedDateUtc ?? string.Empty,
                        ContractId = _appConfigs.ContractId ?? string.Empty
                    };
                    
                    Core.SystemLayer.Middlewares.HttpResponseSnapshot createResponse = await _createBreakdownTaskAtomicHandler.Handle(createRequest);
                    
                    if (!createResponse.IsSuccessStatusCode)
                    {
                        _logger.Error($"CreateBreakdownTask failed: {createResponse.StatusCode}");
                        throw new DownStreamApiFailureException(
                            statusCode: (HttpStatusCode)createResponse.StatusCode,
                            error: ErrorConstants.FSI_TSKCRT_0001,
                            errorDetails: new List<string> { $"Failed to create breakdown task. Response: {createResponse.Content}" },
                            stepName: "CreateWorkOrderHandler / HandleAsync / CreateBreakdownTask");
                    }
                    
                    CreateBreakdownTaskApiResDTO? createData = SOAPHelper.DeserializeSoapResponse<CreateBreakdownTaskApiResDTO>(createResponse.Content!);
                    
                    if (createData == null || string.IsNullOrEmpty(createData.TaskId))
                    {
                        throw new BusinessCaseFailureException(
                            error: ErrorConstants.FSI_WOCRT_0002,
                            errorDetails: new List<string> { "CreateBreakdownTask succeeded but no TaskId returned" },
                            stepName: "CreateWorkOrderHandler / HandleAsync / CreateBreakdownTask");
                    }
                    
                    // Step 5: If recurring, create event and link to task
                    if (workOrder.TicketDetails?.Recurrence == "Y")
                    {
                        _logger.Info($"Creating event for recurring task: {createData.TaskId}");
                        
                        CreateEventHandlerReqDTO eventRequest = new CreateEventHandlerReqDTO
                        {
                            SessionId = sessionId,
                            TaskId = createData.TaskId,
                            Comments = workOrder.TicketDetails?.OldCAFMSRNumber ?? string.Empty
                        };
                        
                        Core.SystemLayer.Middlewares.HttpResponseSnapshot eventResponse = await _createEventAtomicHandler.Handle(eventRequest);
                        
                        if (!eventResponse.IsSuccessStatusCode)
                        {
                            _logger.Warn($"CreateEvent failed: {eventResponse.StatusCode}. Continuing with task creation.");
                        }
                        else
                        {
                            _logger.Info("Event created and linked successfully");
                        }
                    }
                    
                    serviceRequestNumbers.Add(workOrder.ServiceRequestNumber);
                    results.Add(createData);
                    
                    _logger.Info($"Work order processed successfully: {workOrder.ServiceRequestNumber}");
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, $"Failed to process work order: {workOrder.ServiceRequestNumber}");
                    
                    serviceRequestNumbers.Add(workOrder.ServiceRequestNumber);
                    results.Add(new CreateBreakdownTaskApiResDTO { TaskId = null });
                }
            }
            
            _logger.Info("[System Layer]-Completed Create Work Order");
            
            return new BaseResponseDTO(
                message: InfoConstants.CREATE_WORK_ORDER_SUCCESS,
                data: CreateWorkOrderResDTO.Map(results, serviceRequestNumbers),
                errorCode: null);
        }
    }
}
