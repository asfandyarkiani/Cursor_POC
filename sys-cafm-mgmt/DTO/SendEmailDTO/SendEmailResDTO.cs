namespace sys_cafm_mgmt.DTO.SendEmailDTO
{
    public class SendEmailResDTO
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;

        public static SendEmailResDTO Map(bool success, string message)
        {
            return new SendEmailResDTO
            {
                Success = success,
                Message = message
            };
        }
    }
}
