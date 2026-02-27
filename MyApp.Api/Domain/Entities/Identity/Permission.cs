namespace MyApp.Api.Domain.Entities;

public class Permission
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Name { get; private set; } = string.Empty;

    private Permission() { }

    public static Permission Create(string name) => new() { Name = name };
}
