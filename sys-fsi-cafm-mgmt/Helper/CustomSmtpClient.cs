using Core.Extensions;
using Core.Headers;
using FsiCafmSystem.ConfigModels;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using System.Diagnostics;

namespace FsiCafmSystem.Helper
{
    public class CustomSmtpClient
    {
        private readonly ILogger<CustomSmtpClient> _logger;
        private readonly AppConfigs _appConfigs;
        
        public CustomSmtpClient(ILogger<CustomSmtpClient> logger, IOptions<AppConfigs> options)
        {
            _logger = logger;
            _appConfigs = options.Value;
        }
        
        public async Task SendEmailAsync(
            string operationName,
            string fromAddress,
            string toAddress,
            string subject,
            string body,
            bool isHtml = true,
            string? attachmentContent = null,
            string? attachmentFileName = null)
        {
            _logger.Info($"Sending email: {operationName}");
            
            Stopwatch sw = Stopwatch.StartNew();
            
            try
            {
                MimeMessage message = new MimeMessage();
                message.From.Add(MailboxAddress.Parse(fromAddress));
                message.To.Add(MailboxAddress.Parse(toAddress));
                message.Subject = subject;
                
                BodyBuilder bodyBuilder = new BodyBuilder();
                
                if (isHtml)
                {
                    bodyBuilder.HtmlBody = body;
                }
                else
                {
                    bodyBuilder.TextBody = body;
                }
                
                // Add attachment if provided
                if (!string.IsNullOrEmpty(attachmentContent) && !string.IsNullOrEmpty(attachmentFileName))
                {
                    byte[] attachmentBytes = Convert.FromBase64String(attachmentContent);
                    bodyBuilder.Attachments.Add(attachmentFileName, attachmentBytes);
                }
                
                message.Body = bodyBuilder.ToMessageBody();
                
                using SmtpClient client = new SmtpClient();
                
                await client.ConnectAsync(_appConfigs.SmtpHost, _appConfigs.SmtpPort, _appConfigs.SmtpUseSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None);
                
                if (!string.IsNullOrEmpty(_appConfigs.SmtpUsername) && !string.IsNullOrEmpty(_appConfigs.SmtpPassword))
                {
                    await client.AuthenticateAsync(_appConfigs.SmtpUsername, _appConfigs.SmtpPassword);
                }
                
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
                
                sw.Stop();
                ResponseHeaders.DSTimeBreakDown.Item2.Value?.Append($"{operationName}:{sw.ElapsedMilliseconds},");
                
                _logger.Info($"Email sent successfully: {operationName}");
            }
            catch (Exception ex)
            {
                sw.Stop();
                ResponseHeaders.DSTimeBreakDown.Item2.Value?.Append($"{operationName}:{sw.ElapsedMilliseconds},");
                
                _logger.Error(ex, $"Failed to send email: {operationName}");
                throw;
            }
        }
    }
}
