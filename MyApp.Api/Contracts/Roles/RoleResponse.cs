namespace MyApp.Api.Contracts.Roles;

public class RoleResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<PermissionResponse> Permissions { get; set; } = [];
}
