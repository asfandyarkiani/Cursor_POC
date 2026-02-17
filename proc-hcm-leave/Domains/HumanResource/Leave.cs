using Core.ProcessLayer.Domains;

namespace ProcHcmLeave.Domains.HumanResource
{
    public class Leave : IDomain<int>
    {
        private int _id;
        public int Id { get => _id; set => _id = value; }
        
        public int EmployeeNumber { get; set; }
        public string AbsenceType { get; set; } = string.Empty;
        public string Employer { get; set; } = string.Empty;
        public string StartDate { get; set; } = string.Empty;
        public string EndDate { get; set; } = string.Empty;
        public string AbsenceStatusCode { get; set; } = string.Empty;
        public string ApprovalStatusCode { get; set; } = string.Empty;
        public int StartDateDuration { get; set; }
        public int EndDateDuration { get; set; }
    }
}
