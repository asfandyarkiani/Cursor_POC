using ProcHcmLeave.DTOs.CreateLeave;

namespace ProcHcmLeave.SystemAbstractions.HcmMgmt.Interfaces
{
    public interface ILeaveMgmt
    {
        Task<HttpResponseMessage> CreateLeave(CreateLeaveReqDTO request);
    }
}
