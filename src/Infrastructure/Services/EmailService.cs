using Application.Common.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;

namespace Infrastructure.Services;

public class EmailService(IConfiguration configuration, ILogger<EmailService> logger) : IEmailService
{
    private readonly string _smtpHost = configuration["EmailSettings:SmtpHost"];
    private readonly int _smtpPort = int.Parse(configuration["EmailSettings:SmtpPort"]);
    private readonly string _smtpUser = configuration["EmailSettings:SenderEmail"];
    private readonly string _smtpPassword = configuration["EmailSettings:SenderPassword"];
    private readonly string _fromEmail = configuration["EmailSettings:SenderEmail"];
    private readonly string _senderName = configuration["EmailSettings:SenderName"];

    public void SendEmail(string to, string subject, string body)
    {
        try
        {
            // Create the MimeMessage
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_senderName, _fromEmail));
            message.To.Add(new MailboxAddress("Recipient", to));
            message.Subject = subject;

            // Create the body of the email
            var bodyBuilder = new BodyBuilder { HtmlBody = body };
            message.Body = bodyBuilder.ToMessageBody();

            // Connect to the SMTP server and send the email
            using (var client = new SmtpClient())
            {
                client.Connect(_smtpHost, _smtpPort, SecureSocketOptions.SslOnConnect);
                client.Authenticate(_smtpUser, _smtpPassword);

                client.Send(message);
                client.Disconnect(true);
            }

            logger.LogInformation($"Email sent to {to}: {subject}");
        }
        catch (Exception ex)
        {
            logger.LogError($"Failed to send email to {to}: {ex.Message}");
        }
    }
}
