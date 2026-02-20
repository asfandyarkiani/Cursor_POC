using Core.DTOs;
using Core.Extensions;
using Microsoft.Extensions.Logging;
using OracleFusionHcm.Abstractions;
using OracleFusionHcm.DTO.CreateLeaveDTO;
using OracleFusionHcm.Implementations.OracleFusion.Handlers;

namespace OracleFusionHcm.Implementations.OracleFusion.Services
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
