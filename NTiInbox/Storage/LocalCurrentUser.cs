namespace NTiInbox.Storage;

/// <summary>Single local user, suitable for a workstation or a demo only.</summary>
public sealed class LocalCurrentUser : ICurrentUser
{
    /// <inheritdoc />
    public string Key => "local";
}
