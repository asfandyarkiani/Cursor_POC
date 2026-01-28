using CAFMManagementSystem.Abstractions;
using CAFMManagementSystem.DTO.CreateBreakdownTaskDTO;
using CAFMManagementSystem.DTO.GetBreakdownTasksByDtoDTO;
using CAFMManagementSystem.Implementations.FSIConcept.Handlers;
using Core.DTOs;
using Core.Extensions;
using Microsoft.Extensions.Logging;

namespace CAFMManagementSystem.Implementations.FSIConcept.Services
{
    public class BreakdownTaskMgmtService : IBreakdownTaskMgmt
    {
        private readonly ILogger<BreakdownTaskMgmtService> _logger;
        private readonly GetBreakdownTasksByDtoHandler _getBreakdownTasksByDtoHandler;
        private readonly CreateBreakdownTaskHandler _createBreakdownTaskHandler;
        
        public BreakdownTaskMgmtService(
            ILogger<BreakdownTaskMgmtService> logger,
            GetBreakdownTasksByDtoHandler getBreakdownTasksByDtoHandler,
            CreateBreakdownTaskHandler createBreakdownTaskHandler)
        {
            _logger = logger;
            _getBreakdownTasksByDtoHandler = getBreakdownTasksByDtoHandler;
            _createBreakdownTaskHandler = createBreakdownTaskHandler;
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
    }
}
