using Core.DTOs;
using Core.Extensions;
using Microsoft.Extensions.Logging;
using SysCafmMgmt.Abstractions;
using SysCafmMgmt.DTOs.Requests;
using SysCafmMgmt.Implementations.Fsi.Handlers;

namespace SysCafmMgmt.Implementations.Fsi.Services
{
    /// <summary>
    /// Service implementation for Work Order management operations
    /// Delegates to appropriate handlers
    /// </summary>
    public class WorkOrderService : IWorkOrderMgmt
    {
        private readonly CreateWorkOrderHandler _createWorkOrderHandler;
        private readonly ILogger<WorkOrderService> _logger;

        public WorkOrderService(
            CreateWorkOrderHandler createWorkOrderHandler,
            ILogger<WorkOrderService> logger)
        {
            _createWorkOrderHandler = createWorkOrderHandler;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<BaseResponseDTO> CreateWorkOrderAsync(CreateWorkOrderRequestDto request)
        {
            _logger.Info("Creating work order via CAFM FSI");
            return await _createWorkOrderHandler.HandleAsync(request);
        }
    }
}
