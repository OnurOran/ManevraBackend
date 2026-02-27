using Microsoft.AspNetCore.Identity;

namespace MyApp.Api.Domain.Entities;

public class ApplicationUser : IdentityUser<Guid>
{
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string? AvatarUrl { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public bool IsDeleted { get; private set; }

    private ApplicationUser() { }

    public static ApplicationUser Create(string email, string firstName, string lastName) =>
        new()
        {
            Email = email,
            UserName = email,
            FirstName = firstName,
            LastName = lastName,
            CreatedAt = DateTime.UtcNow
        };

    public void UpdateProfile(string firstName, string lastName, string? avatarUrl = null)
    {
        FirstName = firstName;
        LastName = lastName;
        if (avatarUrl is not null) AvatarUrl = avatarUrl;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SoftDelete() => IsDeleted = true;
}
