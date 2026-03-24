namespace MyApp.Api.Common.Extensions;

/// <summary>
/// Registers all feature handlers into DI and maps all endpoints to routes.
///
/// Handler DI registrations and endpoint Map() calls are generated automatically
/// by MyApp.SourceGenerators — do not add them here manually.
///
/// When adding a new feature:
///   1. Implement ICommandHandler&lt;,&gt; or IQueryHandler&lt;,&gt; → auto-registered in DI.
///   2. Add [MapToGroup("groupName")] to the endpoint class → auto-mapped.
///   3. If you need a new route group (e.g. "products"), add it to the groups
///      dictionary below — that is the only manual step.
/// </summary>
public static partial class FeaturesExtensions
{
    public static IServiceCollection AddFeatures(this IServiceCollection services)
    {
        AddGeneratedHandlers(services);
        return services;
    }

    public static WebApplication MapFeatures(this WebApplication app)
    {
        var apiV1 = app.MapGroup("/api/v1").RequireRateLimiting("default");

        // Route group configuration lives here — this is intentionally manual.
        // Groups carry per-group settings (auth, tags, rate limits) that are
        // project-specific and should not be inferred by the generator.
        //
        // To add a new group: add a new entry to this dictionary.
        // The key must match the string passed to [MapToGroup("...")] on the endpoint.
        var groups = new Dictionary<string, RouteGroupBuilder>
        {
            ["users"] = apiV1.MapGroup("/users"),
            ["roles"] = apiV1.MapGroup("/roles"),
            ["wagons"] = apiV1.MapGroup("/wagons"),
            ["manevra"] = apiV1.MapGroup("/manevra"),
            ["cleanup"] = apiV1.MapGroup("/cleanup"),
            ["weekly-maintenance"] = apiV1.MapGroup("/weekly-maintenance"),
            ["dead-wagons"] = apiV1.MapGroup("/dead-wagons"),
        };

        MapGeneratedEndpoints(groups);
        return app;
    }

    // -------------------------------------------------------------------------
    // Partial method declarations — implemented by the source generator.
    // Do not implement these manually.
    // -------------------------------------------------------------------------

    static partial void AddGeneratedHandlers(IServiceCollection services);
    static partial void MapGeneratedEndpoints(IReadOnlyDictionary<string, RouteGroupBuilder> groups);
}
