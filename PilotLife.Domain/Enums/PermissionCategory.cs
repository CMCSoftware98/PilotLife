namespace PilotLife.Domain.Enums;

/// <summary>
/// Permission categories for role-based access control.
/// </summary>
public enum PermissionCategory
{
    // User Management
    /// <summary>View user profiles and data.</summary>
    Users_View,
    /// <summary>Edit user profiles and data.</summary>
    Users_Edit,
    /// <summary>Ban users from the platform.</summary>
    Users_Ban,
    /// <summary>Delete user accounts.</summary>
    Users_Delete,

    // World Management
    /// <summary>View world details.</summary>
    Worlds_View,
    /// <summary>Edit world properties.</summary>
    Worlds_Edit,
    /// <summary>Create new worlds.</summary>
    Worlds_Create,
    /// <summary>Delete worlds.</summary>
    Worlds_Delete,
    /// <summary>Modify world settings.</summary>
    Worlds_Settings,

    // Economy
    /// <summary>View economy data and statistics.</summary>
    Economy_View,
    /// <summary>Adjust player balances and credits.</summary>
    Economy_Adjust,
    /// <summary>View transaction audit logs.</summary>
    Economy_Audit,

    // Jobs
    /// <summary>View all jobs.</summary>
    Jobs_View,
    /// <summary>Create jobs manually.</summary>
    Jobs_Create,
    /// <summary>Edit job details.</summary>
    Jobs_Edit,
    /// <summary>Delete jobs.</summary>
    Jobs_Delete,
    /// <summary>Force complete jobs for players.</summary>
    Jobs_ForceComplete,

    // Aircraft & Marketplace
    /// <summary>View all aircraft data.</summary>
    Aircraft_View,
    /// <summary>Grant aircraft to players.</summary>
    Aircraft_Spawn,
    /// <summary>Manage marketplace settings.</summary>
    Marketplace_Manage,
    /// <summary>Moderate player auctions.</summary>
    Auctions_Moderate,

    // Licenses
    /// <summary>View all license data.</summary>
    Licenses_View,
    /// <summary>Grant licenses to players.</summary>
    Licenses_Grant,
    /// <summary>Revoke licenses from players.</summary>
    Licenses_Revoke,
    /// <summary>Override exam results.</summary>
    Exams_Override,

    // Moderation
    /// <summary>Moderate chat messages.</summary>
    Chat_Moderate,
    /// <summary>View player reports.</summary>
    Reports_View,
    /// <summary>Resolve player reports.</summary>
    Reports_Resolve,
    /// <summary>Issue violations to players.</summary>
    Violations_Issue,
    /// <summary>Clear player violations.</summary>
    Violations_Clear,

    // System
    /// <summary>View system settings.</summary>
    Settings_View,
    /// <summary>Edit system settings.</summary>
    Settings_Edit,
    /// <summary>View system logs.</summary>
    Logs_View,
    /// <summary>View analytics dashboards.</summary>
    Analytics_View,
    /// <summary>Manage IAM (roles, permissions). SuperAdmin only.</summary>
    IAM_Manage
}
