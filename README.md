# MyApp Backend Template

Production-ready .NET 10 backend template designed to be reused across multiple client projects. Each project takes the latest version of this template and configures it to fit its specific needs.

**Target product portfolio:** Production line tracking, personnel tracking, financial tracking, portfolio sites, basic e-commerce.

---

## Solution Structure

```
MyApp.slnx
├── MyApp.Api/              → Main API project (.NET 10 Minimal API)
├── MyApp.SourceGenerators/ → Roslyn source generator — auto-registers handlers in DI
│                             and maps [MapToGroup] endpoints at compile time
└── MyApp.Api.Tests/        → Integration tests (xUnit + WebApplicationFactory + Testcontainers)
```

### MyApp.Api directory layout

```
MyApp.Api/
├── Contracts/           → Request/response DTOs (Users/, Roles/)
├── Common/
│   ├── Authorization/   → Permissions constants, PermissionSyncService
│   ├── Behaviors/       → ICommandHandler<,>, IQueryHandler<,> interfaces
│   ├── Exceptions/      → GlobalExceptionHandler, DomainException, NotFoundException
│   ├── Extensions/      → All service registration extensions (Auth, CORS, Infrastructure…)
│   ├── Models/          → Result<T>, ApiResponse<T>, PagedResult<T>
│   └── Services/        → JwtTokenService, ICurrentUserService / CurrentUserService,
│                           RefreshTokenCookieHelper
├── Domain/
│   └── Entities/        → BaseEntity, ApplicationUser, RefreshToken, Permission, RolePermission
├── Infrastructure/
│   ├── BackgroundJobs/  → CleanupExpiredRefreshTokensJob (Quartz)
│   ├── Email/           → MailKitEmailService, EmailOptions
│   ├── Hubs/            → AppHub (SignalR), INotificationService / NotificationService
│   ├── Persistence/     → AppDbContext, EF Core Configurations/
│   └── Storage/         → HetznerFileService (S3), StorageOptions
└── Features/
    ├── _Shared/         → RequestLoggingMiddleware
    ├── Users/           → RegisterUser, LoginUser, RefreshToken, Logout,
    │                       GetCurrentUser, UpdateProfile, GetUsers, GetUserById
    └── Roles/           → GetRoles, GetPermissions, CreateRole, DeleteRole,
                           AssignPermissionToRole, RemovePermissionFromRole,
                           AssignRoleToUser, RemoveRoleFromUser
```

---

## Technology Stack

| Layer | Technology |
|-------|-----------|
| Framework | .NET 10 Minimal API |
| ORM | EF Core 10 + SqlServer |
| Identity | ASP.NET Core Identity + JWT Bearer |
| Validation | FluentValidation 12.x |
| Mapper | Riok.Mapperly 4.x (source-gen) |
| Logging | Serilog 10 (Console + File) |
| Background Jobs | Quartz.NET 3.x |
| Email | MailKit 4.x |
| File Storage | AWSSDK.S3 3.7.x (Hetzner Object Storage / MinIO) |
| Real-time | SignalR |
| API Docs | Scalar (development only, at `/scalar/v1`) |
| Health Checks | AspNetCore.HealthChecks.SqlServer + DiskStorage |
| Containers | Docker + docker-compose |

---

## Architecture

### Vertical Slice Architecture

Each feature lives in its own directory and consists of Command/Query/Handler/Endpoint/Validator files. Features are independent of each other; shared infrastructure lives under `Common/`.

```
Features/Users/LoginUser/
├── LoginUserCommand.cs     → Data carrier
├── LoginUserValidator.cs   → FluentValidation rules
├── LoginUserHandler.cs     → Business logic
└── LoginUserEndpoint.cs    → HTTP endpoint definition
```

### No MediatR — Custom Handler Interfaces

No pipeline library is used. All handlers are resolved directly through DI.

```csharp
// Commands (cause side effects)
public interface ICommandHandler<TCommand, TResult>
{
    Task<Result<TResult>> Handle(TCommand command, CancellationToken cancellationToken);
}

// Queries (read-only)
public interface IQueryHandler<TQuery, TResult>
{
    Task<Result<TResult>> Handle(TQuery query, CancellationToken cancellationToken);
}
```

All handlers are registered automatically by the `MyApp.SourceGenerators` source generator — no manual `AddScoped<>` calls are needed. The generator also calls `Endpoint.Map()` for every class decorated with `[MapToGroup]`.

