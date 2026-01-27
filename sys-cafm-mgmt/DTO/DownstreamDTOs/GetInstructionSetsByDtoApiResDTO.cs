namespace sys_cafm_mgmt.DTO.DownstreamDTOs
{
    public class GetInstructionSetsByDtoApiResDTO
    {
        public InstructionEnvelopeDTO? Envelope { get; set; }
    }

    public class InstructionEnvelopeDTO
    {
        public InstructionBodyDTO? Body { get; set; }
    }

    public class InstructionBodyDTO
    {
        public GetInstructionSetsByDtoResponseDTO? GetInstructionSetsByDtoResponse { get; set; }
    }

    public class GetInstructionSetsByDtoResponseDTO
    {
        public GetInstructionSetsByDtoResultDTO? GetInstructionSetsByDtoResult { get; set; }
    }

    public class GetInstructionSetsByDtoResultDTO
    {
        public List<InstructionItemDTO>? Instructions { get; set; }
    }

    public class InstructionItemDTO
    {
        public int InstructionId { get; set; }
        public string? InstructionName { get; set; }
        public string? InstructionCode { get; set; }
        public int CategoryId { get; set; }
    }
}
