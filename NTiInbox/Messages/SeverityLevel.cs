namespace NTiInbox.Messages;

/// <summary>
/// Groups IBM i severities into four visual tiers.
/// The tier drives the badge color in the list.
/// </summary>
public enum SeverityLevel
{
    /// <summary>Severity 0 to 9: information or harmless completion.</summary>
    Low,

    /// <summary>Severity 10 to 29: light warning.</summary>
    Info,

    /// <summary>Severity 30 to 39: error, processing could continue.</summary>
    Warning,

    /// <summary>Severity 40 and above: severe error, stop or intervention needed.</summary>
    Critical
}
