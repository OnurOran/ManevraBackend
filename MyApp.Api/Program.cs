using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using MyApp.Api.Common.Extensions;
using MyApp.Api.Infrastructure.Persistence;
using MyApp.Api.Features._Shared;
using MyApp.Api.Infrastructure.Hubs;
using Scalar.AspNetCore;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((ctx, lc) =>
        lc.ReadFrom.Configuration(ctx.Configuration));

    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddAuth(builder.Configuration);
    builder.Services.AddCorsPolicy(builder.Configuration);
    builder.Services.AddRateLimiting(builder.Configuration);
    builder.Services.AddHealthChecks(builder.Configuration);
    builder.Services.AddFeatures();

    builder.Services.AddOpenApi();

    var app = builder.Build();

    app.UseExceptionHandler();
    app.UseHttpsRedirection();
    app.UseCors(CorsExtensions.PolicyName);

    if (app.Environment.IsDevelopment())
    {
        // Auto-apply pending EF Core migrations on startup so developers never
        // have to run "dotnet ef database update" manually.
        // MigrateAsync() is idempotent — safe to call even when schema is current.
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.Database.MigrateAsync();
        }

        await DevSeeder.SeedAsync(app.Services, app.Configuration);
        await ManevraSeeder.SeedAsync(app.Services);

        app.MapOpenApi();
        app.MapScalarApiReference(options =>
        {
            options.WithTitle("MyApp API");
            options.WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
        });
    }

    app.UseMiddleware<RequestLoggingMiddleware>();
    app.UseRateLimiter();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapHealthChecks("/health", new HealthCheckOptions
    {
        ResponseWriter = async (ctx, report) =>
        {
            ctx.Response.ContentType = "application/json";
            var result = System.Text.Json.JsonSerializer.Serialize(new
            {
                status = report.Status.ToString(),
                checks = report.Entries.Select(e => new
                {
                    name = e.Key,
                    status = e.Value.Status.ToString(),
                    description = e.Value.Description
                })
            });
            await ctx.Response.WriteAsync(result);
        }
    });

    app.MapHub<AppHub>("/hubs/app");
    app.MapFeatures();

    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

// Expose Program to the integration test project (WebApplicationFactory<Program>).
public partial class Program { }
