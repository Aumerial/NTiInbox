namespace NTiInbox.Connection;

/// <summary>
/// Optional defaults for the sign-on form, bound to the "Ibmi" configuration section.
/// All values are empty by default and the password is never read from configuration.
/// </summary>
public sealed class IbmiOptions
{
    /// <summary>Name of the configuration section.</summary>
    public const string Section = "Ibmi";

    /// <summary>Default host name or address.</summary>
    public string Host { get; set; } = string.Empty;

    /// <summary>Default user profile.</summary>
    public string User { get; set; } = string.Empty;

    /// <summary>Default library (or special value) searched for message queues.</summary>
    public string Library { get; set; } = string.Empty;

    /// <summary>Whether TLS is ticked by default.</summary>
    public bool UseSsl { get; set; }

    /// <summary>Extra connection string options, for example <c>pooling=true</c>.</summary>
    public string ConnectionOptions { get; set; } = string.Empty;
}
