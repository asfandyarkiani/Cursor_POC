using CAFMSystem.DTO.DownstreamDTOs;

namespace CAFMSystem.DTO.CreateBreakdownTaskDTO
{
    public class CreateBreakdownTaskResDTO
    {
        public string CafmSRNumber { get; set; } = string.Empty;
        public string SourceSRNumber { get; set; } = string.Empty;
        public string SourceOrgId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;

        public static CreateBreakdownTaskResDTO Map(CreateBreakdownTaskApiResDTO apiResponse, string sourceSRNumber, string sourceOrgId)
        {
            return new CreateBreakdownTaskResDTO
            {
                CafmSRNumber = apiResponse?.PrimaryKeyId ?? string.Empty,
                SourceSRNumber = sourceSRNumber ?? string.Empty,
                SourceOrgId = sourceOrgId ?? string.Empty,
                Status = "Success",
                Message = "Work order created successfully"
            };
        }
    }
}