---

## Core Patterns

### Result\<T\>

Handlers never throw exceptions; they return success/failure state via `Result<T>`. The endpoint translates this into an HTTP response.

```csharp
// Handler
return Result<UserResponse>.Failure("Invalid email or password.");
return Result<UserResponse>.Success(response);

// Endpoint
return result.IsSuccess
    ? Results.Ok(ApiResponse<UserResponse>.Ok(result.Value!))
    : Results.Json(ApiResponse<UserResponse>.Fail(result.Error!), statusCode: 401);
```

### ApiResponse\<T\>

Every response is wrapped in this envelope:

```json
// Success
{ "success": true, "data": { ... }, "message": null }

// Error
{ "success": false, "data": null, "errors": ["Invalid email or password."] }
```

There are two variants: generic `ApiResponse<T>` and non-generic `ApiResponse` for responses with no data payload.

### PagedResult\<T\>

For paginated lists:

```csharp
public class PagedResult<T>
{
    public IEnumerable<T> Items { get; }
    public int TotalCount { get; }
    public int Page { get; }
    public int PageSize { get; }
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling(TotalCount / (double)PageSize) : 0;
}
```

### Validation Flow

```csharp
// Inside the endpoint, before the handler
var (isValid, errorResult) = await validator.ValidateRequestAsync(command, ct);
if (!isValid) return errorResult!; // 400 + error list

var result = await handler.Handle(command, ct);
```

`ValidateRequestAsync` is a custom extension to avoid a name collision with FluentValidation's own `ValidateAsync`.

---

## Authentication

### HttpOnly Cookie + JWT Access Token

Token security follows a two-token model:

| Token | Transport | JS accessible | Storage |
|-------|-----------|---------------|---------|
| Access token | `Authorization: Bearer` header | Yes (attached by axios) | Browser memory only |
| Refresh token | HttpOnly cookie | **No** | Cookie only; SHA-256 hash in DB |

Cookie settings: `HttpOnly=true`, `Secure=true`, `SameSite=None`, `Path=/api/v1/users`.

**Login flow:**
1. Email + password are verified
2. User's roles and permissions are loaded from the database
3. A short-lived access token is generated (default: 60 min) — contains `ClaimTypes.Role` and `"permission"` claims
4. A long-lived refresh token is generated (default: 7 days); its **SHA-256 hash** is stored in the database, the raw value is set as an HttpOnly cookie via `RefreshTokenCookieHelper.Append()`
5. The `AuthResponse.RefreshToken` field is `[JsonIgnore]` — it travels handler → endpoint internally but is **never serialized** to the response body
6. The client stores the access token in memory; the browser stores the cookie automatically

**Token refresh flow:**
1. Client sends `POST /api/v1/users/refresh-token` with no body — the browser sends the cookie automatically
2. Endpoint reads `Request.Cookies["refresh-token"]`; returns `401` + clears cookie if missing
3. SHA-256 hash of the raw token is looked up in the database
4. Token is valid if `!IsRevoked && !IsExpired && !IsDeleted`
5. Existing token is **revoked** (rotation) — new access token + new cookie are issued

**Logout flow:**
1. Client sends `POST /api/v1/users/logout` (authenticated)
2. Endpoint reads the refresh token cookie, hashes it, revokes the matching DB record if found
3. `RefreshTokenCookieHelper.Clear()` overwrites the cookie with `MaxAge=0` — browser deletes it
4. On the frontend, the access token is cleared from memory

**Refresh token cleanup:**
A Quartz job runs nightly at 03:00 and deletes expired and revoked tokens via `ExecuteDeleteAsync`.

### RefreshTokenCookieHelper

Centralised static class in `Common/Services/` that owns all cookie configuration to prevent option mismatches across the three auth endpoints. Pass `env.IsDevelopment()` so the cookie is configured correctly for the current environment:

```csharp
RefreshTokenCookieHelper.Append(httpContext.Response, rawToken, expiryDays, env.IsDevelopment());
RefreshTokenCookieHelper.Clear(httpContext.Response, env.IsDevelopment());
```

| Environment | `SameSite` | `Secure` | Reason |
|-------------|-----------|---------|--------|
| Development | `Lax` | `false` | Plain HTTP on localhost; `SameSite=None` requires HTTPS and would be silently dropped by Firefox/Safari |
| Production | `None` | `true` | Cross-origin frontend/backend; HTTPS required |

