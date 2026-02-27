using FluentValidation;

namespace MyApp.Api.Features.Roles.DeleteRole;

public class DeleteRoleValidator : AbstractValidator<DeleteRoleCommand>
{
    public DeleteRoleValidator()
    {
        RuleFor(x => x.RoleId).NotEmpty();
    }
}
