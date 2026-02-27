namespace MyApp.Api.Infrastructure.Email;

/// <summary>Provides email sending capabilities.</summary>
public interface IEmailService
{
    Task SendAsync(string to, string subject, string htmlBody, CancellationToken ct);
    Task SendAsync(IEnumerable<string> to, string subject, string htmlBody, CancellationToken ct);
}
