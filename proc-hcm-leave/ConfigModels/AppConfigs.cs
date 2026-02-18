namespace HcmLeaveProcessLayer.ConfigModels
{
    public class AppConfigs
    {
        public static string SectionName = "AppVariables";
        
        public string ASPNETCORE_ENVIRONMENT { get; set; } = string.Empty;
        public string CreateAbsenceSystemLayerUrl { get; set; } = string.Empty;
        public string ErrorEmailRecipient { get; set; } = string.Empty;
        public string ErrorEmailSender { get; set; } = string.Empty;
        public string ErrorEmailSubjectPrefix { get; set; } = string.Empty;
        public string ErrorEmailFileNamePrefix { get; set; } = string.Empty;
        public string ErrorEmailHasAttachment { get; set; } = string.Empty;
    }
}
