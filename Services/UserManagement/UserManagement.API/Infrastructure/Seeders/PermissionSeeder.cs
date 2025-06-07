using System.Collections.Frozen;

namespace UserManagement.API.Infrastructure.Seeders
{
    /// <summary>
    /// Constants فقط برای Seeding استفاده میشه - نه Runtime!
    /// </summary>
    public static class PermissionConstants
    {
        //  File-scoped static class with readonly properties
        public static class Modules
        {
            public const string User = "User";
            public const string Role = "Role";
            public const string Flight = "Flight";
            public const string Wallet = "Wallet";
        }

        public static class Actions
        {
            // User Actions
            public const string View = "View";
            public const string Edit = "Edit";
            public const string Delete = "Delete";
            public const string Activate = "Activate";
            public const string AssignRole = "AssignRole";
            public const string UnassignRole = "UnassignRole";
            public const string ViewRoles = "ViewRoles";

            // Role Actions
            public const string Create = "Create";
            public const string AssignPermission = "AssignPermission";

            // Wallet Actions  
            public const string Charge = "Charge";

            // Flight Actions
            // اضافه میشه...
        }
    }
    public static class PermissionSeeder
    {
        public static List<Permission> Seed() =>
    [
        // User Management
        new() { Module = PermissionConstants.Modules.User, Action = PermissionConstants.Actions.View, Description = "مشاهده کاربران" },
        new() { Module = PermissionConstants.Modules.User, Action = PermissionConstants.Actions.Edit, Description = "ویرایش کاربران" },
        new() { Module = PermissionConstants.Modules.User, Action = PermissionConstants.Actions.Delete, Description = "حذف کاربران" },
        new() { Module = PermissionConstants.Modules.User, Action = PermissionConstants.Actions.Activate, Description = "فعال/غیرفعال کردن کاربران" },
        
        // User Role Management
        new() { Module = PermissionConstants.Modules.User, Action = PermissionConstants.Actions.AssignRole, Description = "اختصاص نقش به کاربر" },
        new() { Module = PermissionConstants.Modules.User, Action = PermissionConstants.Actions.UnassignRole, Description = "حذف نقش از کاربر" },
        new() { Module = PermissionConstants.Modules.User, Action = PermissionConstants.Actions.ViewRoles, Description = "مشاهده نقش‌های کاربر" },

        // Role Management
        new() { Module = PermissionConstants.Modules.Role, Action = PermissionConstants.Actions.Create, Description = "ایجاد نقش" },
        new() { Module = PermissionConstants.Modules.Role, Action = PermissionConstants.Actions.Edit, Description = "ویرایش نقش" },
        new() { Module = PermissionConstants.Modules.Role, Action = PermissionConstants.Actions.Delete, Description = "حذف نقش" },
        new() { Module = PermissionConstants.Modules.Role, Action = PermissionConstants.Actions.AssignPermission, Description = "اختصاص دسترسی به نقش" },

        // Flight Management
        new() { Module = PermissionConstants.Modules.Flight, Action = PermissionConstants.Actions.View, Description = "مشاهده پروازها" },
        new() { Module = PermissionConstants.Modules.Flight, Action = PermissionConstants.Actions.Create, Description = "ایجاد پرواز" },

        // Wallet Management
        new() { Module = PermissionConstants.Modules.Wallet, Action = PermissionConstants.Actions.View, Description = "مشاهده کیف‌پول" },
        new() { Module = PermissionConstants.Modules.Wallet, Action = PermissionConstants.Actions.Charge, Description = "شارژ کیف‌پول" }
    ];

        //  Alternative method with Frozen collections for better performance
        public static FrozenSet<Permission> GetFrozenPermissions() =>
            Seed().ToFrozenSet();
    }
}