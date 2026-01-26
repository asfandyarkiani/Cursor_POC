using CAFMSystem.Constants;
using CAFMSystem.DTO.AtomicHandlerDTOs;
using CAFMSystem.DTO.DownstreamDTOs;
using CAFMSystem.DTO.HandlerDTOs.CreateWorkOrderDTO;
using CAFMSystem.Helpers;
using CAFMSystem.Implementations.FSI.AtomicHandlers;
using CAFMSystem.Middleware;
using Core.DTOs;
using Core.Exceptions;
using Core.Extensions;
using Core.SystemLayer.Exceptions;
using Core.SystemLayer.Handlers;
using Microsoft.Extensions.Logging;
using System.Net;

namespace CAFMSystem.Implementations.FSI.Handlers
{
    /// <summary>
    /// Handler for creating work orders in CAFM system.
    /// Orchestrates multiple atomic handlers internally (same SOR):
    /// - GetLocationsByDto (lookup BuildingId, LocationId)
    /// - GetInstructionSetsByDto (lookup CategoryId, DisciplineId, PriorityId, InstructionId)
    /// - GetBreakdownTasksByDto (check if task exists)
    /// - CreateBreakdownTask (create task)
    /// - CreateEvent (conditionally link event if recurrence = Y)
    /// </summary>
    public class CreateWorkOrderHandler : IBaseHandler<CreateWorkOrderReqDTO>
    {
        private readonly ILogger<CreateWorkOrderHandler> _logger;
        private readonly GetLocationsByDtoAtomicHandler _getLocationsByDtoAtomicHandler;
        private readonly GetInstructionSetsByDtoAtomicHandler _getInstructionSetsByDtoAtomicHandler;
        private readonly CreateBreakdownTaskAtomicHandler _createBreakdownTaskAtomicHandler;
        private readonly GetBreakdownTasksByDtoAtomicHandler _getBreakdownTasksByDtoAtomicHandler;
        private readonly CreateEventAtomicHandler _createEventAtomicHandler;

        public CreateWorkOrderHandler(
            ILogger<CreateWorkOrderHandler> logger,
            GetLocationsByDtoAtomicHandler getLocationsByDtoAtomicHandler,
            GetInstructionSetsByDtoAtomicHandler getInstructionSetsByDtoAtomicHandler,
            CreateBreakdownTaskAtomicHandler createBreakdownTaskAtomicHandler,
            GetBreakdownTasksByDtoAtomicHandler getBreakdownTasksByDtoAtomicHandler,
            CreateEventAtomicHandler createEventAtomicHandler)
        {
            _logger = logger;
            _getLocationsByDtoAtomicHandler = getLocationsByDtoAtomicHandler;
            _getInstructionSetsByDtoAtomicHandler = getInstructionSetsByDtoAtomicHandler;
            _createBreakdownTaskAtomicHandler = createBreakdownTaskAtomicHandler;
            _getBreakdownTasksByDtoAtomicHandler = getBreakdownTasksByDtoAtomicHandler;
            _createEventAtomicHandler = createEventAtomicHandler;
        }

