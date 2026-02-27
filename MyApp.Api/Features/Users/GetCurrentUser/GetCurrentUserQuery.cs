using MyApp.Api.Common.Behaviors;
using MyApp.Api.Contracts.Users;

namespace MyApp.Api.Features.Users.GetCurrentUser;

public class GetCurrentUserQuery : IQuery<UserResponse>
{
    public Guid UserId { get; set; }
}
