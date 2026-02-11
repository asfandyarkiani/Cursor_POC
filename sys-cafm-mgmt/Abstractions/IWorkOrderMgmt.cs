using Core.DTOs;
using sys_cafm_mgmt.DTO.CreateBreakdownTaskDTO;
using sys_cafm_mgmt.DTO.CreateEventDTO;

namespace sys_cafm_mgmt.Abstractions
{
    public interface IWorkOrderMgmt
    {
        Task<BaseResponseDTO> CreateBreakdownTask(CreateBreakdownTaskReqDTO request);
        Task<BaseResponseDTO> CreateEvent(CreateEventReqDTO request);
    }
}
