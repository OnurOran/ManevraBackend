using MyApp.Api.Domain.Entities.Manevra;

namespace MyApp.Api.Common.Authorization;

/// <summary>Maps zone sections and actions to permission strings.</summary>
public static class ZonePermissions
{
    public static string? GetZonePrefix(SectionType? section) => section switch
    {
        SectionType.BasMakasYonu  => "bas-makas",
        SectionType.ItfaiyeYonu   => "itfaiye-yonu",
        SectionType.ItfaiyeTaraf  => "itfaiye-taraf",
        SectionType.AtolyeYollari => "atolye-yollari",
        SectionType.YikamaTaraf   => "yikama-taraf",
        SectionType.HazirDiziler  => "cari-hat",
        null                      => "vagon-listesi",
        _                         => null,
    };

    public static string? GetStatusAction(WagonStatus status) => status switch
    {
        WagonStatus.Servis            => "servis",
        WagonStatus.CalismaYapilacak  => "calisma-yapilacak",
        WagonStatus.ServiseHazir      => "servise-hazir",
        _                             => null,
    };

    public static string? GetPermissionForStatusChange(SectionType? section, WagonStatus targetStatus)
    {
        var prefix = GetZonePrefix(section);
        var action = GetStatusAction(targetStatus);
        return prefix is not null && action is not null ? $"{prefix}.{action}" : null;
    }

    public static string? GetPermissionForAction(SectionType? section, string action)
    {
        var prefix = GetZonePrefix(section);
        return prefix is not null ? $"{prefix}.{action}" : null;
    }

    /// <summary>Checks if the current user has the given permission claim.</summary>
    public static bool UserHasPermission(System.Security.Claims.ClaimsPrincipal? user, string permission)
        => user?.HasClaim("permission", permission) ?? false;
}
