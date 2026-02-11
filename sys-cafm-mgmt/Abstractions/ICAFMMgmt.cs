using CAFMSystem.DTO.HandlerDTOs.GetLocationsByDtoDTO;
using CAFMSystem.DTO.HandlerDTOs.GetInstructionSetsByDtoDTO;
using CAFMSystem.DTO.HandlerDTOs.GetBreakdownTasksByDtoDTO;
using CAFMSystem.DTO.HandlerDTOs.CreateBreakdownTaskDTO;
using CAFMSystem.DTO.HandlerDTOs.CreateEventDTO;
using Core.DTOs;

namespace CAFMSystem.Abstractions
{
    public interface ICAFMMgmt
    {
        Task<BaseResponseDTO> GetLocationsByDto(GetLocationsByDtoReqDTO request);
        Task<BaseResponseDTO> GetInstructionSetsByDto(GetInstructionSetsByDtoReqDTO request);
        Task<BaseResponseDTO> GetBreakdownTasksByDto(GetBreakdownTasksByDtoReqDTO request);
        Task<BaseResponseDTO> CreateBreakdownTask(CreateBreakdownTaskReqDTO request);
        Task<BaseResponseDTO> CreateEvent(CreateEventReqDTO request);
    }
}
