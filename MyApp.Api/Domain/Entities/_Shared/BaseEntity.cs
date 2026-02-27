namespace MyApp.Api.Domain.Entities;

public abstract class BaseEntity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; private set; }
    public Guid? CreatedBy { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public Guid? UpdatedBy { get; private set; }
    public bool IsDeleted { get; private set; }

    public void SetCreated(Guid? userId = null)
    {
        CreatedAt = DateTime.UtcNow;
        CreatedBy = userId;
    }

    public void SetUpdated(Guid? userId = null)
    {
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = userId;
    }

    public void SoftDelete() => IsDeleted = true;
}
