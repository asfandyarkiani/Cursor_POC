using Core.DTOs;
using Core.Extensions;
using Core.SystemLayer.Handlers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SysCafmMgmt.ConfigModels;
using SysCafmMgmt.DTOs.API;
using SysCafmMgmt.DTOs.Downstream;
using SysCafmMgmt.Implementations.FSI.AtomicHandlers;

namespace SysCafmMgmt.Implementations.FSI.Handlers;

/// <summary>
/// Handler for Create Work Order operation
/// Orchestrates multiple atomic handlers for FSI CAFM work order creation
/// 
/// Flow:
/// 1. Login to FSI CAFM → Get SessionId
/// 2. Parallel: GetLocationsByDto and GetInstructionSetsByDto
/// 3. CreateBreakdownTask
/// 4. Logout (best-effort)
/// </summary>
public class CreateWorkOrderHandler : IBaseHandler<CreateWorkOrderRequestDTO>
{
    private readonly AuthenticateAtomicHandler _authenticateHandler;
    private readonly LogoutAtomicHandler _logoutHandler;
    private readonly GetLocationsByDtoAtomicHandler _getLocationsHandler;
    private readonly GetInstructionSetsByDtoAtomicHandler _getInstructionSetsHandler;
    private readonly CreateBreakdownTaskAtomicHandler _createBreakdownTaskHandler;
    private readonly ILogger<CreateWorkOrderHandler> _logger;
    private readonly FsiConfig _fsiConfig;

    public CreateWorkOrderHandler(
        AuthenticateAtomicHandler authenticateHandler,
        LogoutAtomicHandler logoutHandler,
        GetLocationsByDtoAtomicHandler getLocationsHandler,
        GetInstructionSetsByDtoAtomicHandler getInstructionSetsHandler,
        CreateBreakdownTaskAtomicHandler createBreakdownTaskHandler,
        ILogger<CreateWorkOrderHandler> logger,
        IOptions<AppConfigs> appConfigs)
    {
        _authenticateHandler = authenticateHandler;
        _logoutHandler = logoutHandler;
        _getLocationsHandler = getLocationsHandler;
        _getInstructionSetsHandler = getInstructionSetsHandler;
        _createBreakdownTaskHandler = createBreakdownTaskHandler;
        _logger = logger;
        _fsiConfig = appConfigs.Value.Fsi;
    }

    public async Task<BaseResponseDTO> HandleAsync(CreateWorkOrderRequestDTO requestDTO)
    {
        _logger.Info("Starting CreateWorkOrder handler");

        // Process first work order item (batch processing can be added later)
        var workOrderItem = requestDTO.WorkOrder!.First();
        string? sessionId = null;

        try
        {
            // Step 1: Authenticate to FSI CAFM
            _logger.Info("Step 1: Authenticating to FSI CAFM");
            var authResponse = await _authenticateHandler.Handle(new FsiAuthenticateRequestDTO
            {
                LoginName = _fsiConfig.Username,
                Password = _fsiConfig.Password
            });
            sessionId = authResponse.SessionId;

            // Step 2: Get Location and Instruction Set data in parallel
            _logger.Info("Step 2: Getting Location and Instruction Set data");
            var locationTask = _getLocationsHandler.Handle(new FsiGetLocationsByDtoRequestDTO
            {
                SessionId = sessionId,
                BarCode = workOrderItem.UnitCode
            });

            var instructionTask = _getInstructionSetsHandler.Handle(new FsiGetInstructionSetsByDtoRequestDTO
            {
                SessionId = sessionId,
                InDescription = workOrderItem.SubCategory
            });

            await Task.WhenAll(locationTask, instructionTask);

            var locationResponse = await locationTask;
            var instructionResponse = await instructionTask;

            // Step 3: Create Breakdown Task
            _logger.Info("Step 3: Creating Breakdown Task");
            var createTaskRequest = new FsiCreateBreakdownTaskRequestDTO
            {
                SessionId = sessionId,
                ReporterName = workOrderItem.ReporterName,
                ReporterEmail = workOrderItem.ReporterEmail,
                Phone = workOrderItem.ReporterPhoneNumber,
                CallId = workOrderItem.ServiceRequestNumber,
                CategoryId = instructionResponse.CategoryId,
                DisciplineId = instructionResponse.DisciplineId,
                PriorityId = instructionResponse.PriorityId,
                BuildingId = locationResponse.BuildingId,
                LocationId = locationResponse.LocationId,
                InstructionId = instructionResponse.InstructionId,
                LongDescription = workOrderItem.Description,
                ScheduledDateUtc = FormatScheduledDate(workOrderItem.TicketDetails?.ScheduledDate, workOrderItem.TicketDetails?.ScheduledTimeStart),
                RaisedDateUtc = FormatRaisedDate(workOrderItem.TicketDetails?.RaisedDateUtc),
                ContractId = _fsiConfig.ContractId,
                CallerSourceId = _fsiConfig.CallerSourceId
            };

            var createTaskResponse = await _createBreakdownTaskHandler.Handle(createTaskRequest);

            // Build response
            var responseData = new CreateWorkOrderResponseDTO
            {
                WorkOrderId = createTaskResponse.TaskId,
                TaskNumber = createTaskResponse.TaskNumber,
                SourceServiceRequestNumber = workOrderItem.ServiceRequestNumber,
                Status = "Created",
                Message = "Work order created successfully"
            };

            _logger.Info($"Work order created successfully - TaskNumber: {createTaskResponse.TaskNumber}");

            return new BaseResponseDTO(
                message: "Work order created successfully",
                errorCode: string.Empty,
                data: responseData);
        }
        finally
        {
            // Step 4: Logout (best-effort)
            if (!string.IsNullOrEmpty(sessionId))
            {
                _logger.Info("Step 4: Logging out from FSI CAFM");
                try
                {
                    await _logoutHandler.Handle(new FsiLogoutRequestDTO { SessionId = sessionId });
                }
                catch (Exception ex)
                {
                    _logger.Warn($"Logout failed (non-critical): {ex.Message}");
                }
            }
        }
    }

    private string? FormatScheduledDate(string? date, string? time)
    {
        if (string.IsNullOrWhiteSpace(date))
            return null;

        // Combine date and time
        var dateTime = date;
        if (!string.IsNullOrWhiteSpace(time))
        {
            dateTime = $"{date}T{time}Z";
        }
        else
        {
            dateTime = $"{date}T00:00:00Z";
        }

        if (DateTime.TryParse(dateTime, out var parsed))
        {
            return parsed.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ");
        }

        return dateTime;
    }

    private string? FormatRaisedDate(string? raisedDateUtc)
    {
        if (string.IsNullOrWhiteSpace(raisedDateUtc))
            return DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ");

        if (DateTime.TryParse(raisedDateUtc, out var parsed))
        {
            return parsed.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ");
        }

        return raisedDateUtc;
    }
}
