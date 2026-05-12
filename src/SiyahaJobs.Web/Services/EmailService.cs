using System.Net;
using System.Net.Mail;
using SiyahaJobs.Web.Services.Interfaces;

namespace SiyahaJobs.Web.Services;

/// <summary>
/// Sends email via SMTP using settings from appsettings.json.
/// If SMTP settings are empty, falls back to logging the message (useful in Dev).
/// </summary>
public class EmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration config, ILogger<EmailService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task SendAsync(string to, string subject, string htmlBody)
    {
        var host = _config["EmailSettings:SmtpHost"];
        var username = _config["EmailSettings:Username"];
        var password = _config["EmailSettings:Password"];
        var senderEmail = _config["EmailSettings:SenderEmail"] ?? "no-reply@siyahajobs.jo";
        var senderName = _config["EmailSettings:SenderName"] ?? "SiyahaJobs";
        var port = _config.GetValue<int?>("EmailSettings:SmtpPort") ?? 587;
        var ssl = _config.GetValue<bool?>("EmailSettings:EnableSsl") ?? true;

        if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(username))
        {
            _logger.LogInformation("[Email:DEV] To: {To} | Subject: {Subject}\n{Body}", to, subject, htmlBody);
            return;
        }

        using var client = new SmtpClient(host, port)
        {
            EnableSsl = ssl,
            Credentials = new NetworkCredential(username, password)
        };

        using var msg = new MailMessage
        {
            From = new MailAddress(senderEmail, senderName),
            Subject = subject,
            Body = htmlBody,
            IsBodyHtml = true
        };
        msg.To.Add(to);

        try
        {
            await client.SendMailAsync(msg);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send email to {To}. Falling back to log.", to);
        }
    }
}
