using Microsoft.CodeAnalysis;

namespace MyApp.SourceGenerators;

internal static class DiagnosticDescriptors
{
    /// <summary>
    /// Emitted when [MapToGroup] is placed on a class that has no static Map(RouteGroupBuilder) method.
    /// </summary>
    public static readonly DiagnosticDescriptor MissingMapMethod = new(
        id: "MYAPP001",
        title: "Missing Map method",
        messageFormat: "'{0}' has [MapToGroup] but no 'public static void Map(RouteGroupBuilder)' method",
        category: "MyApp.SourceGenerators",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    /// Emitted when [MapToGroup] is given an empty or whitespace group name.
    /// </summary>
    public static readonly DiagnosticDescriptor EmptyGroupName = new(
        id: "MYAPP002",
        title: "Empty group name",
        messageFormat: "'{0}' has [MapToGroup] with an empty group name",
        category: "MyApp.SourceGenerators",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);
}
