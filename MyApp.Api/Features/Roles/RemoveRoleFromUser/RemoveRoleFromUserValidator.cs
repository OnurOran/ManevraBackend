using FluentValidation;

namespace MyApp.Api.Features.Roles.RemoveRoleFromUser;

public class RemoveRoleFromUserValidator : AbstractValidator<RemoveRoleFromUserCommand>
{
    public RemoveRoleFromUserValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.RoleName).NotEmpty().MaximumLength(256);
    }
}
