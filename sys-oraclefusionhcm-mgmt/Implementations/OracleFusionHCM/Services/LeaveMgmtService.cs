using Core.DTOs;
using Core.Extensions;
using Microsoft.Extensions.Logging;
using OracleFusionHCMSystem.Abstractions;
using OracleFusionHCMSystem.DTO.CreateLeaveDTO;
using OracleFusionHCMSystem.Implementations.OracleFusionHCM.Handlers;

namespace OracleFusionHCMSystem.Implementations.OracleFusionHCM.Services
{
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
