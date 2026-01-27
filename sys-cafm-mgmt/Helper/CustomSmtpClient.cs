using Core.Extensions;
using Core.Headers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using sys_cafm_mgmt.ConfigModels;
using System.Diagnostics;
using System.Net;
using System.Net.Mail;

namespace sys_cafm_mgmt.Helper
{
    public class CustomSmtpClient
    {
        private readonly AppConfigs _appConfigs;
        private readonly ILogger<CustomSmtpClient> _logger;
        private readonly KeyVaultReader _keyVaultReader;

        public CustomSmtpClient(
            IOptions<AppConfigs> options,
            ILogger<CustomSmtpClient> logger,
            KeyVaultReader keyVaultReader)
        {
            _appConfigs = options.Value;
            _logger = logger;
            _keyVaultReader = keyVaultReader;
        }

        public async Task<bool> SendEmailAsync(
            string operationName,
            string to,
            string subject,
            string body,
            string? attachmentFileName = null,
            string? attachmentContent = null)
        {
            _logger.Info($"Sending email: {operationName}");
            
            Stopwatch stopwatch = Stopwatch.StartNew();
            
            try
            {
                string smtpPassword = await _keyVaultReader.GetSecretAsync(_appConfigs.SmtpPassword);
                
                using SmtpClient smtpClient = new SmtpClient(_appConfigs.SmtpHost, _appConfigs.SmtpPort)
                {
                    Credentials = new NetworkCredential(_appConfigs.SmtpUsername, smtpPassword),
                    EnableSsl = _appConfigs.SmtpEnableSsl
                };
                
                using MailMessage mailMessage = new MailMessage
                {
                    From = new MailAddress(_appConfigs.SmtpUsername),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };
                
                mailMessage.To.Add(to);
                
                if (!string.IsNullOrWhiteSpace(attachmentFileName) && !string.IsNullOrWhiteSpace(attachmentContent))
                {
                    byte[] attachmentBytes = Convert.FromBase64String(attachmentContent);
                    using MemoryStream attachmentStream = new MemoryStream(attachmentBytes);
                    Attachment attachment = new Attachment(attachmentStream, attachmentFileName);
                    mailMessage.Attachments.Add(attachment);
                }
                
                await smtpClient.SendMailAsync(mailMessage);
                
                stopwatch.Stop();
                ResponseHeaders.DSTimeBreakDown.Item2.Value?.Append($"{operationName}:{stopwatch.ElapsedMilliseconds},");
                
                _logger.Info($"Email sent successfully: {operationName}");
                return true;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                ResponseHeaders.DSTimeBreakDown.Item2.Value?.Append($"{operationName}:{stopwatch.ElapsedMilliseconds},");
                
                _logger.Error(ex, $"Failed to send email: {operationName}");
                throw;
            }
        }
    }
}
