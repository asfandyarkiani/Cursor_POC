using CAFMSystem.DTO.DownstreamDTOs;

namespace CAFMSystem.DTO.HandlerDTOs.GetInstructionSetsByDtoDTO
{
    public class GetInstructionSetsByDtoResDTO
    {
        public int InstructionId { get; set; }
        public int CategoryId { get; set; }
        public int DisciplineId { get; set; }
        public int PriorityId { get; set; }
        public string Description { get; set; } = string.Empty;

        public static GetInstructionSetsByDtoResDTO Map(GetInstructionSetsByDtoApiResDTO apiResponse)
        {
            FINFILEDtoItem? instruction = apiResponse?.Envelope?.Body?.GetInstructionSetsByDtoResponse?.GetInstructionSetsByDtoResult?.FINFILEDto?.FirstOrDefault();
            
            return new GetInstructionSetsByDtoResDTO
            {
                InstructionId = instruction?.IN_SEQ ?? 0,
                CategoryId = instruction?.IN_FKEY_CAT_SEQ ?? 0,
                DisciplineId = instruction?.IN_FKEY_LAB_SEQ ?? 0,
                PriorityId = instruction?.IN_FKEY_PRI_SEQ ?? 0,
                Description = instruction?.IN_DESCRIPTION ?? string.Empty
            };
        }
    }
}
