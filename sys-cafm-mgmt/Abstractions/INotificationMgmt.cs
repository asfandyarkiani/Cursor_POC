using Core.DTOs;
using sys_cafm_mgmt.DTO.SendEmailDTO;

namespace sys_cafm_mgmt.Abstractions
{
    public interface INotificationMgmt
    {
        Task<BaseResponseDTO> SendEmail(SendEmailReqDTO request);
    }
}
