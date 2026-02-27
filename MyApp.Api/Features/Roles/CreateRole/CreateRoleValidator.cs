using FluentValidation;

namespace MyApp.Api.Features.Roles.CreateRole;

public class CreateRoleValidator : AbstractValidator<CreateRoleCommand>
{
    public CreateRoleValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(64)
            .Matches(@"^[a-zA-Z][a-zA-Z0-9_\- ]*$")
            .WithMessage("Role name must start with a letter and contain only letters, numbers, spaces, hyphens, or underscores.");
    }
}
