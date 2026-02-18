namespace ProcHcmLeave.ConfigModels
{
    public class AppConfigs
    {
        public static string SectionName = "AppVariables";
        
        public string CreateAbsenceUrl { get; set; } = string.Empty;
        public string Environment { get; set; } = string.Empty;
    }
}
