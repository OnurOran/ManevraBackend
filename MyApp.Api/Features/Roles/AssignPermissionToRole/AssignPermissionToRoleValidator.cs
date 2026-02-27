using FluentValidation;

namespace MyApp.Api.Features.Roles.AssignPermissionToRole;

public class AssignPermissionToRoleValidator : AbstractValidator<AssignPermissionToRoleCommand>
{
    public AssignPermissionToRoleValidator()
    {
        RuleFor(x => x.RoleId).NotEmpty();
        RuleFor(x => x.PermissionId).NotEmpty();
    }
}
