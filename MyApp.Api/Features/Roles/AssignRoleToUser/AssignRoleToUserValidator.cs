using FluentValidation;

namespace MyApp.Api.Features.Roles.AssignRoleToUser;

public class AssignRoleToUserValidator : AbstractValidator<AssignRoleToUserCommand>
{
    public AssignRoleToUserValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.RoleName).NotEmpty().MaximumLength(256);
    }
}
