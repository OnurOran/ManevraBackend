using System.Reflection;

namespace MyApp.Api.Common.Authorization;

/// <summary>All permission constants used across the application. Add new nested classes per module.</summary>
public static class Permissions
{
    /// <summary>Built-in role name that always has all permissions. Cannot be deleted.</summary>
    public const string AdminRole = "Admin";

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

    public const string OfficeRole = "Office";
    public const string FieldRole = "Field";

    /// <summary>Returns every permission constant defined in this class via reflection.</summary>
    public static IEnumerable<string> GetAll() =>
        typeof(Permissions)
            .GetNestedTypes(BindingFlags.Public | BindingFlags.Static)
            .SelectMany(t => t.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy))
            .Where(f => f.IsLiteral && !f.IsInitOnly && f.FieldType == typeof(string))
            .Select(f => (string)f.GetRawConstantValue()!);
}
