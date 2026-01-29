using CAFMSystem.Abstractions;
using CAFMSystem.DTO.CreateBreakdownTaskDTO;
using CAFMSystem.DTO.CreateEventDTO;
using CAFMSystem.DTO.GetBreakdownTasksByDtoDTO;
using CAFMSystem.Implementations.FSI.Handlers;
using Core.DTOs;
using Core.Extensions;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace CAFMSystem.Implementations.FSI.Services
{
    /// <summary>
    /// Service implementation for Work Order management operations in CAFM system.
    /// Delegates to handlers for actual operation execution.
    /// </summary>
    public class WorkOrderMgmtService : IWorkOrderMgmt
    {
        private readonly ILogger<WorkOrderMgmtService> _logger;
        private readonly GetBreakdownTasksByDtoHandler _getBreakdownTasksByDtoHandler;
        private readonly CreateBreakdownTaskHandler _createBreakdownTaskHandler;
        private readonly CreateEventHandler _createEventHandler;

        public WorkOrderMgmtService(
            ILogger<WorkOrderMgmtService> logger,
            GetBreakdownTasksByDtoHandler getBreakdownTasksByDtoHandler,
            CreateBreakdownTaskHandler createBreakdownTaskHandler,
            CreateEventHandler createEventHandler)
        {
            _logger = logger;
            _getBreakdownTasksByDtoHandler = getBreakdownTasksByDtoHandler;
            _createBreakdownTaskHandler = createBreakdownTaskHandler;
            _createEventHandler = createEventHandler;
        }

        public async Task<BaseResponseDTO> GetBreakdownTasksByDto(GetBreakdownTasksByDtoReqDTO request)
        {
            _logger.Info("WorkOrderMgmtService.GetBreakdownTasksByDto called");
            return await _getBreakdownTasksByDtoHandler.HandleAsync(request);
        }

        public async Task<BaseResponseDTO> CreateBreakdownTask(CreateBreakdownTaskReqDTO request)
        {
            _logger.Info("WorkOrderMgmtService.CreateBreakdownTask called");
            return await _createBreakdownTaskHandler.HandleAsync(request);
        }

        public async Task<BaseResponseDTO> CreateEvent(CreateEventReqDTO request)
        {
            _logger.Info("WorkOrderMgmtService.CreateEvent called");
            return await _createEventHandler.HandleAsync(request);
        }
    }
}
