namespace sys_cafm_mgmt.DTO.DownstreamDTOs
{
    public class GetBreakdownTasksByDtoApiResDTO
    {
        public BreakdownTaskEnvelopeDTO? Envelope { get; set; }
    }

    public class BreakdownTaskEnvelopeDTO
    {
        public BreakdownTaskBodyDTO? Body { get; set; }
    }

    public class BreakdownTaskBodyDTO
    {
        public GetBreakdownTasksByDtoResponseDTO? GetBreakdownTasksByDtoResponse { get; set; }
    }

    public class GetBreakdownTasksByDtoResponseDTO
    {
        public GetBreakdownTasksByDtoResultDTO? GetBreakdownTasksByDtoResult { get; set; }
    }

    public class GetBreakdownTasksByDtoResultDTO
    {
        public List<BreakdownTaskItemDTO>? Tasks { get; set; }
    }

    public class BreakdownTaskItemDTO
    {
        public int TaskId { get; set; }
        public string? BarCode { get; set; }
        public string? Description { get; set; }
        public string? Status { get; set; }
    }
}
