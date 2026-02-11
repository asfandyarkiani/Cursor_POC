using Core.DTOs;
using Core.Extensions;
using Core.SystemLayer.Handlers;
using Microsoft.Extensions.Logging;
using SysCafmMgmt.DTOs.API;
using SysCafmMgmt.DTOs.DownStream;
using SysCafmMgmt.Implementations.FSI.AtomicHandlers;
using System.Net;

namespace SysCafmMgmt.Implementations.FSI.Handlers
{
    /// <summary>
    /// Handler for orchestrating CAFM work order creation operations
    /// Coordinates multiple atomic handlers to complete the work order creation process
    /// </summary>
    public class CafmWorkOrderHandler : IBaseHandler<CreateWorkOrderRequestDTO>
    {
        private readonly LoginAtomicHandler _loginHandler;
        private readonly CreateEventAtomicHandler _createEventHandler;
        private readonly GetLocationsByDtoAtomicHandler _getLocationsHandler;
        private readonly LogoutAtomicHandler _logoutHandler;
        private readonly ILogger<CafmWorkOrderHandler> _logger;

        public CafmWorkOrderHandler(
            LoginAtomicHandler loginHandler,
            CreateEventAtomicHandler createEventHandler,
            GetLocationsByDtoAtomicHandler getLocationsHandler,
            LogoutAtomicHandler logoutHandler,
            ILogger<CafmWorkOrderHandler> logger)
        {
            _loginHandler = loginHandler ?? throw new ArgumentNullException(nameof(loginHandler));
            _createEventHandler = createEventHandler ?? throw new ArgumentNullException(nameof(createEventHandler));
            _getLocationsHandler = getLocationsHandler ?? throw new ArgumentNullException(nameof(getLocationsHandler));
            _logoutHandler = logoutHandler ?? throw new ArgumentNullException(nameof(logoutHandler));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<BaseResponseDTO> HandleAsync(CreateWorkOrderRequestDTO requestDTO)
        {
            string? sessionId = null;

            try
            {
                _logger.Info($"[CafmWorkOrderHandler] Starting work order creation for SR: {requestDTO.ServiceRequestNumber}");

                // Step 1: Login to CAFM
                var loginRequest = new LoginSoapRequestDTO
                {
                    Username = requestDTO.ServiceRequestNumber, // TODO: Use actual credentials from config
                    Password = "password" // TODO: Use actual credentials from config
                };

                var loginResponse = await _loginHandler.Handle(loginRequest);
                
                if (!loginResponse.Success || string.IsNullOrWhiteSpace(loginResponse.SessionId))
                {
                    _logger.Error($"[CafmWorkOrderHandler] CAFM login failed");
                    return new BaseResponseDTO
                    {
                        StatusCode = (int)HttpStatusCode.Unauthorized,
                        Message = "CAFM login failed",
                        Data = new { Error = loginResponse.ErrorMessage }
                    };
                }

                sessionId = loginResponse.SessionId;
                _logger.Info($"[CafmWorkOrderHandler] CAFM login successful");

                // Step 2: Get Location (if needed)
                // TODO: Implement location lookup logic based on PropertyName/UnitCode

                // Step 3: Create Event/Work Order
                var createEventRequest = new CreateEventSoapRequestDTO
                {
                    SessionId = sessionId,
                    EventType = "WorkOrder",
                    Description = requestDTO.Description,
                    LocationId = "TODO", // TODO: Get from location lookup
                    Priority = requestDTO.TicketDetails?.Priority,
                    ScheduledDate = requestDTO.TicketDetails?.ScheduledDate,
                    TaskId = requestDTO.ServiceRequestNumber
                };

                var createEventResponse = await _createEventHandler.Handle(createEventRequest);

                if (!createEventResponse.Success)
                {
                    _logger.Error($"[CafmWorkOrderHandler] Create event failed");
                    return new BaseResponseDTO
                    {
                        StatusCode = (int)HttpStatusCode.InternalServerError,
                        Message = "Failed to create work order in CAFM",
                        Data = new { Error = createEventResponse.ErrorMessage }
                    };
                }

                _logger.Info($"[CafmWorkOrderHandler] Work order created: {createEventResponse.EventNumber}");

                return new BaseResponseDTO
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = "Work order created successfully",
                    Data = new CreateWorkOrderResponseDTO
                    {
                        Success = true,
                        WorkOrderId = createEventResponse.EventId,
                        WorkOrderNumber = createEventResponse.EventNumber
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"[CafmWorkOrderHandler] Exception during work order creation: {ex.Message}");
                return new BaseResponseDTO
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = "Exception during work order creation",
                    Data = new { Error = ex.Message }
                };
            }
            finally
            {
                // Always attempt logout
                if (!string.IsNullOrWhiteSpace(sessionId))
                {
                    try
                    {
                        var logoutRequest = new LogoutSoapRequestDTO { SessionId = sessionId };
                        await _logoutHandler.Handle(logoutRequest);
                        _logger.Info($"[CafmWorkOrderHandler] CAFM logout completed");
                    }
                    catch (Exception ex)
                    {
                        _logger.Warning(ex, $"[CafmWorkOrderHandler] Logout failed (non-critical): {ex.Message}");
                    }
                }
            }
        }
    }
}
