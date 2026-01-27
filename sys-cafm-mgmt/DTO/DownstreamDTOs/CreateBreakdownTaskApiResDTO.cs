namespace sys_cafm_mgmt.DTO.DownstreamDTOs
{
    public class CreateBreakdownTaskApiResDTO
    {
        public CreateBreakdownTaskEnvelopeDTO? Envelope { get; set; }
    }

    public class CreateBreakdownTaskEnvelopeDTO
    {
        public CreateBreakdownTaskBodyDTO? Body { get; set; }
    }

    public class CreateBreakdownTaskBodyDTO
    {
        public CreateBreakdownTaskResponseDTO? CreateBreakdownTaskResponse { get; set; }
    }

    public class CreateBreakdownTaskResponseDTO
    {
        public CreateBreakdownTaskResultDTO? CreateBreakdownTaskResult { get; set; }
    }

    public class CreateBreakdownTaskResultDTO
    {
        public int PrimaryKeyId { get; set; }
        public string? BarCode { get; set; }
        public string? Description { get; set; }
        public int CategoryId { get; set; }
        public int LocationId { get; set; }
        public int BuildingId { get; set; }
    }
}
