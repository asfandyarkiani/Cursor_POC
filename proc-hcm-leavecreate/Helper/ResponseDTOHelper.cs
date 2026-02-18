using Core.Extensions;
using ProcHcmLeaveCreate.DTOs.CreateLeave;
using System.Text.Json;

namespace ProcHcmLeaveCreate.Helper
{
    public static class ResponseDTOHelper
    {
        public static void PopulateCreateLeaveRes(string json, CreateLeaveResDTO dto)
        {
            Dictionary<string, object>? docDict = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
            
            if (docDict != null)
            {
                dto.Status = docDict.ToStringValue("Status");
                dto.Message = docDict.ToStringValue("Message");
                dto.PersonAbsenceEntryId = docDict.ToLongValue("PersonAbsenceEntryId");
                dto.Success = docDict.ToStringValue("Success");
            }
        }
    }
}