        public async Task<BaseResponseDTO> HandleAsync(CreateWorkOrderReqDTO request)
        {
            _logger.Info("[System Layer]-Initiating Create Work Order");

            string? sessionId = RequestContext.GetSessionId();
            if (string.IsNullOrEmpty(sessionId))
            {
                _logger.Error("SessionId not found in RequestContext");
                throw new BaseException(("SYS_AUTHENT_0003", "SessionId not available. Authentication may have failed."));
            }

            List<WorkOrderResultDTO> results = new List<WorkOrderResultDTO>();

            // Process each work order in the array
            foreach (WorkOrderItemDTO workOrder in request.WorkOrders)
            {
                try
                {
                    _logger.Info($"Processing work order: {workOrder.ServiceRequestNumber}");

                    // Step 1: Check if task already exists (check-before-create pattern)
                    _logger.Info($"Checking if task exists for SR: {workOrder.ServiceRequestNumber}");
                    GetBreakdownTasksByDtoHandlerReqDTO checkRequest = new GetBreakdownTasksByDtoHandlerReqDTO
                    {
                        SessionId = sessionId,
                        CallId = workOrder.ServiceRequestNumber
                    };

                    HttpResponseSnapshot checkResponse = await _getBreakdownTasksByDtoAtomicHandler.Handle(checkRequest);
                    
                    if (checkResponse.IsSuccessStatusCode)
                    {
                        GetBreakdownTasksByDtoApiResDTO? checkResult = SOAPHelper.DeserializeSoapResponse<GetBreakdownTasksByDtoApiResDTO>(checkResponse.Content!);
                        
                        // If CallId is empty, task doesn't exist - skip creation
                        if (string.IsNullOrWhiteSpace(checkResult?.CallId))
                        {
                            _logger.Info($"Task already exists for SR: {workOrder.ServiceRequestNumber}, skipping creation");
                            results.Add(WorkOrderResultDTO.MapError(
                                workOrder.ServiceRequestNumber,
                                workOrder.SourceOrgId,
                                "Task already exists in CAFM system"));
                            continue;
                        }
                    }

                    // Step 2: Get location details (internal lookup)
                    _logger.Info($"Getting location details for UnitCode: {workOrder.UnitCode}");
                    GetLocationsByDtoHandlerReqDTO locationRequest = new GetLocationsByDtoHandlerReqDTO
                    {
                        SessionId = sessionId,
                        BarCode = workOrder.UnitCode
                    };

                    HttpResponseSnapshot locationResponse = await _getLocationsByDtoAtomicHandler.Handle(locationRequest);
                    
                    if (!locationResponse.IsSuccessStatusCode)
                    {
                        _logger.Error($"Failed to get location details: {locationResponse.StatusCode}");
                        throw new DownStreamApiFailureException(
                            statusCode: (HttpStatusCode)locationResponse.StatusCode,
                            error: ErrorConstants.SYS_LOCGET_0001,
                            errorDetails: new List<string> { $"Failed to get location. Status: {locationResponse.StatusCode}. Response: {locationResponse.Content}" },
                            stepName: "CreateWorkOrderHandler.cs / HandleAsync / GetLocationsByDto");
                    }

                    GetLocationsByDtoApiResDTO? locationData = SOAPHelper.DeserializeSoapResponse<GetLocationsByDtoApiResDTO>(locationResponse.Content!);
                    
                    if (locationData == null || string.IsNullOrWhiteSpace(locationData.BuildingId) || string.IsNullOrWhiteSpace(locationData.LocationId))
                    {
                        _logger.Error($"Location not found for UnitCode: {workOrder.UnitCode}");
                        throw new NotFoundException(
                            error: ErrorConstants.SYS_LOCGET_0002,
                            errorDetails: new List<string> { $"Location not found for UnitCode: {workOrder.UnitCode}" },
                            stepName: "CreateWorkOrderHandler.cs / HandleAsync / GetLocationsByDto");
                    }

                    // Step 3: Get instruction set details (internal lookup)
                    _logger.Info($"Getting instruction set details for SubCategory: {workOrder.SubCategory}");
                    GetInstructionSetsByDtoHandlerReqDTO instructionRequest = new GetInstructionSetsByDtoHandlerReqDTO
                    {
                        SessionId = sessionId,
                        InstructionDescription = workOrder.SubCategory
                    };

                    HttpResponseSnapshot instructionResponse = await _getInstructionSetsByDtoAtomicHandler.Handle(instructionRequest);
                    
                    if (!instructionResponse.IsSuccessStatusCode)
                    {
                        _logger.Error($"Failed to get instruction set: {instructionResponse.StatusCode}");
                        throw new DownStreamApiFailureException(
                            statusCode: (HttpStatusCode)instructionResponse.StatusCode,
                            error: ErrorConstants.SYS_INSGET_0001,
                            errorDetails: new List<string> { $"Failed to get instruction set. Status: {instructionResponse.StatusCode}. Response: {instructionResponse.Content}" },
                            stepName: "CreateWorkOrderHandler.cs / HandleAsync / GetInstructionSetsByDto");
                    }

                    GetInstructionSetsByDtoApiResDTO? instructionData = SOAPHelper.DeserializeSoapResponse<GetInstructionSetsByDtoApiResDTO>(instructionResponse.Content!);
                    
                    if (instructionData == null || string.IsNullOrWhiteSpace(instructionData.IN_SEQ))
                    {
                        _logger.Error($"Instruction set not found for SubCategory: {workOrder.SubCategory}");
                        throw new NotFoundException(
                            error: ErrorConstants.SYS_INSGET_0002,
                            errorDetails: new List<string> { $"Instruction set not found for SubCategory: {workOrder.SubCategory}" },
                            stepName: "CreateWorkOrderHandler.cs / HandleAsync / GetInstructionSetsByDto");
                    }

                    // Step 4: Create breakdown task (aggregate all lookups)
                    _logger.Info($"Creating breakdown task for SR: {workOrder.ServiceRequestNumber}");
                    CreateBreakdownTaskHandlerReqDTO taskRequest = new CreateBreakdownTaskHandlerReqDTO
                    {
                        SessionId = sessionId,
                        CallerSourceId = workOrder.ServiceRequestNumber,
                        Comments = workOrder.Description,
                        ContactEmail = workOrder.ReporterEmail,
                        ContactName = workOrder.ReporterName,
                        ContactPhone = workOrder.ReporterPhoneNumber,
                        BuildingId = locationData.BuildingId!,
                        CategoryId = instructionData.IN_FKEY_CAT_SEQ ?? string.Empty,
                        DisciplineId = instructionData.IN_FKEY_LAB_SEQ ?? string.Empty,
                        LocationId = locationData.LocationId!,
                        PriorityId = instructionData.IN_FKEY_PRI_SEQ ?? string.Empty,
                        LoggedBy = "EQARCOM+",
                        RaisedDate = workOrder.TicketDetails?.RaisedDateUtc ?? string.Empty,
                        ScheduledDate = workOrder.TicketDetails?.ScheduledDate ?? string.Empty,
                        ScheduledEndTime = workOrder.TicketDetails?.ScheduledTimeEnd ?? string.Empty,
                        ScheduledStartTime = workOrder.TicketDetails?.ScheduledTimeStart ?? string.Empty,
                        Status = workOrder.TicketDetails?.Status ?? string.Empty,
                        SubStatus = workOrder.TicketDetails?.SubStatus ?? string.Empty,
                        ContractId = "TODO_CONTRACT_ID", // TODO: Get from configuration
                        InstructionId = instructionData.IN_SEQ!
                    };

                    HttpResponseSnapshot taskResponse = await _createBreakdownTaskAtomicHandler.Handle(taskRequest);
                    
                    if (!taskResponse.IsSuccessStatusCode)
                    {
                        _logger.Error($"Failed to create breakdown task: {taskResponse.StatusCode}");
                        throw new DownStreamApiFailureException(
                            statusCode: (HttpStatusCode)taskResponse.StatusCode,
                            error: ErrorConstants.SYS_TSKCRT_0001,
                            errorDetails: new List<string> { $"Failed to create breakdown task. Status: {taskResponse.StatusCode}. Response: {taskResponse.Content}" },
                            stepName: "CreateWorkOrderHandler.cs / HandleAsync / CreateBreakdownTask");
                    }

                    CreateBreakdownTaskApiResDTO? taskData = SOAPHelper.DeserializeSoapResponse<CreateBreakdownTaskApiResDTO>(taskResponse.Content!);
                    
                    if (taskData == null || string.IsNullOrWhiteSpace(taskData.TaskId))
                    {
                        _logger.Error("CreateBreakdownTask succeeded but no TaskId returned");
                        throw new NoResponseBodyException(
                            errorDetails: new List<string> { "CreateBreakdownTask succeeded but no TaskId in response" },
                            stepName: "CreateWorkOrderHandler.cs / HandleAsync / CreateBreakdownTask");
                    }

                    // Step 5: Conditionally create event (only if recurrence = Y)
                    if (!string.IsNullOrWhiteSpace(workOrder.TicketDetails?.Recurrence) && 
                        workOrder.TicketDetails.Recurrence.Equals("Y", StringComparison.OrdinalIgnoreCase))
                    {
                        _logger.Info($"Creating event for recurring task: {taskData.TaskId}");
                        CreateEventHandlerReqDTO eventRequest = new CreateEventHandlerReqDTO
                        {
                            SessionId = sessionId,
                            Comments = workOrder.TicketDetails?.OldCAFMSRnumber ?? string.Empty,
                            TaskId = taskData.TaskId
                        };

                        HttpResponseSnapshot eventResponse = await _createEventAtomicHandler.Handle(eventRequest);
                        
                        if (!eventResponse.IsSuccessStatusCode)
                        {
                            _logger.Warn($"Failed to create event but task created successfully: {eventResponse.StatusCode}");
                            // Continue - task was created successfully even if event failed
                        }
                        else
                        {
                            _logger.Info("Event created successfully for recurring task");
                        }
                    }

                    // Map success result
                    results.Add(WorkOrderResultDTO.MapSuccess(
                        taskData,
                        workOrder.ServiceRequestNumber,
                        workOrder.SourceOrgId));

                    _logger.Info($"Work order processed successfully: {workOrder.ServiceRequestNumber}");
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, $"Error processing work order: {workOrder.ServiceRequestNumber}");
                    results.Add(WorkOrderResultDTO.MapError(
                        workOrder.ServiceRequestNumber,
                        workOrder.SourceOrgId,
                        ex.Message));
                }
            }

            _logger.Info("[System Layer]-Completed Create Work Order");

            return new BaseResponseDTO(
                message: InfoConstants.CREATE_WORK_ORDER_SUCCESS,
                data: CreateWorkOrderResDTO.Map(results),
                errorCode: null);
        }
    }
}
