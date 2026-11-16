namespace NTiInbox.Messages;

/// <summary>
/// Number of messages received during one hour, split by severity tier.
/// </summary>
public sealed class ActivityBucket
{
    /// <summary>Start of the slot, on the hour.</summary>
    public DateTime Hour { get; init; }

    /// <summary>Messages of severity 0 to 9.</summary>
    public int Low { get; set; }

    /// <summary>Messages of severity 10 to 29.</summary>
    public int Info { get; set; }

    /// <summary>Messages of severity 30 to 39.</summary>
    public int Warning { get; set; }

    /// <summary>Messages of severity 40 and above.</summary>
    public int Critical { get; set; }

    /// <summary>Total of the slot.</summary>
    public int Total => Low + Info + Warning + Critical;
}
