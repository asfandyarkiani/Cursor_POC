using CAFMSystem.DTO.DownstreamDTOs;

namespace CAFMSystem.DTO.HandlerDTOs.CreateWorkOrderDTO
{
    /// <summary>
    /// Response DTO for CreateWorkOrder API.
    /// Returns array of work order results.
    /// </summary>
    public class CreateWorkOrderResDTO
    {
        public List<WorkOrderResultDTO> WorkOrders { get; set; } = new List<WorkOrderResultDTO>();

        public static CreateWorkOrderResDTO Map(List<WorkOrderResultDTO> results)
        {
            return new CreateWorkOrderResDTO
            {
                WorkOrders = results ?? new List<WorkOrderResultDTO>()
            };
        }
    }

    public class WorkOrderResultDTO
    {
        public string CafmSRNumber { get; set; } = string.Empty;
        public string SourceSRNumber { get; set; } = string.Empty;
        public string SourceOrgId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;

        public static WorkOrderResultDTO MapSuccess(
            CreateBreakdownTaskApiResDTO taskResponse,
            string sourceSRNumber,
            string sourceOrgId)
        {
            return new WorkOrderResultDTO
            {
                CafmSRNumber = taskResponse?.TaskId ?? string.Empty,
                SourceSRNumber = sourceSRNumber,
                SourceOrgId = sourceOrgId,
                Status = "Success",
                Message = "Work order created successfully in CAFM"
            };
        }

        public static WorkOrderResultDTO MapError(
            string sourceSRNumber,
            string sourceOrgId,
            string errorMessage)
        {
            return new WorkOrderResultDTO
            {
                CafmSRNumber = string.Empty,
                SourceSRNumber = sourceSRNumber,
                SourceOrgId = sourceOrgId,
                Status = "Error",
                Message = errorMessage
            };
        }
    }
}
