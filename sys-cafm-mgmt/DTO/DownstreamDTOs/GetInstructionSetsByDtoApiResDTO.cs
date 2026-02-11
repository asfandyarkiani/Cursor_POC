namespace CAFMSystem.DTO.DownstreamDTOs
{
    public class GetInstructionSetsByDtoApiResDTO
    {
        public InstructionEnvelopeDto? Envelope { get; set; }
    }

    public class InstructionEnvelopeDto
    {
        public InstructionBodyDto? Body { get; set; }
    }

    public class InstructionBodyDto
    {
        public GetInstructionSetsByDtoResponseDto? GetInstructionSetsByDtoResponse { get; set; }
    }

    public class GetInstructionSetsByDtoResponseDto
    {
        public GetInstructionSetsByDtoResultDto? GetInstructionSetsByDtoResult { get; set; }
    }

    public class GetInstructionSetsByDtoResultDto
    {
        public List<FINFILEDtoItem>? FINFILEDto { get; set; }
    }

    public class FINFILEDtoItem
    {
        public int? IN_SEQ { get; set; }
        public int? IN_FKEY_CAT_SEQ { get; set; }
        public int? IN_FKEY_LAB_SEQ { get; set; }
        public int? IN_FKEY_PRI_SEQ { get; set; }
        public string? IN_DESCRIPTION { get; set; }
    }
}