### JWT for SignalR

WebSocket connections cannot send HTTP headers. The `OnMessageReceived` event reads the token from the `?access_token=` query string for `/hubs` paths.

---

## User API Endpoints

| Method | Path | Auth | Description |
|--------|------|------|-------------|
| POST | `/api/v1/users/register` | — | Create account |
| POST | `/api/v1/users/login` | — | Returns access token; sets refresh cookie |
| POST | `/api/v1/users/refresh-token` | — (cookie) | Rotates tokens |
| POST | `/api/v1/users/logout` | Required | Revokes refresh token + clears cookie |
| GET | `/api/v1/users/me` | Required | Current user profile |
| PUT | `/api/v1/users/me` | Required | Update profile (firstName, lastName, avatarUrl) |
| GET | `/api/v1/users` | `users.view` | Paginated user list with roles (page, pageSize, search, sortBy, sortDesc) |
| GET | `/api/v1/users/{userId}` | `users.view` | Single user with roles |

---

## Permission System

### Structure

```
IdentityRole<Guid>  ─┐
                      ├─ RolePermission (junction)
Permission          ─┘
```

- **`Permission`**: A permission constant (`"users.view"`, `"roles.manage"`, etc.) — not a `BaseEntity`, no soft-delete
- **`RolePermission`**: Role–Permission link, composite PK `(RoleId, PermissionId)`
- **`IdentityRole<Guid>`**: ASP.NET Core Identity's standard role system

### Permission Constants

All permission names are defined in nested static classes inside `Common/Authorization/Permissions.cs`:

```csharp
public static class Permissions
{
    public const string AdminRole = "Admin"; // built-in role, cannot be deleted

    public static class Users
    {
        public const string View   = "users.view";
        public const string Create = "users.create";
        public const string Edit   = "users.edit";
        public const string Delete = "users.delete";
    }

    public static class Roles
    {
        public const string View   = "roles.view";
        public const string Manage = "roles.manage";
    }

    // Returns all constants via reflection — used by PermissionSyncService
    public static IEnumerable<string> GetAll() => ...
}
```

To **add a new module**, simply add a new nested class. `GetAll()` discovers it automatically via reflection.

### PermissionSyncService

An `IHostedService` that runs on application startup:

1. Collects all permission constants in code via `Permissions.GetAll()`
2. Inserts any missing ones into the `Permissions` table (idempotent)
3. Creates the `Admin` role if it does not exist
4. Assigns all permissions to the Admin role

Code changes are reflected in the database automatically — no migration or seed script needed.

```
Startup
  └─ PermissionSyncService.StartAsync()
       ├─ SyncPermissionsAsync()    → insert missing permissions
       └─ EnsureAdminRoleAsync()   → create Admin role / assign all permissions
```

If an error occurs (e.g. database unreachable), the application **does not crash** — the error is logged and startup continues.

### Permission Claims in JWT

During login/token refresh, permissions are loaded from the user's roles and embedded in the access token:

```csharp
// JwtTokenService.GenerateAccessToken
claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));
claims.AddRange(permissions.Select(p => new Claim("permission", p)));
```

### Endpoint Protection

```csharp
.RequirePermission(Permissions.Roles.Manage)

// Expands to:
builder.RequireAuthorization(p => p.RequireClaim("permission", "roles.manage"));
```

Authentication + specific claim check combined in one call.

### Role API Endpoints

| Method | Path | Permission | Description |
|--------|------|------------|-------------|
| GET | `/api/v1/roles` | `roles.view` | All roles with their permissions |
| GET | `/api/v1/roles/permissions` | `roles.view` | All available permissions |
| POST | `/api/v1/roles` | `roles.manage` | Create a new role |
| DELETE | `/api/v1/roles/{roleId}` | `roles.manage` | Delete a role (Admin protected) |
| POST | `/api/v1/roles/{roleId}/permissions` | `roles.manage` | Assign permission to role |
| DELETE | `/api/v1/roles/{roleId}/permissions/{permId}` | `roles.manage` | Remove permission from role |
| POST | `/api/v1/users/{userId}/roles` | `roles.manage` | Assign role to user |
| DELETE | `/api/v1/users/{userId}/roles/{roleName}` | `roles.manage` | Remove role from user |

