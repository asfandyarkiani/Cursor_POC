using CAFMManagementSystem.DTO.CreateBreakdownTaskDTO;
using CAFMManagementSystem.DTO.GetBreakdownTasksByDtoDTO;
using Core.DTOs;

namespace CAFMManagementSystem.Abstractions
{
    public interface IBreakdownTaskMgmt
    {
        Task<BaseResponseDTO> GetBreakdownTasksByDto(GetBreakdownTasksByDtoReqDTO request);
        Task<BaseResponseDTO> CreateBreakdownTask(CreateBreakdownTaskReqDTO request);
    }
}
