using CAFMSystemLayer.Constants;
using CAFMSystemLayer.DTO.AtomicHandlerDTOs;
using CAFMSystemLayer.DTO.DownstreamDTOs;
using CAFMSystemLayer.DTO.HandlerDTOs.CreateWorkOrderDTO;
using CAFMSystemLayer.Helper;
using CAFMSystemLayer.Implementations.CAFM.AtomicHandlers;
using Core.DTOs;
using Core.Extensions;
using Core.SystemLayer.Exceptions;
using Core.SystemLayer.Handlers;
using Microsoft.Extensions.Logging;
using System.Net;

namespace CAFMSystemLayer.Implementations.CAFM.Handlers
{
    public class CreateWorkOrderHandler : IBaseHandler<CreateWorkOrderReqDTO>
    {
        private readonly ILogger<CreateWorkOrderHandler> _logger;
        private readonly GetLocationsAtomicHandler _getLocationsAtomicHandler;
        private readonly GetInstructionSetsAtomicHandler _getInstructionSetsAtomicHandler;
        private readonly GetBreakdownTasksAtomicHandler _getBreakdownTasksAtomicHandler;
        private readonly CreateBreakdownTaskAtomicHandler _createBreakdownTaskAtomicHandler;
        private readonly CreateEventLinkTaskAtomicHandler _createEventLinkTaskAtomicHandler;
        
        public CreateWorkOrderHandler(
            ILogger<CreateWorkOrderHandler> logger,
            GetLocationsAtomicHandler getLocationsAtomicHandler,
            GetInstructionSetsAtomicHandler getInstructionSetsAtomicHandler,
            GetBreakdownTasksAtomicHandler getBreakdownTasksAtomicHandler,
            CreateBreakdownTaskAtomicHandler createBreakdownTaskAtomicHandler,
            CreateEventLinkTaskAtomicHandler createEventLinkTaskAtomicHandler)
        {
            _logger = logger;
            _getLocationsAtomicHandler = getLocationsAtomicHandler;
            _getInstructionSetsAtomicHandler = getInstructionSetsAtomicHandler;
            _getBreakdownTasksAtomicHandler = getBreakdownTasksAtomicHandler;
            _createBreakdownTaskAtomicHandler = createBreakdownTaskAtomicHandler;
            _createEventLinkTaskAtomicHandler = createEventLinkTaskAtomicHandler;
        }
        