---

## Audit Trail

### BaseEntity

All entities that inherit from `BaseEntity` automatically get audit fields:

```csharp
public abstract class BaseEntity
{
    public Guid Id { get; }           // auto NewGuid
    public DateTime CreatedAt { get; }
    public Guid? CreatedBy { get; }   // user ID who performed the action
    public DateTime? UpdatedAt { get; }
    public Guid? UpdatedBy { get; }
    public bool IsDeleted { get; }    // soft-delete flag
}
```

### Automatic Population

`AppDbContext.SaveChangesAsync` is overridden. On every save, the current user is obtained via `ICurrentUserService`:

```csharp
// Added   → SetCreated(userId)
// Modified → SetUpdated(userId)
```

In background contexts like `PermissionSyncService`, there is no user (`UserId = null`); audit fields are left empty — this is intentional.

### ApplicationUser Special Case

`ApplicationUser` inherits from `IdentityUser<Guid>` and does **not** use `BaseEntity`. It carries its own `CreatedAt`, `UpdatedAt`, and `IsDeleted` fields, with a separate query filter applied in `ApplicationUserConfiguration`.

---

## Soft Delete

For entities inheriting from `BaseEntity`, a global query filter is applied automatically via reflection in `AppDbContext.OnModelCreating`:

```csharp
// For every BaseEntity subclass:
builder.Entity<T>().HasQueryFilter(e => !e.IsDeleted);
```

Result: `WHERE IsDeleted = false` is always appended to queries automatically.

When access to deleted records is needed (e.g. refresh token cleanup):

```csharp
_db.RefreshTokens.IgnoreQueryFilters().Where(...)
```

---

## SignalR

### AppHub

```csharp
[Authorize]
public class AppHub : Hub
{
    // On connect, the user automatically joins their personal group: "user-{userId}"
    // Client-callable methods:
    Task JoinGroupAsync(string groupName)   // join a named group (e.g. "board-42")
    Task LeaveGroupAsync(string groupName)  // leave a group
}
```

Hub address: `wss://host/hubs/app?access_token=<jwt>`

### INotificationService

Server-side code never uses SignalR directly; it sends messages through this abstraction:

```csharp
public interface INotificationService
{
    Task SendToUserAsync(string userId, string eventName, object payload, CancellationToken ct = default);
    Task SendToGroupAsync(string group, string eventName, object payload, CancellationToken ct = default);
    Task BroadcastAsync(string eventName, object payload, CancellationToken ct = default);
}
```

Usage from a handler or service:

```csharp
await _notificationService.SendToUserAsync(userId, "OrderUpdated", new { orderId, status });
```

---

## Background Jobs (Quartz.NET)

Default job: `CleanupExpiredRefreshTokensJob`

- Runs nightly at 03:00 (cron: `0 0 3 * * ?`)
- Deletes all expired or revoked refresh tokens directly via `ExecuteDeleteAsync`
- `[DisallowConcurrentExecution]` ensures only one instance runs at a time

The cron expression is configurable via `appsettings.json`:

```json
"Quartz": {
  "CleanupExpiredRefreshTokensCron": "0 0 3 * * ?"
}
```

---

## Email (MailKit)

```csharp
public interface IEmailService
{
    Task SendAsync(string to, string subject, string htmlBody, CancellationToken ct);
    Task SendAsync(IEnumerable<string> to, string subject, string htmlBody, CancellationToken ct);
}
```

`appsettings.json` configuration:

```json
"Email": {
  "Host": "smtp.example.com",
  "Port": 587,
  "EnableSsl": true,      // set to false for local dev SMTP (e.g. MailHog on port 1025)
  "Username": "...",      // if empty, AuthenticateAsync is skipped
  "Password": "...",
  "FromAddress": "noreply@example.com",
  "FromName": "MyApp"
}
```

---

## File Storage (S3)

```csharp
public interface IFileService
{
    Task<string> UploadAsync(Stream fileStream, string fileName, string contentType, CancellationToken ct);
    Task<Stream> DownloadAsync(string fileKey, CancellationToken ct);
    Task DeleteAsync(string fileKey, CancellationToken ct);
    Task<string> GetPublicUrlAsync(string fileKey, CancellationToken ct);
}
```

