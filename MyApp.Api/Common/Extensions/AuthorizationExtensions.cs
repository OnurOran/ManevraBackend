namespace MyApp.Api.Common.Extensions;

public static class AuthorizationExtensions
{
    /// <summary>Requires the caller to have the specified permission claim.</summary>
    public static RouteHandlerBuilder RequirePermission(this RouteHandlerBuilder builder, string permission) =>
        builder.RequireAuthorization(p => p.RequireClaim("permission", permission));
}