        public async Task<BaseResponseDTO> HandleAsync(CreateWorkOrderReqDTO request)
        {
            _logger.Info("[System Layer]-Initiating Create Work Order");
            
            string? sessionId = RequestContext.GetSessionId();
            if (string.IsNullOrEmpty(sessionId))
            {
                _logger.Error("SessionId not found in RequestContext");
                throw new Core.Exceptions.BaseException(ErrorConstants.CAFM_AUTHENT_0002);
            }
            
            CreateWorkOrderResDTO responseDto = CreateWorkOrderResDTO.CreateEmpty();
            
            // Process each work order in the array
            foreach (WorkOrderItemDTO workOrder in request.WorkOrders)
            {
                WorkOrderResultDTO result = new WorkOrderResultDTO
                {
                    ServiceRequestNumber = workOrder.ServiceRequestNumber,
                    Status = "Processing"
                };
                
                try
                {
                    _logger.Info($"Processing work order: {workOrder.ServiceRequestNumber}");
                    
                    // Step 1: Get Location ID
                    GetLocationsHandlerReqDTO locationRequest = new GetLocationsHandlerReqDTO
                    {
                        SessionId = sessionId,
                        PropertyName = workOrder.PropertyName,
                        UnitCode = workOrder.UnitCode
                    };
                    
                    HttpResponseSnapshot locationResponse = await _getLocationsAtomicHandler.Handle(locationRequest);
                    
                    if (!locationResponse.IsSuccessStatusCode)
                    {
                        _logger.Error($"GetLocations failed: {locationResponse.StatusCode}");
                        throw new DownStreamApiFailureException(
                            statusCode: (HttpStatusCode)locationResponse.StatusCode,
                            error: ErrorConstants.CAFM_LOCGET_0001,
                            errorDetails: [$"Status: {locationResponse.StatusCode}. Response: {locationResponse.Content}"],
                            stepName: "CreateWorkOrderHandler.cs / HandleAsync / GetLocations"
                        );
                    }
                    
                    GetLocationsApiResDTO? locationData = SOAPHelper.DeserializeSoapResponse<GetLocationsApiResDTO>(locationResponse.Content!);
                    
                    if (locationData == null || string.IsNullOrEmpty(locationData.LocationId))
                    {
                        throw new Core.Exceptions.NotFoundException(
                            ErrorConstants.CAFM_LOCGET_0002,
                            [$"No location found for Property: {workOrder.PropertyName}, Unit: {workOrder.UnitCode}"],
                            "CreateWorkOrderHandler.cs / HandleAsync / GetLocations"
                        );
                    }
                    
                    result.LocationId = locationData.LocationId;
                    _logger.Info($"Location found: {locationData.LocationId}");
                    
                    // Step 2: Get Instruction Set ID
                    GetInstructionSetsHandlerReqDTO instructionRequest = new GetInstructionSetsHandlerReqDTO
                    {
                        SessionId = sessionId,
                        CategoryName = workOrder.CategoryName,
                        SubCategory = workOrder.SubCategory
                    };
                    
                    HttpResponseSnapshot instructionResponse = await _getInstructionSetsAtomicHandler.Handle(instructionRequest);
                    
                    if (!instructionResponse.IsSuccessStatusCode)
                    {
                        _logger.Error($"GetInstructionSets failed: {instructionResponse.StatusCode}");
                        throw new DownStreamApiFailureException(
                            statusCode: (HttpStatusCode)instructionResponse.StatusCode,
                            error: ErrorConstants.CAFM_INSGET_0001,
                            errorDetails: [$"Status: {instructionResponse.StatusCode}. Response: {instructionResponse.Content}"],
                            stepName: "CreateWorkOrderHandler.cs / HandleAsync / GetInstructionSets"
                        );
                    }
                    
                    GetInstructionSetsApiResDTO? instructionData = SOAPHelper.DeserializeSoapResponse<GetInstructionSetsApiResDTO>(instructionResponse.Content!);
                    
                    if (instructionData == null || string.IsNullOrEmpty(instructionData.InstructionSetId))
                    {
                        throw new Core.Exceptions.NotFoundException(
                            ErrorConstants.CAFM_INSGET_0002,
                            [$"No instruction set found for Category: {workOrder.CategoryName}, SubCategory: {workOrder.SubCategory}"],
                            "CreateWorkOrderHandler.cs / HandleAsync / GetInstructionSets"
                        );
                    }
                    
                    result.InstructionSetId = instructionData.InstructionSetId;
                    _logger.Info($"Instruction set found: {instructionData.InstructionSetId}");
                    
                    // Step 3: Check if task already exists
                    GetBreakdownTasksHandlerReqDTO checkTaskRequest = new GetBreakdownTasksHandlerReqDTO
                    {
                        SessionId = sessionId,
                        ServiceRequestNumber = workOrder.ServiceRequestNumber
                    };
                    
                    HttpResponseSnapshot checkTaskResponse = await _getBreakdownTasksAtomicHandler.Handle(checkTaskRequest);
                    
                    if (!checkTaskResponse.IsSuccessStatusCode)
                    {
                        _logger.Error($"GetBreakdownTasks failed: {checkTaskResponse.StatusCode}");
                        throw new DownStreamApiFailureException(
                            statusCode: (HttpStatusCode)checkTaskResponse.StatusCode,
                            error: ErrorConstants.CAFM_TSKCHK_0001,
                            errorDetails: [$"Status: {checkTaskResponse.StatusCode}. Response: {checkTaskResponse.Content}"],
                            stepName: "CreateWorkOrderHandler.cs / HandleAsync / CheckTaskExists"
                        );
                    }
                    
                    GetBreakdownTasksApiResDTO? existingTaskData = SOAPHelper.DeserializeSoapResponse<GetBreakdownTasksApiResDTO>(checkTaskResponse.Content!);
                    
                    // Step 4: Check-before-create pattern (same SOR - Handler orchestrates with if/else)
                    if (existingTaskData != null && !string.IsNullOrEmpty(existingTaskData.TaskId))
                    {
                        // Task already exists - skip creation
                        _logger.Info($"Task already exists for ServiceRequestNumber: {workOrder.ServiceRequestNumber}, TaskId: {existingTaskData.TaskId}");
                        result.TaskId = existingTaskData.TaskId;
                        result.Status = "Skipped";
                        result.AlreadyExists = true;
                        result.Message = "Task already exists in CAFM";
                        responseDto.SkippedCount++;
                    }
                    else
                    {
                        // Task doesn't exist - create new task
                        _logger.Info($"Creating new task for ServiceRequestNumber: {workOrder.ServiceRequestNumber}");
                        
                        CreateBreakdownTaskHandlerReqDTO createTaskRequest = new CreateBreakdownTaskHandlerReqDTO
                        {
                            SessionId = sessionId,
                            LocationId = locationData.LocationId!,
                            InstructionSetId = instructionData.InstructionSetId!,
                            Description = workOrder.Description,
                            ServiceRequestNumber = workOrder.ServiceRequestNumber,
                            ReporterName = workOrder.ReporterName,
                            ReporterEmail = workOrder.ReporterEmail,
                            ReporterPhoneNumber = workOrder.ReporterPhoneNumber,
                            Status = workOrder.TicketDetails?.Status ?? string.Empty,
                            SubStatus = workOrder.TicketDetails?.SubStatus ?? string.Empty,
                            Priority = workOrder.TicketDetails?.Priority ?? string.Empty,
                            ScheduledDate = workOrder.TicketDetails?.ScheduledDate ?? string.Empty,
                            ScheduledTimeStart = workOrder.TicketDetails?.ScheduledTimeStart ?? string.Empty,
                            ScheduledTimeEnd = workOrder.TicketDetails?.ScheduledTimeEnd ?? string.Empty,
                            Technician = workOrder.Technician,
                            SourceOrgId = workOrder.SourceOrgId
                        };
                        
                        HttpResponseSnapshot createTaskResponse = await _createBreakdownTaskAtomicHandler.Handle(createTaskRequest);
                        
                        if (!createTaskResponse.IsSuccessStatusCode)
                        {
                            _logger.Error($"CreateBreakdownTask failed: {createTaskResponse.StatusCode}");
                            throw new DownStreamApiFailureException(
                                statusCode: (HttpStatusCode)createTaskResponse.StatusCode,
                                error: ErrorConstants.CAFM_TSKCRT_0001,
                                errorDetails: [$"Status: {createTaskResponse.StatusCode}. Response: {createTaskResponse.Content}"],
                                stepName: "CreateWorkOrderHandler.cs / HandleAsync / CreateBreakdownTask"
                            );
                        }
                        
                        CreateBreakdownTaskApiResDTO? createTaskData = SOAPHelper.DeserializeSoapResponse<CreateBreakdownTaskApiResDTO>(createTaskResponse.Content!);
                        
                        if (createTaskData == null || string.IsNullOrEmpty(createTaskData.TaskId))
                        {
                            throw new Core.Exceptions.NoResponseBodyException(
                                errorDetails: ["CreateBreakdownTask succeeded but no TaskId returned"],
                                stepName: "CreateWorkOrderHandler.cs / HandleAsync / CreateBreakdownTask"
                            );
                        }
                        
                        result.TaskId = createTaskData.TaskId;
                        _logger.Info($"Task created successfully: {createTaskData.TaskId}");
                        
                        // Step 5: Create event and link to task
                        CreateEventLinkTaskHandlerReqDTO linkEventRequest = new CreateEventLinkTaskHandlerReqDTO
                        {
                            SessionId = sessionId,
                            TaskId = createTaskData.TaskId,
                            EventType = "WorkOrderCreated",
                            Description = $"Work order created from EQ+ ticket: {workOrder.ServiceRequestNumber}"
                        };
                        
                        HttpResponseSnapshot linkEventResponse = await _createEventLinkTaskAtomicHandler.Handle(linkEventRequest);
                        
                        if (!linkEventResponse.IsSuccessStatusCode)
                        {
                            _logger.Warn($"CreateEventLinkTask failed: {linkEventResponse.StatusCode} - Task created but event link failed");
                            // Don't throw - task was created successfully, event link is supplementary
                        }
                        else
                        {
                            CreateEventLinkTaskApiResDTO? linkEventData = SOAPHelper.DeserializeSoapResponse<CreateEventLinkTaskApiResDTO>(linkEventResponse.Content!);
                            result.EventId = linkEventData?.EventId;
                            _logger.Info($"Event linked successfully: {linkEventData?.EventId}");
                        }
                        
                        result.Status = "Success";
                        result.AlreadyExists = false;
                        result.Message = "Work order created successfully";
                        responseDto.SuccessCount++;
                    }
                    
                    responseDto.Results.Add(result);
                    responseDto.TotalProcessed++;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, $"Failed to process work order: {workOrder.ServiceRequestNumber}");
                    result.Status = "Failed";
                    result.Message = ex.Message;
                    responseDto.Results.Add(result);
                    responseDto.TotalProcessed++;
                    responseDto.FailureCount++;
                }
            }
            
            _logger.Info($"[System Layer]-Completed Create Work Order - Total: {responseDto.TotalProcessed}, Success: {responseDto.SuccessCount}, Skipped: {responseDto.SkippedCount}, Failed: {responseDto.FailureCount}");
            
            return new BaseResponseDTO(
                message: InfoConstants.CREATE_WORK_ORDER_SUCCESS,
                data: responseDto,
                errorCode: null
            );
        }
    }
}
