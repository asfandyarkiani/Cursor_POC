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
                dto.PersonAbsenceEntryId = docDict.ToLongValue("PersonAbsenceEntryId");
                dto.Status = "success";
                dto.Message = "Data successfully sent to Oracle Fusion";
                dto.Success = "true";
            }
        }
    }
}
