using Core.DTOs;
using Core.Extensions;
using Core.SystemLayer.Handlers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SysCafmMgmt.ConfigModels;
using SysCafmMgmt.DTOs.Downstream;
using SysCafmMgmt.DTOs.Requests;
using SysCafmMgmt.DTOs.Responses;
using SysCafmMgmt.Implementations.Fsi.AtomicHandlers;
using System.Net;

namespace SysCafmMgmt.Implementations.Fsi.Handlers
{
    /// <summary>
    /// Handler for orchestrating Create Work Order flow
    /// Coordinates: Login -> GetLocations -> GetInstructionSets -> CreateBreakdownTask -> Logout
    /// </summary>
    public class CreateWorkOrderHandler : IBaseHandler<CreateWorkOrderRequestDto>
    {
        private readonly AuthenticateAtomicHandler _authenticateHandler;
        private readonly LogoutAtomicHandler _logoutHandler;
        private readonly GetLocationsByDtoAtomicHandler _getLocationsHandler;
        private readonly GetInstructionSetsByDtoAtomicHandler _getInstructionsHandler;
        private readonly CreateBreakdownTaskAtomicHandler _createTaskHandler;
        private readonly ILogger<CreateWorkOrderHandler> _logger;
        private readonly AppConfigs _config;

        private const string StepName = "CreateWorkOrderHandler";

        public CreateWorkOrderHandler(
            AuthenticateAtomicHandler authenticateHandler,
            LogoutAtomicHandler logoutHandler,
            GetLocationsByDtoAtomicHandler getLocationsHandler,
            GetInstructionSetsByDtoAtomicHandler getInstructionsHandler,
            CreateBreakdownTaskAtomicHandler createTaskHandler,
            ILogger<CreateWorkOrderHandler> logger,
            IOptions<AppConfigs> config)
        {
            _authenticateHandler = authenticateHandler;
            _logoutHandler = logoutHandler;
            _getLocationsHandler = getLocationsHandler;
            _getInstructionsHandler = getInstructionsHandler;
            _createTaskHandler = createTaskHandler;
            _logger = logger;
            _config = config.Value;
        }

        public async Task<BaseResponseDTO> HandleAsync(CreateWorkOrderRequestDto requestDTO)
        {
            requestDTO.ValidateAPIRequestParameters();

            var workOrderItem = requestDTO.WorkOrder!.First();
            string? sessionId = null;
            
            try
            {
                _logger.Info($"Starting Create Work Order flow for SR: {workOrderItem.ServiceRequestNumber}");

                // Step 1: Authenticate to CAFM FSI
                sessionId = await AuthenticateAsync();

                // Step 2: Get Location details (parallel with Step 3)
                var getLocationsTask = GetLocationDetailsAsync(sessionId, workOrderItem.UnitCode);

                // Step 3: Get Instruction Set details (parallel with Step 2)
                var getInstructionsTask = GetInstructionSetDetailsAsync(sessionId, workOrderItem.CategoryName);

                // Wait for both to complete
                await Task.WhenAll(getLocationsTask, getInstructionsTask);

                var locationDetails = await getLocationsTask;
                var instructionDetails = await getInstructionsTask;

                // Step 4: Create Breakdown Task
                var createTaskResult = await CreateBreakdownTaskAsync(
                    sessionId,
                    workOrderItem,
                    locationDetails,
                    instructionDetails
                );

                // Build success response
                var responseData = new CreateWorkOrderResponseDto
                {
                    WorkOrder = new List<WorkOrderResponseItem>
                    {
                        new WorkOrderResponseItem
                        {
                            CafmSrNumber = createTaskResult.TaskId,
                            SourceSrNumber = workOrderItem.ServiceRequestNumber,
                            SourceOrgId = workOrderItem.SourceOrgId,
                            Success = true,
                            Message = "Work order created successfully"
                        }
                    }
                };

                _logger.Info($"Successfully created work order. CAFM TaskId: {createTaskResult.TaskId}");

                return new BaseResponseDTO(
                    message: "Work order created successfully",
                    errorCode: "SUCCESS",
                    data: responseData
                );
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Error creating work order for SR: {workOrderItem.ServiceRequestNumber}");

                // Build error response
                var errorResponseData = new CreateWorkOrderResponseDto
                {
                    WorkOrder = new List<WorkOrderResponseItem>
                    {
                        new WorkOrderResponseItem
                        {
                            SourceSrNumber = workOrderItem.ServiceRequestNumber,
                            SourceOrgId = workOrderItem.SourceOrgId,
                            Success = false,
                            Message = ex.Message
                        }
                    }
                };

                throw; // Re-throw to let ExceptionHandlerMiddleware handle it
            }
            finally
            {
                // Step 5: Always attempt logout
                if (!string.IsNullOrEmpty(sessionId))
                {
                    await LogoutAsync(sessionId);
                }
            }
        }

        private async Task<string> AuthenticateAsync()
        {
            _logger.Info("Step 1: Authenticating to CAFM FSI");

            var authRequest = new AuthenticateRequestDto
            {
                LoginName = _config.Cafm.Username,
                Password = _config.Cafm.Password
            };

            // TODO: Credentials should be retrieved from secure store
            if (string.IsNullOrWhiteSpace(authRequest.LoginName) || string.IsNullOrWhiteSpace(authRequest.Password))
            {
                throw new Core.Exceptions.HTTPBaseException(
                    message: "CAFM credentials not configured",
                    errorCode: "CONFIG_ERROR",
                    statusCode: HttpStatusCode.InternalServerError,
                    stepName: "AuthenticateAsync"
                );
            }

            var authResult = await _authenticateHandler.Handle(authRequest);
            return authResult.SessionId!;
        }

