using CAFMSystemLayer.DTO.DownstreamDTOs;

namespace CAFMSystemLayer.DTO.HandlerDTOs.CreateWorkOrderDTO
{
    public class CreateWorkOrderResDTO
    {
        public List<WorkOrderResultDTO> Results { get; set; } = new List<WorkOrderResultDTO>();
        public int TotalProcessed { get; set; }
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public int SkippedCount { get; set; }
        
        public static CreateWorkOrderResDTO CreateEmpty()
        {
            return new CreateWorkOrderResDTO
            {
                Results = new List<WorkOrderResultDTO>(),
                TotalProcessed = 0,
                SuccessCount = 0,
                FailureCount = 0,
                SkippedCount = 0
            };
        }
    }
    
    public class WorkOrderResultDTO
    {
        public string ServiceRequestNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? TaskId { get; set; }
        public string? LocationId { get; set; }
        public string? InstructionSetId { get; set; }
        public string? EventId { get; set; }
        public string? Message { get; set; }
        public bool AlreadyExists { get; set; }
    }
}
