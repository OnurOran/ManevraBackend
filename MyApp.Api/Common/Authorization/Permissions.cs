using System.Reflection;

namespace MyApp.Api.Common.Authorization;

/// <summary>All permission constants used across the application. Add new nested classes per module.</summary>
public static class Permissions
{
    // ── Role names ────────────────────────────────────────────────────────────
    /// <summary>Built-in role that always has all permissions. Cannot be deleted.</summary>
    public const string SuperAdminRole = "SuperAdmin";
    public const string AdminRole = "Admin";
    public const string KumandaMerkeziRole = "KumandaMerkezi";
    public const string ManevraciRole = "Manevraci";
    public const string HatVardiyaAmiriRole = "HatVardiyaAmiri";
    public const string SefRole = "Sef";
    public const string AtolyePersoneliRole = "AtolyePersoneli";

    // ── Admin panel permissions ───────────────────────────────────────────────
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

    // ── General operational permissions (endpoint-level auth) ─────────────────
    public static class Manevra
    {
        public const string View    = "manevra.view";
        public const string Edit    = "manevra.edit";
        public const string Approve = "manevra.approve";
    }

    public static class Wagons
    {
        public const string View   = "wagons.view";
        public const string Create = "wagons.create";
        public const string Edit   = "wagons.edit";
        public const string Delete = "wagons.delete";
    }

    public static class Cleanup
    {
        public const string View = "cleanup.view";
        public const string Edit = "cleanup.edit";
    }

    public static class WeeklyMaintenance
    {
        public const string View = "weekly-maintenance.view";
        public const string Edit = "weekly-maintenance.edit";
    }

    public static class Field
    {
        public const string View = "field.view";
    }

    // ── Zone-specific permissions (fine-grained per area + action) ────────────

    public static class VagonListesi
    {
        public const string Servis           = "vagon-listesi.servis";
        public const string CalismaYapilacak = "vagon-listesi.calisma-yapilacak";
        public const string ServiseHazir     = "vagon-listesi.servise-hazir";
        public const string DiziyiBoz        = "vagon-listesi.diziyi-boz";
        public const string Ozellikler       = "vagon-listesi.ozellikler";
        public const string Surukle          = "vagon-listesi.surukle";
        public const string Goruntule        = "vagon-listesi.goruntule";
        public const string Yaz              = "vagon-listesi.yaz";
    }

    public static class Temizlik
    {
        public const string Servis           = "temizlik.servis";
        public const string CalismaYapilacak = "temizlik.calisma-yapilacak";
        public const string ServiseHazir     = "temizlik.servise-hazir";
        public const string DiziyiBoz        = "temizlik.diziyi-boz";
        public const string Ozellikler       = "temizlik.ozellikler";
        public const string Surukle          = "temizlik.surukle";
        public const string Goruntule        = "temizlik.goruntule";
        public const string Yaz              = "temizlik.yaz";
    }

    public static class CariHat
    {
        public const string Servis           = "cari-hat.servis";
        public const string CalismaYapilacak = "cari-hat.calisma-yapilacak";
        public const string ServiseHazir     = "cari-hat.servise-hazir";
        public const string DiziyiBoz        = "cari-hat.diziyi-boz";
        public const string Ozellikler       = "cari-hat.ozellikler";
        public const string Surukle          = "cari-hat.surukle";
        public const string Goruntule        = "cari-hat.goruntule";
        public const string Yaz              = "cari-hat.yaz";
    }

    public static class BasMakas
    {
        public const string Servis           = "bas-makas.servis";
        public const string CalismaYapilacak = "bas-makas.calisma-yapilacak";
        public const string ServiseHazir     = "bas-makas.servise-hazir";
        public const string DiziyiBoz        = "bas-makas.diziyi-boz";
        public const string Ozellikler       = "bas-makas.ozellikler";
        public const string Surukle          = "bas-makas.surukle";
        public const string Goruntule        = "bas-makas.goruntule";
        public const string Yaz              = "bas-makas.yaz";
    }

    public static class ItfaiyeYonu
    {
        public const string Servis           = "itfaiye-yonu.servis";
        public const string CalismaYapilacak = "itfaiye-yonu.calisma-yapilacak";
        public const string ServiseHazir     = "itfaiye-yonu.servise-hazir";
        public const string DiziyiBoz        = "itfaiye-yonu.diziyi-boz";
        public const string Ozellikler       = "itfaiye-yonu.ozellikler";
        public const string Surukle          = "itfaiye-yonu.surukle";
        public const string Goruntule        = "itfaiye-yonu.goruntule";
        public const string Yaz              = "itfaiye-yonu.yaz";
    }

