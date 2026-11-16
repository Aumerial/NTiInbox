namespace NTiInbox.Localization;

/// <summary>
/// Business error whose text is a translation key, resolved at display time so the
/// business layer never holds language-dependent text.
/// </summary>
public sealed class UserMessageException(string key, params object[] args) : Exception(key)
{
    /// <summary>Translation key.</summary>
    public string Key { get; } = key;

    /// <summary>Format arguments of the translated text.</summary>
    public object[] Args { get; } = args;
}
