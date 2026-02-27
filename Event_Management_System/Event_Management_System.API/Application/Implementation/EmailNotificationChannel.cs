using Event_Management_System.API.Application.Interfaces;
using Event_Management_System.API.Domain.DTOs;
using Event_Management_System.API.Helpers;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using System.Net.Mail;

namespace Event_Management_System.API.Application.Implementation
{
    public class EmailNotificationChannel : INotificationChannel
    {
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<EmailNotificationChannel> _logger;

        public Domain.Enums.NotificationChannel ChannelType => Domain.Enums.NotificationChannel.Email;

        public EmailNotificationChannel(IOptions<EmailSettings> emailSettings, ILogger<EmailNotificationChannel> logger)
        {
            _emailSettings = emailSettings.Value;
            _logger = logger;
        }

        public async Task SendAsync(NotificationRequest request, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(request.RecipientEmail))
            {
                _logger.LogWarning("Cannot send email notification: RecipientEmail is empty for notification type {Type}", request.Type);
                return;
            }

            try
            {
                var (subject, bodyHtml) = EmailTemplateBuilder.Build(
                    request.Type,
                    request.RecipientName ?? "User",
                    request.Data);

                // 1. Build the MimeMessage (Replaces MailMessage)
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_emailSettings.DefaultFromName, _emailSettings.DefaultFromEmail));
                message.To.Add(new MailboxAddress(request.RecipientName, request.RecipientEmail));
                message.Subject = subject;

                // 2. Set the body using BodyBuilder
                var bodyBuilder = new BodyBuilder { HtmlBody = bodyHtml };
                message.Body = bodyBuilder.ToMessageBody();

                // 3. Configure the MailKit SmtpClient (Note: different namespace than System.Net.Mail)
                using var client = new MailKit.Net.Smtp.SmtpClient();

                // Determine security options (Auto usually handles STARTTLS on 587 and SSL on 465 automatically)
                var secureSocketOption = _emailSettings.Port == 25
                    ? SecureSocketOptions.None
                    : SecureSocketOptions.Auto;

                // 4. Connect, Authenticate, and Send
                await client.ConnectAsync(_emailSettings.SMTPSetting.Host, _emailSettings.Port, secureSocketOption, cancellationToken);

                if (!string.IsNullOrWhiteSpace(_emailSettings.UserName))
                {
                    await client.AuthenticateAsync(_emailSettings.UserName, _emailSettings.Password, cancellationToken);
                }

                await client.SendAsync(message, cancellationToken);

                // 5. Disconnect cleanly
                await client.DisconnectAsync(true, cancellationToken);

                _logger.LogInformation(
                    "Email notification sent to {Email} for {Type}",
                    request.RecipientEmail,
                    request.Type);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email notification to {Email} for {Type}", request.RecipientEmail, request.Type);
                throw;
            }
        }
    }
}