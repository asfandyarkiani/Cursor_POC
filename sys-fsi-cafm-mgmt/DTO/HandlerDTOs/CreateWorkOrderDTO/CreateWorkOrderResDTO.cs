using FsiCafmSystem.DTO.DownstreamDTOs;

namespace FsiCafmSystem.DTO.HandlerDTOs.CreateWorkOrderDTO
{
    public class CreateWorkOrderResDTO
    {
        public List<WorkOrderResultDTO> Results { get; set; } = new List<WorkOrderResultDTO>();
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        
        public static CreateWorkOrderResDTO Map(List<CreateBreakdownTaskApiResDTO> apiResponses, List<string> serviceRequestNumbers)
        {
            CreateWorkOrderResDTO response = new CreateWorkOrderResDTO();
            
            for (int i = 0; i < apiResponses.Count; i++)
            {
                CreateBreakdownTaskApiResDTO apiRes = apiResponses[i];
                string srNumber = i < serviceRequestNumbers.Count ? serviceRequestNumbers[i] : string.Empty;
                
                WorkOrderResultDTO result = new WorkOrderResultDTO
                {
                    ServiceRequestNumber = srNumber,
                    TaskId = apiRes.TaskId ?? string.Empty,
                    Success = !string.IsNullOrEmpty(apiRes.TaskId),
                    Message = !string.IsNullOrEmpty(apiRes.TaskId) ? "Work order created successfully" : "Failed to create work order"
                };
                
                response.Results.Add(result);
                
                if (result.Success)
                {
                    response.SuccessCount++;
                }
                else
                {
                    response.FailureCount++;
                }
            }
            
            return response;
        }
    }
    
    public class WorkOrderResultDTO
    {
        public string ServiceRequestNumber { get; set; } = string.Empty;
        public string TaskId { get; set; } = string.Empty;
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
