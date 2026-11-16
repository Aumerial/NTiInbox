namespace NTiInbox.Localization;

/// <summary>
/// Interface texts, one dictionary per language (<c>Strings.En.cs</c>, <c>Strings.Fr.cs</c>).
/// Keys are hierarchical, for example <c>inbox.refresh</c>. Both files must hold the same keys.
/// </summary>
public static partial class Strings
{
    /// <summary>Text of a key in a language, falling back to English, then to the key itself.</summary>
    public static string Get(string language, string key)
    {
        var dictionary = language == "fr" ? Fr : En;
        return dictionary.TryGetValue(key, out var text) ? text : En.GetValueOrDefault(key, key);
    }

    /// <summary>Keys present in one dictionary and missing in the other. Empty when they match.</summary>
    public static IReadOnlyList<string> FindMismatches() =>
    [
        .. En.Keys.Except(Fr.Keys).Order().Select(key => $"missing in fr: {key}"),
        .. Fr.Keys.Except(En.Keys).Order().Select(key => $"missing in en: {key}")
    ];
}
