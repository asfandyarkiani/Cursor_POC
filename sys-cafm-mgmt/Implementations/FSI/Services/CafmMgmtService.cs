using Core.DTOs;
using Core.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SysCafmMgmt.Abstractions;
using SysCafmMgmt.ConfigModels;
using SysCafmMgmt.DTOs.API;
using SysCafmMgmt.DTOs.DownStream;
using SysCafmMgmt.Implementations.FSI.AtomicHandlers;
using SysCafmMgmt.Implementations.FSI.Handlers;
using System.Net;

namespace SysCafmMgmt.Implementations.FSI.Services
{
    /// <summary>
    /// Service implementation for CAFM management operations
    /// Provides high-level methods for interacting with FSI CAFM system
    /// </summary>
    public class CafmMgmtService : ICafmMgmt
    {
        private readonly LoginAtomicHandler _loginHandler;
        private readonly LogoutAtomicHandler _logoutHandler;
        private readonly GetLocationsByDtoAtomicHandler _getLocationsHandler;
        private readonly GetInstructionSetsByDtoAtomicHandler _getInstructionSetsHandler;
        private readonly GetBreakdownTasksByDtoAtomicHandler _getBreakdownTasksHandler;
        private readonly CreateEventAtomicHandler _createEventHandler;
        private readonly CafmWorkOrderHandler _workOrderHandler;
        private readonly ILogger<CafmMgmtService> _logger;
        private readonly AppConfigs _appConfigs;

        public CafmMgmtService(
            LoginAtomicHandler loginHandler,
            LogoutAtomicHandler logoutHandler,
            GetLocationsByDtoAtomicHandler getLocationsHandler,
            GetInstructionSetsByDtoAtomicHandler getInstructionSetsHandler,
            GetBreakdownTasksByDtoAtomicHandler getBreakdownTasksHandler,
            CreateEventAtomicHandler createEventHandler,
            CafmWorkOrderHandler workOrderHandler,
            ILogger<CafmMgmtService> logger,
            IOptions<AppConfigs> appConfigs)
        {
            _loginHandler = loginHandler ?? throw new ArgumentNullException(nameof(loginHandler));
            _logoutHandler = logoutHandler ?? throw new ArgumentNullException(nameof(logoutHandler));
            _getLocationsHandler = getLocationsHandler ?? throw new ArgumentNullException(nameof(getLocationsHandler));
            _getInstructionSetsHandler = getInstructionSetsHandler ?? throw new ArgumentNullException(nameof(getInstructionSetsHandler));
            _getBreakdownTasksHandler = getBreakdownTasksHandler ?? throw new ArgumentNullException(nameof(getBreakdownTasksHandler));
            _createEventHandler = createEventHandler ?? throw new ArgumentNullException(nameof(createEventHandler));
            _workOrderHandler = workOrderHandler ?? throw new ArgumentNullException(nameof(workOrderHandler));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _appConfigs = appConfigs?.Value ?? throw new ArgumentNullException(nameof(appConfigs));
        }

        public async Task<BaseResponseDTO> LoginAsync()
        {
            try
            {
                _logger.Info("[CafmMgmtService] Login request received");

                var request = new LoginSoapRequestDTO
                {
                    Username = _appConfigs.CafmSettings.Username,
                    Password = _appConfigs.CafmSettings.Password
                };

                var response = await _loginHandler.Handle(request);

                return new BaseResponseDTO
                {
                    StatusCode = response.Success ? (int)HttpStatusCode.OK : (int)HttpStatusCode.Unauthorized,
                    Message = response.Success ? "Login successful" : "Login failed",
                    Data = new LoginResponseDTO
                    {
                        Success = response.Success,
                        SessionId = response.SessionId,
                        ErrorMessage = response.ErrorMessage
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "[CafmMgmtService] Login exception");
                return new BaseResponseDTO
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = "Login exception",
                    Data = new { Error = ex.Message }
                };
            }
        }

        public async Task<BaseResponseDTO> LogoutAsync(string sessionId)
        {
            try
            {
                _logger.Info($"[CafmMgmtService] Logout request for session: {sessionId}");

                var request = new LogoutSoapRequestDTO { SessionId = sessionId };
                var response = await _logoutHandler.Handle(request);

                return new BaseResponseDTO
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = "Logout completed",
                    Data = new LogoutResponseDTO
                    {
                        Success = response.Success,
                        Message = response.Message
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.Warning(ex, "[CafmMgmtService] Logout exception (non-critical)");
                return new BaseResponseDTO
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = "Logout completed with warnings",
                    Data = new { Warning = ex.Message }
                };
            }
        }

