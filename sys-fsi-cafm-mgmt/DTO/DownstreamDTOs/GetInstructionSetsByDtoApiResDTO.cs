namespace FsiCafmSystem.DTO.DownstreamDTOs
{
    public class GetInstructionSetsByDtoApiResDTO
    {
        public List<FINFILEDtoItem>? FINFILEDto { get; set; }
    }
    
    public class FINFILEDtoItem
    {
        public string? IN_FKEY_CAT_SEQ { get; set; }
        public string? IN_FKEY_LAB_SEQ { get; set; }
        public string? IN_FKEY_PRI_SEQ { get; set; }
        public string? IN_SEQ { get; set; }
        public string? IN_DESCRIPTION { get; set; }
    }
}
