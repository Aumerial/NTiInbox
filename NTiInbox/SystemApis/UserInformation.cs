namespace NTiInbox.SystemApis;

/// <summary>
/// User profile information, as returned by QSYRUSRI (USRI0300 format).
/// Only the fields this application needs are read.
/// </summary>
public class UserInformation
{
    /// <summary>Profile name.</summary>
    public string? UserProfileName { get; set; }

    /// <summary>Last sign-on, or null if the profile never signed on.</summary>
    public DateTime? PreviousSignOn { get; set; }

    /// <summary>Invalid sign-on attempts since the last successful one.</summary>
    public int InvalidSignOnAttempts { get; set; }

    /// <summary>Profile status: *ENABLED or *DISABLED.</summary>
    public string? Status { get; set; }

    /// <summary>User class: *USER, *SYSOPR, *PGMR, *SECADM, *SECOFR.</summary>
    public string? UserClass { get; set; }

    /// <summary>Special authorities held by the profile.</summary>
    public List<string> SpecialAuthorities { get; set; } = new();

    /// <summary>Group profile, or *NONE.</summary>
    public string? GroupProfile { get; set; }

    /// <summary>Current library, or *CRTDFT.</summary>
    public string? CurrentLibrary { get; set; }

    /// <summary>Limit capabilities: *NO, *PARTIAL or *YES.</summary>
    public string? LimitCapabilities { get; set; }

    /// <summary>Descriptive text of the profile.</summary>
    public string? TextDescription { get; set; }

    /// <summary>Disk space used by the objects of the profile, in kilobytes.</summary>
    public int StorageUsed { get; set; }

    /// <summary>Job description tied to the profile.</summary>
    public string? JobDescriptionName { get; set; }

    /// <summary>Library of the job description.</summary>
    public string? JobDescriptionLibrary { get; set; }

    /// <summary>Message queue of the profile.</summary>
    public string? MessageQueueName { get; set; }

    /// <summary>Library of the profile message queue.</summary>
    public string? MessageQueueLibrary { get; set; }

    /// <summary>Queue delivery method: *NOTIFY, *BREAK, *HOLD or *DFT.</summary>
    public string? MessageQueueDeliveryMethod { get; set; }

    /// <summary>Minimum severity from which messages are delivered.</summary>
    public int MessageQueueSeverity { get; set; }

    /// <summary>Output queue of the profile.</summary>
    public string? OutputQueueName { get; set; }

    /// <summary>Library of the output queue.</summary>
    public string? OutputQueueLibrary { get; set; }

    /// <summary>True when the profile is enabled.</summary>
    public bool IsEnabled => string.Equals(Status?.Trim(), "*ENABLED", StringComparison.OrdinalIgnoreCase);
}
