using Core.DTOs;
using Core.Extensions;
using Microsoft.Extensions.Logging;
using OracleFusionHCM.LeaveManagement.Abstractions;
using OracleFusionHCM.LeaveManagement.DTO.CreateLeaveDTO;
using OracleFusionHCM.LeaveManagement.Implementations.OracleFusion.Handlers;

namespace OracleFusionHCM.LeaveManagement.Implementations.OracleFusion.Services
{
    /// <summary>
    /// Service implementation for Leave Management operations
    /// Delegates to Handlers for business logic execution
    /// </summary>
    public class LeaveMgmtService : ILeaveMgmt
    {
        private readonly ILogger<LeaveMgmtService> _logger;
        private readonly CreateLeaveHandler _createLeaveHandler;
        
        public LeaveMgmtService(
            ILogger<LeaveMgmtService> logger,
            CreateLeaveHandler createLeaveHandler)
        {
            _logger = logger;
            _createLeaveHandler = createLeaveHandler;
        }
        
        public async Task<BaseResponseDTO> CreateLeave(CreateLeaveReqDTO request)
        {
            _logger.Info("LeaveMgmtService.CreateLeave called");
            
            return await _createLeaveHandler.HandleAsync(request);
        }
    }
}
