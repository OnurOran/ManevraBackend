namespace MyApp.Api.Common.Services;

/// <summary>
/// Centralises all HttpOnly refresh-token cookie logic so every auth endpoint
/// uses identical options — preventing subtle mismatches (wrong path, wrong
/// SameSite mode) that would silently break the refresh flow.
///
/// Security profile (production):
///   HttpOnly       — JavaScript can never read this cookie.
///   Secure         — Only sent over HTTPS. Pair with UseHttpsRedirection in Program.cs.
///   SameSite=None  — Required for cross-origin requests (frontend on a different
///                    domain/port). Must be used together with Secure=true.
///   Path           — Scoped to /api/v1/users so the cookie is not sent with every
///                    API request, only to the endpoints that actually need it.
///
/// Development override (isDevelopment=true):
///   Secure=false, SameSite=Lax — Required because local dev runs on plain HTTP.
///   SameSite=None requires Secure; browsers silently drop SameSite=None cookies
///   over HTTP (Chrome grants a localhost exception, Firefox/Safari do not).
///   Using Lax instead lets the cookie flow correctly in all browsers during local dev.
/// </summary>
public static class RefreshTokenCookieHelper
{
    public const string CookieName = "refreshToken";
    private const string CookiePath = "/api/v1/users";

    public static void Append(HttpResponse response, string token, int expiryDays, bool isDevelopment = false) =>
        response.Cookies.Append(CookieName, token, BuildOptions(TimeSpan.FromDays(expiryDays), isDevelopment));

    public static void Clear(HttpResponse response, bool isDevelopment = false) =>
        response.Cookies.Append(CookieName, string.Empty, BuildOptions(TimeSpan.Zero, isDevelopment));

    private static CookieOptions BuildOptions(TimeSpan maxAge, bool isDevelopment) =>
        isDevelopment
            ? new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                SameSite = SameSiteMode.Lax,
                Path = CookiePath,
                MaxAge = maxAge,
            }
            : new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Path = CookiePath,
                MaxAge = maxAge,
            };
}
