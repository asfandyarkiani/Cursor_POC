using Microsoft.Extensions.Logging;
using AGI.SystemLayer.CAFM.DTOs.API;
using AGI.SystemLayer.CAFM.DTOs.Downstream;
using AGI.SystemLayer.CAFM.Implementations.FSI.AtomicHandlers;
using Core.SystemLayer.Handlers;

namespace AGI.SystemLayer.CAFM.Implementations.FSI.Handlers;

/// <summary>
/// Handler for orchestrating work order creation in CAFM.
/// This handler coordinates multiple atomic operations to create a complete work order.
/// </summary>
public class CreateWorkOrderHandler : IBaseHandler
{
    private readonly GetLocationsByDtoAtomicHandler _getLocationsHandler;
    private readonly GetInstructionSetsByDtoAtomicHandler _getInstructionSetsHandler;
    private readonly GetBreakdownTasksByDtoAtomicHandler _getBreakdownTasksHandler;
    private readonly CreateBreakdownTaskAtomicHandler _createBreakdownTaskHandler;
    private readonly CreateEventAtomicHandler _createEventHandler;
    private readonly ILogger<CreateWorkOrderHandler> _logger;

    public CreateWorkOrderHandler(
        GetLocationsByDtoAtomicHandler getLocationsHandler,
        GetInstructionSetsByDtoAtomicHandler getInstructionSetsHandler,
        GetBreakdownTasksByDtoAtomicHandler getBreakdownTasksHandler,
        CreateBreakdownTaskAtomicHandler createBreakdownTaskHandler,
        CreateEventAtomicHandler createEventHandler,
        ILogger<CreateWorkOrderHandler> logger)
    {
        _getLocationsHandler = getLocationsHandler;
        _getInstructionSetsHandler = getInstructionSetsHandler;
        _getBreakdownTasksHandler = getBreakdownTasksHandler;
        _createBreakdownTaskHandler = createBreakdownTaskHandler;
        _createEventHandler = createEventHandler;
        _logger = logger;
    }

    /// <summary>
    /// Creates a work order in CAFM by orchestrating multiple SOAP operations
    /// </summary>
    public async Task<CreateWorkOrderResponseDTO> CreateWorkOrderAsync(
        CreateWorkOrderRequestDTO request,
        string sessionId,
        CancellationToken cancellationToken)
    {
        var response = new CreateWorkOrderResponseDTO
        {
            WorkOrder = new WorkOrderResult
            {
                Items = new List<WorkOrderItem>()
            }
        };

        if (request.WorkOrder?.ServiceRequests == null || !request.WorkOrder.ServiceRequests.Any())
        {
            _logger.LogWarning("CreateWorkOrder: No service requests provided");
            response.Success = false;
            response.Message = "No service requests provided";
            return response;
        }

        foreach (var serviceRequest in request.WorkOrder.ServiceRequests)
        {
            try
            {
                _logger.LogInformation("CreateWorkOrder: Processing service request {SRNumber}",
                    serviceRequest.ServiceRequestNumber);

                // Step 1: Get Location by barcode (unit code)
                var locationResponse = await _getLocationsHandler.GetLocationsAsync(
                    new GetLocationsByDtoRequestDTO
                    {
                        SessionId = sessionId,
                        BarCode = serviceRequest.UnitCode ?? ""
                    },
                    cancellationToken);

                var location = locationResponse.Locations?.FirstOrDefault();
                if (location == null)
                {
                    _logger.LogWarning("CreateWorkOrder: Location not found for UnitCode {UnitCode}",
                        serviceRequest.UnitCode);
                    continue;
                }

                _logger.LogInformation("CreateWorkOrder: Found location {LocationId} for UnitCode {UnitCode}",
                    location.LocationId, serviceRequest.UnitCode);

                // Step 2: Get Instruction Sets (if needed)
                // This can be used to determine the instruction set for the task
                var instructionSetsResponse = await _getInstructionSetsHandler.GetInstructionSetsAsync(
                    new GetInstructionSetsByDtoRequestDTO
                    {
                        SessionId = sessionId,
                        InstructionSetCode = null // Get all or filter by code
                    },
                    cancellationToken);

                var instructionSet = instructionSetsResponse.InstructionSets?.FirstOrDefault();

                // Step 3: Create Breakdown Task
                var createTaskResponse = await _createBreakdownTaskHandler.CreateBreakdownTaskAsync(
                    new CreateBreakdownTaskRequestDTO
                    {
                        SessionId = sessionId,
                        TaskDetails = new BreakdownTaskDetails
                        {
                            LocationId = location.LocationId,
                            Description = serviceRequest.Description,
                            Priority = serviceRequest.Priority,
                            Category = serviceRequest.Category,
                            SubCategory = serviceRequest.SubCategory,
                            RequestedBy = serviceRequest.RequestedBy,
                            ContactPhone = serviceRequest.ContactPhone,
                            Notes = serviceRequest.Notes,
                            InstructionSetId = instructionSet?.InstructionSetId
                        }
                    },
                    cancellationToken);

                if (!createTaskResponse.IsSuccess || string.IsNullOrEmpty(createTaskResponse.TaskId))
                {
                    _logger.LogError("CreateWorkOrder: Failed to create breakdown task for SR {SRNumber}. Error: {Error}",
                        serviceRequest.ServiceRequestNumber, createTaskResponse.ErrorMessage);
                    continue;
                }

                _logger.LogInformation("CreateWorkOrder: Created breakdown task {TaskId} for SR {SRNumber}",
                    createTaskResponse.TaskId, serviceRequest.ServiceRequestNumber);

                // Step 4: Create Event/Link task (optional, based on requirements)
                // This can be used to link the task to other systems or create events
                // Uncomment if needed:
                /*
                var createEventResponse = await _createEventHandler.CreateEventAsync(
                    new CreateEventRequestDTO
                    {
                        SessionId = sessionId,
                        EventDetails = new EventDetails
                        {
                            TaskId = createTaskResponse.TaskId,
                            EventType = "WorkOrderCreated",
                            EventDescription = $"Work order created from {serviceRequest.SourceOrgId}",
                            LinkedTaskId = serviceRequest.ServiceRequestNumber
                        }
                    },
                    cancellationToken);
                */

                // Add to response
                response.WorkOrder.Items.Add(new WorkOrderItem
                {
                    CafmSRNumber = createTaskResponse.TaskId,
                    SourceSRNumber = serviceRequest.ServiceRequestNumber,
                    SourceOrgId = serviceRequest.SourceOrgId
                });

                _logger.LogInformation("CreateWorkOrder: Successfully processed service request {SRNumber}",
                    serviceRequest.ServiceRequestNumber);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CreateWorkOrder: Error processing service request {SRNumber}",
                    serviceRequest.ServiceRequestNumber);
                // Continue processing other requests
            }
        }

        response.Success = response.WorkOrder.Items.Any();
        response.Message = response.Success
            ? $"Successfully created {response.WorkOrder.Items.Count} work order(s)"
            : "Failed to create any work orders";

        return response;
    }
}
