using CAFMSystem.Abstractions;
using CAFMSystem.DTO.HandlerDTOs.GetLocationsByDtoDTO;
using CAFMSystem.DTO.HandlerDTOs.GetInstructionSetsByDtoDTO;
using CAFMSystem.DTO.HandlerDTOs.GetBreakdownTasksByDtoDTO;
using CAFMSystem.DTO.HandlerDTOs.CreateBreakdownTaskDTO;
using CAFMSystem.DTO.HandlerDTOs.CreateEventDTO;
using CAFMSystem.Implementations.FSI.Handlers;
using Core.DTOs;
using Core.Extensions;
using Microsoft.Extensions.Logging;

namespace CAFMSystem.Implementations.FSI.Services
{
    public class CAFMMgmtService : ICAFMMgmt
    {
        private readonly ILogger<CAFMMgmtService> _logger;
        private readonly GetLocationsByDtoHandler _getLocationsByDtoHandler;
        private readonly GetInstructionSetsByDtoHandler _getInstructionSetsByDtoHandler;
        private readonly GetBreakdownTasksByDtoHandler _getBreakdownTasksByDtoHandler;
        private readonly CreateBreakdownTaskHandler _createBreakdownTaskHandler;
        private readonly CreateEventHandler _createEventHandler;

        public CAFMMgmtService(
            ILogger<CAFMMgmtService> logger,
            GetLocationsByDtoHandler getLocationsByDtoHandler,
            GetInstructionSetsByDtoHandler getInstructionSetsByDtoHandler,
            GetBreakdownTasksByDtoHandler getBreakdownTasksByDtoHandler,
            CreateBreakdownTaskHandler createBreakdownTaskHandler,
            CreateEventHandler createEventHandler)
        {
            _logger = logger;
            _getLocationsByDtoHandler = getLocationsByDtoHandler;
            _getInstructionSetsByDtoHandler = getInstructionSetsByDtoHandler;
            _getBreakdownTasksByDtoHandler = getBreakdownTasksByDtoHandler;
            _createBreakdownTaskHandler = createBreakdownTaskHandler;
            _createEventHandler = createEventHandler;
        }

        public async Task<BaseResponseDTO> GetLocationsByDto(GetLocationsByDtoReqDTO request)
        {
            _logger.Info("CAFMMgmtService.GetLocationsByDto called");
            return await _getLocationsByDtoHandler.HandleAsync(request);
        }

        public async Task<BaseResponseDTO> GetInstructionSetsByDto(GetInstructionSetsByDtoReqDTO request)
        {
            _logger.Info("CAFMMgmtService.GetInstructionSetsByDto called");
            return await _getInstructionSetsByDtoHandler.HandleAsync(request);
        }

        public async Task<BaseResponseDTO> GetBreakdownTasksByDto(GetBreakdownTasksByDtoReqDTO request)
        {
            _logger.Info("CAFMMgmtService.GetBreakdownTasksByDto called");
            return await _getBreakdownTasksByDtoHandler.HandleAsync(request);
        }

        public async Task<BaseResponseDTO> CreateBreakdownTask(CreateBreakdownTaskReqDTO request)
        {
            _logger.Info("CAFMMgmtService.CreateBreakdownTask called");
            return await _createBreakdownTaskHandler.HandleAsync(request);
        }

        public async Task<BaseResponseDTO> CreateEvent(CreateEventReqDTO request)
        {
            _logger.Info("CAFMMgmtService.CreateEvent called");
            return await _createEventHandler.HandleAsync(request);
        }
    }
}
