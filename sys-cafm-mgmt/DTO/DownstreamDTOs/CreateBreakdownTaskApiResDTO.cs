namespace CAFMSystem.DTO.DownstreamDTOs
{
    public class CreateBreakdownTaskApiResDTO
    {
        public CreateTaskEnvelopeDto? Envelope { get; set; }
    }

    public class CreateTaskEnvelopeDto
    {
        public CreateTaskBodyDto? Body { get; set; }
    }

    public class CreateTaskBodyDto
    {
        public CreateBreakdownTaskResponseDto? CreateBreakdownTaskResponse { get; set; }
    }

    public class CreateBreakdownTaskResponseDto
    {
        public CreateBreakdownTaskResultDto? CreateBreakdownTaskResult { get; set; }
    }

    public class CreateBreakdownTaskResultDto
    {
        public int? TaskId { get; set; }
        public int? PrimaryKeyId { get; set; }
        public int? FilterQueryId { get; set; }
        public string? ValidateOnly { get; set; }
        public string? TaskNumber { get; set; }
        public string? CallId { get; set; }
    }
}
