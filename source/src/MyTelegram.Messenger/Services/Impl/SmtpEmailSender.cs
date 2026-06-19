using System.Net;
using System.Net.Mail;

namespace MyTelegram.Messenger.Services.Impl;

public class SmtpOptions
{
    public bool Enabled { get; set; } = false;
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public bool UseSsl { get; set; } = true;
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string From { get; set; } = string.Empty;
    public string FromName { get; set; } = "MyTelegram";
}

public class SmtpEmailSender(
    IOptions<SmtpOptions> options,
    ILogger<SmtpEmailSender> logger)
    : IEmailSender, ITransientDependency
{
    private readonly SmtpOptions _options = options.Value;

    public async Task SendAsync(string to, string subject, string body)
    {
        if (!_options.Enabled)
        {
            logger.LogInformation("SMTP disabled. Would send to {To}: [{Subject}] {Body}", to, subject, body);
            return;
        }

        try
        {
            using var client = new SmtpClient(_options.Host, _options.Port)
            {
                EnableSsl = _options.UseSsl,
                Credentials = new NetworkCredential(_options.UserName, _options.Password)
            };
            var from = new MailAddress(_options.From, _options.FromName);
            using var message = new MailMessage(from, new MailAddress(to))
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = false
            };
            await client.SendMailAsync(message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "SMTP send failed to {To}", to);
        }
    }
}
