using Core.Validators;

namespace SysCafmMgmt.ConfigModels
{
    public class AppConfigs : IConfigValidator
    {
        public CafmSettings CafmSettings { get; set; } = new();
        public EmailSettings EmailSettings { get; set; } = new();

        public void Validate()
        {
            if (CafmSettings == null)
                throw new ArgumentNullException(nameof(CafmSettings), "CAFM settings are required");

            if (string.IsNullOrWhiteSpace(CafmSettings.BaseUrl))
                throw new ArgumentException("CAFM BaseUrl is required", nameof(CafmSettings.BaseUrl));

            if (string.IsNullOrWhiteSpace(CafmSettings.Username))
                throw new ArgumentException("CAFM Username is required", nameof(CafmSettings.Username));

            if (string.IsNullOrWhiteSpace(CafmSettings.Password))
                throw new ArgumentException("CAFM Password is required", nameof(CafmSettings.Password));

            if (EmailSettings == null)
                throw new ArgumentNullException(nameof(EmailSettings), "Email settings are required");
        }
    }

    public class CafmSettings
    {
        public string BaseUrl { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string LoginResourcePath { get; set; } = "/FSIWebServices/EvolutionService.asmx";
        public string LoginSoapAction { get; set; } = "http://www.fsi.co.uk/services/evolution/04/09/Authenticate";
        public string LogoutResourcePath { get; set; } = "/FSIWebServices/EvolutionService.asmx";
        public string LogoutSoapAction { get; set; } = "http://www.fsi.co.uk/services/evolution/04/09/LogOut";
        public string GetLocationsByDtoSoapAction { get; set; } = "http://www.fsi.co.uk/services/evolution/04/09/GetLocationsByDto";
        public string GetInstructionSetsByDtoSoapAction { get; set; } = "http://www.fsi.co.uk/services/evolution/04/09/GetInstructionSetsByDto";
        public string GetBreakdownTasksByDtoSoapAction { get; set; } = "http://www.fsi.co.uk/services/evolution/04/09/GetBreakdownTasksByDto";
        public string CreateEventSoapAction { get; set; } = "http://www.fsi.co.uk/services/evolution/04/09/CreateEvent";
        public int TimeoutSeconds { get; set; } = 60;
    }

    public class EmailSettings
    {
        public string SmtpHost { get; set; } = "smtp-mail.outlook.com";
        public int SmtpPort { get; set; } = 587;
        public string FromEmail { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool EnableSsl { get; set; } = false;
        public bool EnableTls { get; set; } = true;
    }
}
