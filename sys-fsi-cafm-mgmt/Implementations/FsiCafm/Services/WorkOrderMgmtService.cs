using Core.DTOs;
using Core.Extensions;
using FsiCafmSystem.Abstractions;
using FsiCafmSystem.DTO.HandlerDTOs.CreateWorkOrderDTO;
using FsiCafmSystem.Implementations.FsiCafm.Handlers;
using Microsoft.Extensions.Logging;

namespace FsiCafmSystem.Implementations.FsiCafm.Services
{
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
