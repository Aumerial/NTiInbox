namespace NTiInbox.Storage;

/// <summary>
/// Identifies the person using the application, to keep local data separated.
/// The app has no authentication: replace the implementation before any shared deployment.
/// </summary>
public interface ICurrentUser
{
    /// <summary>Stable key of the user, used to name the storage file.</summary>
    string Key { get; }
}
