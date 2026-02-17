using CAFMSystem.Abstractions;
using CAFMSystem.DTO.CreateBreakdownTaskDTO;
using CAFMSystem.DTO.CreateEventDTO;
using CAFMSystem.DTO.GetBreakdownTasksByDtoDTO;
using CAFMSystem.Implementations.CAFM.Handlers;
using Core.DTOs;
using Core.Extensions;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace CAFMSystem.Implementations.CAFM.Services
{
    public class BreakdownTaskMgmtService : IBreakdownTaskMgmt
    {
        private readonly ILogger<BreakdownTaskMgmtService> _logger;
        private readonly GetBreakdownTasksByDtoHandler _getBreakdownTasksByDtoHandler;
        private readonly CreateBreakdownTaskHandler _createBreakdownTaskHandler;
        private readonly CreateEventHandler _createEventHandler;

        public BreakdownTaskMgmtService(
            ILogger<BreakdownTaskMgmtService> logger,
            GetBreakdownTasksByDtoHandler getBreakdownTasksByDtoHandler,
            CreateBreakdownTaskHandler createBreakdownTaskHandler,
            CreateEventHandler createEventHandler)
        {
            _logger = logger;
            _getBreakdownTasksByDtoHandler = getBreakdownTasksByDtoHandler;
            _createBreakdownTaskHandler = createBreakdownTaskHandler;
            _createEventHandler = createEventHandler;
        }

        public async Task<BaseResponseDTO> GetBreakdownTasksByDto(GetBreakdownTasksByDtoReqDTO request)
        {
            _logger.Info("BreakdownTaskMgmtService.GetBreakdownTasksByDto called");
            return await _getBreakdownTasksByDtoHandler.HandleAsync(request);
        }

        public async Task<BaseResponseDTO> CreateBreakdownTask(CreateBreakdownTaskReqDTO request)
        {
            _logger.Info("BreakdownTaskMgmtService.CreateBreakdownTask called");
            return await _createBreakdownTaskHandler.HandleAsync(request);
        }

        public async Task<BaseResponseDTO> CreateEvent(CreateEventReqDTO request)
        {
            _logger.Info("BreakdownTaskMgmtService.CreateEvent called");
            return await _createEventHandler.HandleAsync(request);
        }
    }
}
