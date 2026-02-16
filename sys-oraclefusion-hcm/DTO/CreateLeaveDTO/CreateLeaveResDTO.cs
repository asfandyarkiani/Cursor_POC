using sys_oraclefusion_hcm.DTO.DownstreamDTOs;

namespace sys_oraclefusion_hcm.DTO.CreateLeaveDTO
{
    public class CreateLeaveResDTO
    {
        public long PersonAbsenceEntryId { get; set; }

        public static CreateLeaveResDTO Map(CreateLeaveApiResDTO apiResponse)
        {
            return new CreateLeaveResDTO
            {
                PersonAbsenceEntryId = apiResponse.PersonAbsenceEntryId
            };
        }
    }
}
