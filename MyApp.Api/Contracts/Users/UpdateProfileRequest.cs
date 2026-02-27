namespace MyApp.Api.Contracts.Users;

public class UpdateProfileRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    /// <summary>
    /// Optional URL of an already-uploaded avatar image.
    /// Upload the file first via POST /api/v1/files/upload, then pass the returned URL here.
    /// </summary>
    public string? AvatarUrl { get; set; }
}
