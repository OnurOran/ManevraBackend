using FluentValidation;

namespace MyApp.Api.Features.Roles.RemovePermissionFromRole;

public class RemovePermissionFromRoleValidator : AbstractValidator<RemovePermissionFromRoleCommand>
{
    public RemovePermissionFromRoleValidator()
    {
        RuleFor(x => x.RoleId).NotEmpty();
        RuleFor(x => x.PermissionId).NotEmpty();
    }
}
