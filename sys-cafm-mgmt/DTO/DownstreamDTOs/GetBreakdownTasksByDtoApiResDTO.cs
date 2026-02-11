namespace CAFMSystem.DTO.DownstreamDTOs
{
    public class GetBreakdownTasksByDtoApiResDTO
    {
        public BreakdownTaskEnvelopeDto? Envelope { get; set; }
    }

    public class BreakdownTaskEnvelopeDto
    {
        public BreakdownTaskBodyDto? Body { get; set; }
    }

    public class BreakdownTaskBodyDto
    {
        public GetBreakdownTasksByDtoResponseDto? GetBreakdownTasksByDtoResponse { get; set; }
    }

    public class GetBreakdownTasksByDtoResponseDto
    {
        public GetBreakdownTasksByDtoResultDto? GetBreakdownTasksByDtoResult { get; set; }
    }

    public class GetBreakdownTasksByDtoResultDto
    {
        public List<BreakdownTaskDtoV3Item>? BreakdownTaskDtoV3 { get; set; }
    }

    public class BreakdownTaskDtoV3Item
    {
        public string? CallId { get; set; }
        public int? TaskId { get; set; }
        public int? PrimaryKeyId { get; set; }
        public string? TaskNumber { get; set; }
        public string? LongDescription { get; set; }
    }
}
