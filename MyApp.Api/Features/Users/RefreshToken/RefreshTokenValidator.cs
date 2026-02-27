// Validator intentionally removed.
// The refresh token is read directly from the HttpOnly cookie in the endpoint.
// Presence is validated inline: if cookie is missing → 401 is returned immediately.
// No FluentValidation rule is needed since the token is not user-supplied input.
namespace MyApp.Api.Features.Users.RefreshToken;
