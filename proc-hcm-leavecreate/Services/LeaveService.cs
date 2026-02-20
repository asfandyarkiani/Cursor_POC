using Core.Extensions;
using Core.Middlewares;
using Microsoft.Extensions.Logging;
using ProcHcmLeaveCreate.Domains;
using ProcHcmLeaveCreate.SystemAbstractions.OracleFusionMgmt;

namespace ProcHcmLeaveCreate.Services
{
    public class LeaveService
    {
        private readonly ILogger<LeaveService> _logger;
        private readonly AbsenceMgmtSys _absenceMgmtSys;

        public LeaveService(ILogger<LeaveService> logger, AbsenceMgmtSys absenceMgmtSys)
        {
            _logger = logger;
            _absenceMgmtSys = absenceMgmtSys;
        }

        public async Task<HttpResponseMessage> CreateLeave(Leave domain)
        {
            _logger.Info("[Process Layer]-Initiating CreateLeave");
            
            HttpResponseMessage response = await _absenceMgmtSys.CreateAbsence(domain);
            
            _logger.Info("[Process Layer]-Completed CreateLeave");
            
            return response;
        }
    }
}
