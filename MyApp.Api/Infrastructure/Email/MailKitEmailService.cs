using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace MyApp.Api.Infrastructure.Email;

public class EmailOptions
{
    public const string SectionName = "Email";
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public bool EnableSsl { get; set; } = true;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FromAddress { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
}

public class MailKitEmailService : IEmailService
{
    private readonly EmailOptions _options;
    private readonly ILogger<MailKitEmailService> _logger;

    public MailKitEmailService(IOptions<EmailOptions> options, ILogger<MailKitEmailService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public Task SendAsync(string to, string subject, string htmlBody, CancellationToken ct) =>
        SendAsync([to], subject, htmlBody, ct);

    public async Task SendAsync(IEnumerable<string> to, string subject, string htmlBody, CancellationToken ct)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_options.FromName, _options.FromAddress));
        foreach (var address in to)
        {
            if (!MailboxAddress.TryParse(address, out var mailbox))
                throw new ArgumentException($"Invalid email address: '{address}'");
            message.To.Add(mailbox);
        }
        message.Subject = subject;
        message.Body = new TextPart("html") { Text = htmlBody };

        using var client = new SmtpClient();
        var secureSocket = _options.EnableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None;
        await client.ConnectAsync(_options.Host, _options.Port, secureSocket, ct);
        if (!string.IsNullOrEmpty(_options.Username))
            await client.AuthenticateAsync(_options.Username, _options.Password, ct);
        await client.SendAsync(message, ct);
        await client.DisconnectAsync(true, ct);
        _logger.LogInformation("Email sent to {Recipients} with subject '{Subject}'", string.Join(", ", to), subject);
    }
}
