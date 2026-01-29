namespace CAFMSystem.DTO.DownstreamDTOs
{
    /// <summary>
    /// Response DTO for CAFM GetInstructionSetsByDto operation.
    /// Maps to SOAP response: Envelope/Body/GetInstructionSetsByDtoResponse/GetInstructionSetsByDtoResult/FINFILEDto
    /// </summary>
    public class GetInstructionSetsByDtoApiResDTO
    {
        public string? IN_FKEY_CAT_SEQ { get; set; }
        public string? IN_FKEY_LAB_SEQ { get; set; }
        public string? IN_FKEY_PRI_SEQ { get; set; }
        public string? IN_SEQ { get; set; }
        public string? IN_DESCRIPTION { get; set; }
    }
}
