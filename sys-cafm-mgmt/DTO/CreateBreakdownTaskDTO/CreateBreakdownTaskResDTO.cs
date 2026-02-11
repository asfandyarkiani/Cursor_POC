using sys_cafm_mgmt.DTO.DownstreamDTOs;

namespace sys_cafm_mgmt.DTO.CreateBreakdownTaskDTO
{
    public class CreateBreakdownTaskResDTO
    {
        public int BreakdownTaskId { get; set; }
        public string ServiceRequestNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;

        public static CreateBreakdownTaskResDTO Map(CreateBreakdownTaskApiResDTO apiResponse)
        {
            return new CreateBreakdownTaskResDTO
            {
                BreakdownTaskId = apiResponse?.Envelope?.Body?.CreateBreakdownTaskResponse?.CreateBreakdownTaskResult?.PrimaryKeyId ?? 0,
                ServiceRequestNumber = apiResponse?.Envelope?.Body?.CreateBreakdownTaskResponse?.CreateBreakdownTaskResult?.BarCode ?? string.Empty,
                Status = "Created",
                Message = "Breakdown task created successfully"
            };
        }
    }
}
