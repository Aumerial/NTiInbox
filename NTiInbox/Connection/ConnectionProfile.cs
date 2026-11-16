namespace NTiInbox.Connection;

/// <summary>
/// Saved connection. Never contains the password.
/// </summary>
public sealed class ConnectionProfile
{
    /// <summary>Stable identifier, kept when the same connection is saved again.</summary>
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    public string Host { get; set; } = string.Empty;

    public string User { get; set; } = string.Empty;

    /// <summary>Library (or special value) searched for message queues.</summary>
    public string Library { get; set; } = string.Empty;

    public bool UseSsl { get; set; }

    /// <summary>Marks the environment as production (red marking across the app).</summary>
    public bool IsProduction { get; set; }

    /// <summary>Last sign-on with this connection, in UTC. The most recent one comes first.</summary>
    public DateTime LastUsed { get; set; }

    /// <summary>Same host and user, ignoring case.</summary>
    public bool Matches(string host, string user)
        => string.Equals(Host.Trim(), host.Trim(), StringComparison.OrdinalIgnoreCase)
           && string.Equals(User.Trim(), user.Trim(), StringComparison.OrdinalIgnoreCase);
}