`HetznerFileService` works with Hetzner Object Storage (S3-compatible) and MinIO. Changing `ServiceUrl` allows switching to any other S3-compatible provider.

File key format: `{newGuid}/{originalFileName}` — safe against directory traversal attacks.

---

## Logging (Serilog)

A bootstrap logger starts before the application; it captures startup crashes too:

```csharp
Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateBootstrapLogger();
try { /* application */ }
catch (Exception ex) when (ex is not HostAbortedException)
{ Log.Fatal(ex, "Application terminated unexpectedly"); }
finally { Log.CloseAndFlush(); }
```

`RequestLoggingMiddleware` logs every HTTP request in `method path → status latency` format. Sensitive fields (`password`, `token`, etc.) are never written to logs.

Configuration is managed via `appsettings.json`:

```json
"Serilog": {
  "MinimumLevel": { "Default": "Information" },
  "WriteTo": [
    { "Name": "Console" },
    { "Name": "File", "Args": { "path": "logs/log-.txt", "rollingInterval": "Day" } }
  ]
}
```

---

## Rate Limiting

Two fixed-window policies. Limits are configurable via `appsettings.json` with the defaults shown below:

| Policy | Config key | Default | Applied to |
|--------|-----------|---------|------------|
| `"default"` | `RateLimit:Default:PermitLimit` | 60 req/min | All `/api/v1` endpoints |
| `"auth"` | `RateLimit:Auth:PermitLimit` | 10 req/min | Login, Register, RefreshToken |

Exceeded: `429 Too Many Requests`

In integration tests the limits are raised to 10 000 via `appsettings` overrides in `ApiWebApplicationFactory`, so tests never get throttled.

---

## Health Checks

`GET /health` — JSON response:

```json
{
  "status": "Healthy",
  "checks": [
    { "name": "sqlserver", "status": "Healthy", "description": null },
    { "name": "disk",      "status": "Healthy", "description": null }
  ]
}
```

| Check | Condition |
|-------|-----------|
| `sqlserver` | SQL Server connection |
| `disk` | Minimum 1 GB free on `/` |

---

## CORS

The application will not start if `AllowedOrigins` is empty — misconfiguration is never silently ignored:

```json
"Cors": {
  "AllowedOrigins": ["https://your-frontend-domain.com"]
}
```

`AllowCredentials()` is enabled — required for cookies and SignalR WebSocket connections.

---

## Configuration Files

Two `appsettings` files are used. This is the standard ASP.NET Core convention:

| File | Purpose |
|------|---------|
| `appsettings.json` | Production defaults (no secrets, no connection strings) |
| `appsettings.Development.json` | Local dev overrides (localhost DB, MinIO, MailHog, debug logging) |

In production, all secrets are supplied via environment variables (see Production Deployment below). There is no `appsettings.Production.json` — environment variables override `appsettings.json` at runtime.

---

## Testing

### Integration Tests (`MyApp.Api.Tests`)

The test project uses **WebApplicationFactory** + **Testcontainers** to run the full application stack against a real SQL Server container spun up automatically during the test run.

**Prerequisites:** Docker must be running.

```bash
dotnet test MyApp.Api.Tests
```

What's covered out of the box:

| Test class | Scenarios |
|------------|-----------|
| `LoginTests` | Valid credentials → JWT + cookie; wrong password/email → 401; empty body → 400; refresh token not in response body |
| `RefreshTokenTests` | Token rotation after login; no cookie → 401; refresh after logout → 401 |
| `RegisterTests` | Valid registration; duplicate email → 400; weak password → 400; missing fields → 400 |
| `GetUsersTests` | Paginated list with valid token → 200; no token → 401; get by ID |

### Test Architecture

- `ApiWebApplicationFactory` — single SQL Server container shared across all tests in the `"Api"` collection (fast startup, one container per suite run)
- `appsettings` overrides inject the testcontainer connection string, predictable seed credentials, and elevated rate-limit ceilings
- `Helpers/AuthHelpers.cs` — `GetAdminTokenAsync`, `CreateAuthenticatedClientAsync` shared across test classes

Add new test classes to `Auth/`, `Users/`, `Roles/`, etc. Use `[Collection("Api")]` and inject `ApiWebApplicationFactory` to share the container.

---

## Development Setup

### Prerequisites

- .NET 10 SDK
- Docker + Docker Compose

### Quick Start

