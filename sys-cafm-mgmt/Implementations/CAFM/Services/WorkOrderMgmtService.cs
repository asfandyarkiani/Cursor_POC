using Core.DTOs;
using Core.Extensions;
using Microsoft.Extensions.Logging;
using sys_cafm_mgmt.Abstractions;
using sys_cafm_mgmt.DTO.CreateBreakdownTaskDTO;
using sys_cafm_mgmt.DTO.CreateEventDTO;
using sys_cafm_mgmt.Implementations.CAFM.Handlers;

namespace sys_cafm_mgmt.Implementations.CAFM.Services
{
    public class WorkOrderMgmtService : IWorkOrderMgmt
    {
        private readonly ILogger<WorkOrderMgmtService> _logger;
        private readonly CreateBreakdownTaskHandler _createBreakdownTaskHandler;
        private readonly CreateEventHandler _createEventHandler;

        public WorkOrderMgmtService(
            ILogger<WorkOrderMgmtService> logger,
            CreateBreakdownTaskHandler createBreakdownTaskHandler,
            CreateEventHandler createEventHandler)
        {
            _logger = logger;
            _createBreakdownTaskHandler = createBreakdownTaskHandler;
            _createEventHandler = createEventHandler;
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