        private async Task LogoutAsync(string sessionId)
        {
            _logger.Info("Step 5: Logging out from CAFM FSI");

            try
            {
                var logoutRequest = new LogoutRequestDto { SessionId = sessionId };
                await _logoutHandler.Handle(logoutRequest);
            }
            catch (Exception ex)
            {
                // Log but don't fail - logout errors should not affect the response
                _logger.Warn($"Logout failed: {ex.Message}");
            }
        }

        private async Task<(string? BuildingId, string? LocationId)> GetLocationDetailsAsync(string sessionId, string? unitCode)
        {
            _logger.Info($"Step 2: Getting location details for unit code: {unitCode}");

            if (string.IsNullOrWhiteSpace(unitCode))
            {
                _logger.Warn("Unit code not provided, skipping location lookup");
                return (null, null);
            }

            var locationsRequest = new GetLocationsByDtoRequestDto
            {
                SessionId = sessionId,
                LocationCode = unitCode
            };

            var locationsResult = await _getLocationsHandler.Handle(locationsRequest);

            if (locationsResult.Locations?.Count > 0)
            {
                var location = locationsResult.Locations.First();
                _logger.Info($"Found location - BuildingId: {location.BuildingId}, LocationId: {location.LocationId}");
                return (location.BuildingId, location.LocationId);
            }

            _logger.Warn($"No locations found for unit code: {unitCode}");
            return (null, null);
        }

        private async Task<(string? CategoryId, string? DisciplineId, string? InstructionId, string? PriorityId)> GetInstructionSetDetailsAsync(
            string sessionId, string? categoryName)
        {
            _logger.Info($"Step 3: Getting instruction set details for category: {categoryName}");

            if (string.IsNullOrWhiteSpace(categoryName))
            {
                _logger.Warn("Category name not provided, skipping instruction set lookup");
                return (null, null, null, null);
            }

            var instructionsRequest = new GetInstructionSetsByDtoRequestDto
            {
                SessionId = sessionId,
                CategoryName = categoryName
            };

            var instructionsResult = await _getInstructionsHandler.Handle(instructionsRequest);

            if (instructionsResult.InstructionSets?.Count > 0)
            {
                var instruction = instructionsResult.InstructionSets.First();
                _logger.Info($"Found instruction set - CategoryId: {instruction.CategoryId}, DisciplineId: {instruction.DisciplineId}");
                return (instruction.CategoryId, instruction.DisciplineId, instruction.InstructionId, instruction.PriorityId);
            }

            _logger.Warn($"No instruction sets found for category: {categoryName}");
            return (null, null, null, null);
        }

        private async Task<CreateBreakdownTaskResponseDto> CreateBreakdownTaskAsync(
            string sessionId,
            WorkOrderItem workOrderItem,
            (string? BuildingId, string? LocationId) locationDetails,
            (string? CategoryId, string? DisciplineId, string? InstructionId, string? PriorityId) instructionDetails)
        {
            _logger.Info($"Step 4: Creating breakdown task for SR: {workOrderItem.ServiceRequestNumber}");

            // Format dates as required by FSI
            var scheduledDateUtc = FormatScheduledDate(
                workOrderItem.TicketDetails?.ScheduledDate,
                workOrderItem.TicketDetails?.ScheduledTimeStart
            );
            var raisedDateUtc = FormatRaisedDate(workOrderItem.TicketDetails?.RaisedDateUtc);

            var createTaskRequest = new CreateBreakdownTaskRequestDto
            {
                SessionId = sessionId,
                ReporterName = workOrderItem.ReporterName,
                ReporterEmail = workOrderItem.ReporterEmail,
                ReporterPhone = workOrderItem.ReporterPhoneNumber,
                BuildingId = locationDetails.BuildingId,
                LocationId = locationDetails.LocationId,
                CategoryId = instructionDetails.CategoryId,
                DisciplineId = instructionDetails.DisciplineId,
                InstructionId = instructionDetails.InstructionId,
                PriorityId = instructionDetails.PriorityId,
                CallId = workOrderItem.ServiceRequestNumber,
                LongDescription = workOrderItem.Description,
                ScheduledDateUtc = scheduledDateUtc,
                RaisedDateUtc = raisedDateUtc,
                ContractId = _config.Cafm.DefaultContractId,
                CallerSourceId = _config.Cafm.CallerSourceId
            };

            return await _createTaskHandler.Handle(createTaskRequest);
        }

        /// <summary>
        /// Format scheduled date combining date and time
        /// Based on Boomi scripting: scheduledDate + "T" + scheduledTimeStart + "Z"
        /// </summary>
        private string? FormatScheduledDate(string? scheduledDate, string? scheduledTimeStart)
        {
            if (string.IsNullOrWhiteSpace(scheduledDate))
                return null;

            try
            {
                var time = scheduledTimeStart ?? "00:00:00";
                var fullDateTime = $"{scheduledDate}T{time}Z";
                var date = DateTime.Parse(fullDateTime);
                // Format with precision matching Boomi: ".0208713Z"
                return date.ToString("yyyy-MM-ddTHH:mm:ss.0208713Z");
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Format raised date
        /// Based on Boomi scripting for raisedDateUtc conversion
        /// </summary>
        private string? FormatRaisedDate(string? raisedDateUtc)
        {
            if (string.IsNullOrWhiteSpace(raisedDateUtc))
                return null;

            try
            {
                var date = DateTime.Parse(raisedDateUtc);
                return date.ToString("yyyy-MM-ddTHH:mm:ss.0208713Z");
            }
            catch
            {
                return raisedDateUtc; // Return as-is if parsing fails
            }
        }
    }
}
