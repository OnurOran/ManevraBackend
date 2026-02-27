using MyApp.Api.Common.Behaviors;
using MyApp.Api.Contracts.Users;

namespace MyApp.Api.Features.Users.GetUserById;

public class GetUserByIdQuery : IQuery<UserListItemResponse>
{
    public Guid UserId { get; set; }
}
