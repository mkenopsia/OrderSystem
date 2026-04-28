using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Options;
using NotificationService.Infrastructure.Email;

namespace NotificationService.Services;

public class MailKitEmailSender(IOptions<EmailSettings> options, ILogger<MailKitEmailSender> logger) : IEmailSender
{
    private readonly EmailSettings _settings = options.Value;

    public async Task SendAsync(string to, string subject, string body, CancellationToken ct = default)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_settings.FromName, _settings.FromEmail));
        message.To.Add(new MailboxAddress(to, to));
        message.Subject = subject;
        message.Body = new TextPart("html") { Text = body };

        using var client = new SmtpClient();
        try
        {
            await client.ConnectAsync(_settings.SmtpHost, _settings.SmtpPort, MailKit.Security.SecureSocketOptions.StartTls, ct);
            await client.AuthenticateAsync(_settings.Username, _settings.Password, ct);
            await client.SendAsync(message, ct);
            logger.LogInformation("📧 Email sent to {To}", to);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send email to {To}", to);
        }
        finally
        {
            await client.DisconnectAsync(true, ct);
        }
    }
}