using CAFMSystem.Abstractions;
using CAFMSystem.DTO.HandlerDTOs.CreateWorkOrderDTO;
using CAFMSystem.Implementations.FSI.Handlers;
using Core.DTOs;
using Core.Extensions;
using Microsoft.Extensions.Logging;

namespace CAFMSystem.Implementations.FSI.Services
{
    /// <summary>
    /// Service implementation for Work Order management in CAFM system.
    /// Delegates to CreateWorkOrderHandler for orchestration.
    /// </summary>
    public class WorkOrderMgmtService : IWorkOrderMgmt
    {
        private readonly ILogger<WorkOrderMgmtService> _logger;
        private readonly CreateWorkOrderHandler _createWorkOrderHandler;

        public WorkOrderMgmtService(
            ILogger<WorkOrderMgmtService> logger,
            CreateWorkOrderHandler createWorkOrderHandler)
        {
            _logger = logger;
            _createWorkOrderHandler = createWorkOrderHandler;
        }

        public async Task<BaseResponseDTO> CreateWorkOrder(CreateWorkOrderReqDTO request)
        {
            _logger.Info("WorkOrderMgmtService.CreateWorkOrder called");
            return await _createWorkOrderHandler.HandleAsync(request);
        }
    }
}
