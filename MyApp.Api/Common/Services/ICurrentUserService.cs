namespace MyApp.Api.Common.Services;

/// <summary>Provides access to the currently authenticated user's claims.</summary>
public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? Email { get; }
    IEnumerable<string> Roles { get; }
    bool IsAuthenticated { get; }
}
