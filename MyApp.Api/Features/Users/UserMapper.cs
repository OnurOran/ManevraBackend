using MyApp.Api.Domain.Entities;
using MyApp.Api.Contracts.Users;
using Riok.Mapperly.Abstractions;

namespace MyApp.Api.Features.Users;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public static partial class UserMapper
{
    public static partial UserResponse ToResponse(ApplicationUser user);
}
