namespace FsiCafmSystem.DTO.HandlerDTOs.SendEmailDTO
{
    public class SendEmailResDTO
    {
        public bool EmailSent { get; set; }
        public string Message { get; set; } = string.Empty;
        
        public static SendEmailResDTO Map(bool success, string message)
        {
            return new SendEmailResDTO
            {
                EmailSent = success,
                Message = message
            };
        }
    }
}
