using Core.Extensions;
using ProcHcmLeave.DTOs.CreateLeave;
using System.Text.Json;

namespace ProcHcmLeave.Helper
{
    public static class ResponseDTOHelper
    {
        public static void PopulateCreateLeaveRes(string json, CreateLeaveResDTO dto)
        {
            Dictionary<string, object>? docDict = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
            
            if (docDict != null)
            {
                dto.PersonAbsenceEntryId = docDict.ToLongValue("PersonAbsenceEntryId");
                dto.AbsenceType = docDict.ToStringValue("AbsenceType");
                dto.StartDate = docDict.ToStringValue("StartDate");
                dto.EndDate = docDict.ToStringValue("EndDate");
                dto.AbsenceStatusCd = docDict.ToStringValue("AbsenceStatusCd");
                dto.ApprovalStatusCd = docDict.ToStringValue("ApprovalStatusCd");
            }
        }
    }
}