```bash
# Copy and fill in environment variables
cp .env.example .env

# Start SQL Server + MinIO + MailHog
docker compose --profile dev up -d

# Start the API (migrations run automatically on startup)
dotnet run --project MyApp.Api
```

> **Auto-migration**: On `Development` environment, `db.Database.MigrateAsync()` runs automatically at startup. There is no need to run `dotnet ef database update` manually during development.
>
> For new migrations, still use the EF Core CLI to generate them:
> ```bash
> dotnet ef migrations add <MigrationName> --project MyApp.Api
> ```

| Service | Address | Profile |
|---------|---------|---------|
| API | http://localhost:8080 | always |
| Scalar (API docs) | http://localhost:8080/scalar/v1 | always |
| Health | http://localhost:8080/health | always |
| SignalR Hub | ws://localhost:8080/hubs/app | always |
| MinIO S3 API | http://localhost:9000 | `--profile dev` |
| MinIO console | http://localhost:9001 | `--profile dev` |
| MailHog UI | http://localhost:8025 | `--profile dev` |

### First Admin User

In `Development`, a default admin user is seeded automatically on first startup via `DevSeeder`. Credentials are configured in `appsettings.Development.json`:

```json
"Seed": {
  "AdminEmail": "admin@example.com",
  "AdminPassword": "Admin123!"
}
```

The seeder runs after migrations and is fully idempotent — if the user already exists it does nothing. Change the email/password to whatever suits your local setup.

In production no seeding happens. Create the first admin user by registering normally, then assign the `Admin` role directly in the database once.

---

## Production Deployment

### Environment Variables

```bash
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=Server=...;Database=...;User Id=sa;Password=...;TrustServerCertificate=True
Jwt__Secret=<minimum 32 characters>
Jwt__Issuer=MyApp
Jwt__Audience=MyApp
Cors__AllowedOrigins__0=https://your-frontend.com
```

### Docker

```bash
# Build image
docker build -f MyApp.Api/Dockerfile -t myapp-api .

# Full stack with docker-compose
docker compose up -d
```

---

## Adding a New Feature

Example: `CreateProduct` feature

```
Features/Products/CreateProduct/
├── CreateProductCommand.cs   → public class CreateProductCommand { ... }
├── CreateProductValidator.cs → AbstractValidator<CreateProductCommand>
├── CreateProductHandler.cs   → ICommandHandler<CreateProductCommand, ProductResponse>
└── CreateProductEndpoint.cs  → static void Map(RouteGroupBuilder group)
```

1. **Add a permission** (`Permissions.cs`):
   ```csharp
   public static class Products
   {
       public const string Create = "products.create";
   }
   ```
   On next startup, `PermissionSyncService` automatically inserts the new permission into the database.

2. **Add the route group** (`FeaturesExtensions.MapFeatures`) — **only if this is a brand-new group**:
   ```csharp
   ["products"] = apiV1.MapGroup("/products"),
   ```
   If the endpoint maps to an existing group (e.g. `"users"`) this step is not needed.

3. **Create the feature files** — that's it. The source generator (`MyApp.SourceGenerators`) scans for:
   - Classes implementing `ICommandHandler<,>` or `IQueryHandler<,>` → registered in DI automatically
   - Endpoint classes decorated with `[MapToGroup("products")]` → `Map()` called automatically

   No manual `AddScoped<>` or `Endpoint.Map()` calls are needed.

---

## Database

This project uses **SQL Server** (via `Microsoft.EntityFrameworkCore.SqlServer`). The `docker-compose.yml` includes a SQL Server 2022 container for local development.

---

## Key Technical Decisions

- No **AutoMapper** — Mapperly uses source-generation: no reflection, compile-time safe
- No **MediatR** — handlers resolved directly via DI, no pipeline overhead
- No **Hangfire** — Quartz.NET is preferred (works in environments with restricted DB permissions)
- No **Swashbuckle** — Scalar + ASP.NET Core built-in OpenAPI
- `AppDbContext.SaveChangesAsync` is overridden; inheriting from `BaseEntity` is sufficient to get audit fields automatically
- All `DateTime` values are UTC
- Refresh tokens are stored as SHA-256 hashes in the database; the raw value is never persisted
- Refresh token is delivered exclusively via HttpOnly cookie — it is `[JsonIgnore]` on `AuthResponse` and never appears in a response body
