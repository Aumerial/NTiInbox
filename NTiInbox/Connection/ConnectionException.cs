namespace NTiInbox.Connection;

/// <summary>Sign-on failure whose text has already been stripped of any credential.</summary>
/// <param name="message">System message, without credential.</param>
/// <param name="licenseMissing">The NTi license is missing or invalid.</param>
public sealed class ConnectionException(string message, bool licenseMissing = false) : Exception(message)
{
    /// <summary>Whether the failure comes from an absent NTi license.</summary>
    public bool LicenseMissing { get; } = licenseMissing;
}