        public async Task<BaseResponseDTO> CreateWorkOrderAsync(CreateWorkOrderRequestDTO request)
        {
            try
            {
                _logger.Info($"[CafmMgmtService] Create work order request for SR: {request.ServiceRequestNumber}");
                return await _workOrderHandler.HandleAsync(request);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "[CafmMgmtService] Create work order exception");
                return new BaseResponseDTO
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = "Create work order exception",
                    Data = new { Error = ex.Message }
                };
            }
        }

        public async Task<BaseResponseDTO> GetLocationsAsync(GetLocationsRequestDTO request)
        {
            string? sessionId = null;

            try
            {
                _logger.Info($"[CafmMgmtService] Get locations request");

                // Login
                var loginResponse = await LoginAndGetSession();
                if (loginResponse.SessionId == null)
                {
                    return CreateErrorResponse(HttpStatusCode.Unauthorized, "Login failed");
                }

                sessionId = loginResponse.SessionId;

                // Get locations
                var soapRequest = new GetLocationsByDtoSoapRequestDTO
                {
                    SessionId = sessionId,
                    LocationCode = request.LocationCode,
                    PropertyName = request.PropertyName
                };

                var response = await _getLocationsHandler.Handle(soapRequest);

                return new BaseResponseDTO
                {
                    StatusCode = response.Success ? (int)HttpStatusCode.OK : (int)HttpStatusCode.InternalServerError,
                    Message = response.Success ? "Locations retrieved" : "Failed to retrieve locations",
                    Data = new GetLocationsResponseDTO
                    {
                        Success = response.Success,
                        Locations = response.Locations?.Select(l => new LocationDTO
                        {
                            LocationId = l.LocationId,
                            LocationCode = l.LocationCode,
                            LocationName = l.LocationName,
                            PropertyName = l.PropertyName,
                            BuildingName = l.BuildingName,
                            FloorName = l.FloorName
                        }).ToList(),
                        ErrorMessage = response.ErrorMessage
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "[CafmMgmtService] Get locations exception");
                return CreateErrorResponse(HttpStatusCode.InternalServerError, $"Exception: {ex.Message}");
            }
            finally
            {
                await LogoutIfNeeded(sessionId);
            }
        }

        public async Task<BaseResponseDTO> GetInstructionSetsAsync(GetInstructionSetsRequestDTO request)
        {
            string? sessionId = null;

            try
            {
                _logger.Info($"[CafmMgmtService] Get instruction sets request");

                var loginResponse = await LoginAndGetSession();
                if (loginResponse.SessionId == null)
                {
                    return CreateErrorResponse(HttpStatusCode.Unauthorized, "Login failed");
                }

                sessionId = loginResponse.SessionId;

                var soapRequest = new GetInstructionSetsByDtoSoapRequestDTO
                {
                    SessionId = sessionId,
                    InstructionSetCode = request.InstructionSetCode,
                    CategoryName = request.CategoryName
                };

                var response = await _getInstructionSetsHandler.Handle(soapRequest);

                return new BaseResponseDTO
                {
                    StatusCode = response.Success ? (int)HttpStatusCode.OK : (int)HttpStatusCode.InternalServerError,
                    Message = response.Success ? "Instruction sets retrieved" : "Failed to retrieve instruction sets",
                    Data = new GetInstructionSetsResponseDTO
                    {
                        Success = response.Success,
                        InstructionSets = response.InstructionSets?.Select(i => new InstructionSetDTO
                        {
                            InstructionSetId = i.InstructionSetId,
                            InstructionSetCode = i.InstructionSetCode,
                            InstructionSetName = i.InstructionSetName,
                            CategoryName = i.CategoryName,
                            Description = i.Description
                        }).ToList(),
                        ErrorMessage = response.ErrorMessage
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "[CafmMgmtService] Get instruction sets exception");
                return CreateErrorResponse(HttpStatusCode.InternalServerError, $"Exception: {ex.Message}");
            }
            finally
            {
                await LogoutIfNeeded(sessionId);
            }
        }

        public async Task<BaseResponseDTO> GetBreakdownTasksAsync(GetBreakdownTasksRequestDTO request)
        {
            string? sessionId = null;

            try
            {
                _logger.Info($"[CafmMgmtService] Get breakdown tasks request");

                var loginResponse = await LoginAndGetSession();
                if (loginResponse.SessionId == null)
                {
                    return CreateErrorResponse(HttpStatusCode.Unauthorized, "Login failed");
                }

                sessionId = loginResponse.SessionId;

                var soapRequest = new GetBreakdownTasksByDtoSoapRequestDTO
                {
                    SessionId = sessionId,
                    BreakdownTaskCode = request.BreakdownTaskCode,
                    CategoryName = request.CategoryName
                };

                var response = await _getBreakdownTasksHandler.Handle(soapRequest);

                return new BaseResponseDTO
                {
                    StatusCode = response.Success ? (int)HttpStatusCode.OK : (int)HttpStatusCode.InternalServerError,
                    Message = response.Success ? "Breakdown tasks retrieved" : "Failed to retrieve breakdown tasks",
                    Data = new GetBreakdownTasksResponseDTO
                    {
                        Success = response.Success,
                        BreakdownTasks = response.BreakdownTasks?.Select(b => new BreakdownTaskDTO
                        {
                            BreakdownTaskId = b.BreakdownTaskId,
                            BreakdownTaskCode = b.BreakdownTaskCode,
                            BreakdownTaskName = b.BreakdownTaskName,
                            CategoryName = b.CategoryName,
                            Description = b.Description
                        }).ToList(),
                        ErrorMessage = response.ErrorMessage
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "[CafmMgmtService] Get breakdown tasks exception");
                return CreateErrorResponse(HttpStatusCode.InternalServerError, $"Exception: {ex.Message}");
            }
            finally
            {
                await LogoutIfNeeded(sessionId);
            }
        }

        public async Task<BaseResponseDTO> CreateEventAsync(CreateEventRequestDTO request)
        {
            string? sessionId = null;

            try
            {
                _logger.Info($"[CafmMgmtService] Create event request");

                var loginResponse = await LoginAndGetSession();
                if (loginResponse.SessionId == null)
                {
                    return CreateErrorResponse(HttpStatusCode.Unauthorized, "Login failed");
                }

                sessionId = loginResponse.SessionId;

                var soapRequest = new CreateEventSoapRequestDTO
                {
                    SessionId = sessionId,
                    EventType = request.EventType,
                    Description = request.Description,
                    LocationId = request.LocationId,
                    Priority = request.Priority,
                    ScheduledDate = request.ScheduledDate,
                    TaskId = request.TaskId
                };

                var response = await _createEventHandler.Handle(soapRequest);

                return new BaseResponseDTO
                {
                    StatusCode = response.Success ? (int)HttpStatusCode.OK : (int)HttpStatusCode.InternalServerError,
                    Message = response.Success ? "Event created" : "Failed to create event",
                    Data = new CreateEventResponseDTO
                    {
                        Success = response.Success,
                        EventId = response.EventId,
                        EventNumber = response.EventNumber,
                        ErrorMessage = response.ErrorMessage
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "[CafmMgmtService] Create event exception");
                return CreateErrorResponse(HttpStatusCode.InternalServerError, $"Exception: {ex.Message}");
            }
            finally
            {
                await LogoutIfNeeded(sessionId);
            }
        }

        private async Task<LoginSoapResponseDTO> LoginAndGetSession()
        {
            var request = new LoginSoapRequestDTO
            {
                Username = _appConfigs.CafmSettings.Username,
                Password = _appConfigs.CafmSettings.Password
            };

            return await _loginHandler.Handle(request);
        }

        private async Task LogoutIfNeeded(string? sessionId)
        {
            if (!string.IsNullOrWhiteSpace(sessionId))
            {
                try
                {
                    var logoutRequest = new LogoutSoapRequestDTO { SessionId = sessionId };
                    await _logoutHandler.Handle(logoutRequest);
                }
                catch (Exception ex)
                {
                    _logger.Warning(ex, "[CafmMgmtService] Logout failed (non-critical)");
                }
            }
        }

        private BaseResponseDTO CreateErrorResponse(HttpStatusCode statusCode, string message)
        {
            return new BaseResponseDTO
            {
                StatusCode = (int)statusCode,
                Message = message,
                Data = new { Error = message }
            };
        }
    }
}
