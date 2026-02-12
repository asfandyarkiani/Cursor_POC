using Core.DTOs;
using Core.Extensions;
using Microsoft.Extensions.Logging;
using OracleFusionHCMSystemLayer.Abstractions;
using OracleFusionHCMSystemLayer.DTO.CreateAbsenceDTO;
using OracleFusionHCMSystemLayer.Implementations.OracleFusionHCM.Handlers;

namespace OracleFusionHCMSystemLayer.Implementations.OracleFusionHCM.Services
{
    public class AbsenceMgmtService : IAbsenceMgmt
    {
        private readonly ILogger<AbsenceMgmtService> _logger;
        private readonly CreateAbsenceHandler _createAbsenceHandler;

        public AbsenceMgmtService(
            ILogger<AbsenceMgmtService> logger,
            CreateAbsenceHandler createAbsenceHandler)
        {
            _logger = logger;
            _createAbsenceHandler = createAbsenceHandler;
        }

        public async Task<BaseResponseDTO> CreateAbsence(CreateAbsenceReqDTO request)
        {
            _logger.Info("AbsenceMgmtService.CreateAbsence called");
            return await _createAbsenceHandler.HandleAsync(request);
        }
    }
}