    public static class AtolyeYollari
    {
        public const string Servis           = "atolye-yollari.servis";
        public const string CalismaYapilacak = "atolye-yollari.calisma-yapilacak";
        public const string ServiseHazir     = "atolye-yollari.servise-hazir";
        public const string DiziyiBoz        = "atolye-yollari.diziyi-boz";
        public const string Ozellikler       = "atolye-yollari.ozellikler";
        public const string Surukle          = "atolye-yollari.surukle";
        public const string Goruntule        = "atolye-yollari.goruntule";
        public const string Yaz              = "atolye-yollari.yaz";
    }

    public static class ItfaiyeTaraf
    {
        public const string Servis           = "itfaiye-taraf.servis";
        public const string CalismaYapilacak = "itfaiye-taraf.calisma-yapilacak";
        public const string ServiseHazir     = "itfaiye-taraf.servise-hazir";
        public const string DiziyiBoz        = "itfaiye-taraf.diziyi-boz";
        public const string Ozellikler       = "itfaiye-taraf.ozellikler";
        public const string Surukle          = "itfaiye-taraf.surukle";
        public const string Goruntule        = "itfaiye-taraf.goruntule";
        public const string Yaz              = "itfaiye-taraf.yaz";
    }

    public static class YikamaTaraf
    {
        public const string Servis           = "yikama-taraf.servis";
        public const string CalismaYapilacak = "yikama-taraf.calisma-yapilacak";
        public const string ServiseHazir     = "yikama-taraf.servise-hazir";
        public const string DiziyiBoz        = "yikama-taraf.diziyi-boz";
        public const string Ozellikler       = "yikama-taraf.ozellikler";
        public const string Surukle          = "yikama-taraf.surukle";
        public const string Goruntule        = "yikama-taraf.goruntule";
        public const string Yaz              = "yikama-taraf.yaz";
    }

    public static class ServisDisi
    {
        public const string Servis           = "servis-disi.servis";
        public const string CalismaYapilacak = "servis-disi.calisma-yapilacak";
        public const string ServiseHazir     = "servis-disi.servise-hazir";
        public const string DiziyiBoz        = "servis-disi.diziyi-boz";
        public const string Ozellikler       = "servis-disi.ozellikler";
        public const string Surukle          = "servis-disi.surukle";
        public const string Goruntule        = "servis-disi.goruntule";
        public const string Yaz              = "servis-disi.yaz";
    }

    public static class HaftalikBakim
    {
        public const string Servis           = "haftalik-bakim.servis";
        public const string CalismaYapilacak = "haftalik-bakim.calisma-yapilacak";
        public const string ServiseHazir     = "haftalik-bakim.servise-hazir";
        public const string DiziyiBoz        = "haftalik-bakim.diziyi-boz";
        public const string Ozellikler       = "haftalik-bakim.ozellikler";
        public const string Surukle          = "haftalik-bakim.surukle";
        public const string Goruntule        = "haftalik-bakim.goruntule";
        public const string Yaz              = "haftalik-bakim.yaz";
    }

    public static class HaftalikTorna
    {
        public const string Servis           = "haftalik-torna.servis";
        public const string CalismaYapilacak = "haftalik-torna.calisma-yapilacak";
        public const string ServiseHazir     = "haftalik-torna.servise-hazir";
        public const string DiziyiBoz        = "haftalik-torna.diziyi-boz";
        public const string Ozellikler       = "haftalik-torna.ozellikler";
        public const string Surukle          = "haftalik-torna.surukle";
        public const string Goruntule        = "haftalik-torna.goruntule";
        public const string Yaz              = "haftalik-torna.yaz";
    }

    /// <summary>Returns every permission constant defined in this class via reflection.</summary>
    public static IEnumerable<string> GetAll() =>
        typeof(Permissions)
            .GetNestedTypes(BindingFlags.Public | BindingFlags.Static)
            .SelectMany(t => t.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy))
            .Where(f => f.IsLiteral && !f.IsInitOnly && f.FieldType == typeof(string))
            .Select(f => (string)f.GetRawConstantValue()!);
}
