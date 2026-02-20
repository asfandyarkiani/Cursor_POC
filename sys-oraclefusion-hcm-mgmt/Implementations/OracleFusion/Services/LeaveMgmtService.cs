using Core.DTOs;
using Core.Extensions;
using Microsoft.Extensions.Logging;
using OracleFusionHcmMgmt.Abstractions;
using OracleFusionHcmMgmt.DTO.CreateLeaveDTO;
using OracleFusionHcmMgmt.Implementations.OracleFusion.Handlers;

namespace OracleFusionHcmMgmt.Implementations.OracleFusion.Services
{
    /// <summary>
    /// Service implementation for Leave Management operations.
    /// Delegates to Handlers for orchestration.
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
            
            BaseResponseDTO result = await _createLeaveHandler.HandleAsync(request);
            
            _logger.Info("LeaveMgmtService.CreateLeave completed");
            
            return result;
        }
    }
}
