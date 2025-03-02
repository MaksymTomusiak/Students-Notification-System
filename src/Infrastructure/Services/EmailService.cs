using Application.Common.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;

namespace Infrastructure.Services;

public class EmailService(IConfiguration configuration) : IEmailService
{
    private readonly string _smtpHost = configuration["EmailSettings:SmtpHost"];
    private readonly int _smtpPort = int.Parse(configuration["EmailSettings:SmtpPort"]);
    private readonly string _smtpUser = configuration["EmailSettings:SenderEmail"];
    private readonly string _smtpPassword = configuration["EmailSettings:SenderPassword"];
    private readonly string _fromEmail = configuration["EmailSettings:SenderEmail"];
    private readonly string _senderName = configuration["EmailSettings:SenderName"];

    public void SendEmail(string to, string subject, string body, bool isHtml = true)
    {
        // Create the email message
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_senderName, _fromEmail));
        message.To.Add(new MailboxAddress("Recipient", to));
        message.Subject = subject;

        // Set the body as HTML or plain text based on isHtml
        message.Body = new TextPart(isHtml ? "html" : "plain")
        {
            Text = body
        };

        // Send the email using MailKit
        try
        {
            using var client = new SmtpClient();
            client.Connect(_smtpHost, _smtpPort, SecureSocketOptions.SslOnConnect);
            client.Authenticate(_smtpUser, _smtpPassword);
            client.Send(message);
            client.Disconnect(true);
        }
        catch (Exception ex)
        {
            // Log the error (e.g., using ILogger or Console)
            Console.WriteLine($"Failed to send email: {ex.Message}");
        }
    }
}
