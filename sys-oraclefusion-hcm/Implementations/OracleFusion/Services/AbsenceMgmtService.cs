using Core.DTOs;
using Core.Extensions;
using Microsoft.Extensions.Logging;
using OracleFusionHcmSystemLayer.Abstractions;
using OracleFusionHcmSystemLayer.DTO.CreateAbsenceDTO;
using OracleFusionHcmSystemLayer.Implementations.OracleFusion.Handlers;

namespace OracleFusionHcmSystemLayer.Implementations.OracleFusion.Services
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
