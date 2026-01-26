using CAFMSystemLayer.Abstractions;
using CAFMSystemLayer.DTO.HandlerDTOs.CreateWorkOrderDTO;
using CAFMSystemLayer.Implementations.CAFM.Handlers;
using Core.DTOs;
using Core.Extensions;
using Microsoft.Extensions.Logging;

namespace CAFMSystemLayer.Implementations.CAFM.Services
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
