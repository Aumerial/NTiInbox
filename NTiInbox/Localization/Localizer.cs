using System.Globalization;

namespace NTiInbox.Localization;

/// <summary>
/// Translation of the interface texts for the current circuit. English is the fallback
/// language, then the key itself, so a missing translation never breaks a page.
/// </summary>
public sealed class Localizer
{
    /// <summary>Language used when nothing else is known.</summary>
    public const string DefaultLanguage = "en";

    private static readonly Dictionary<string, CultureInfo> Cultures = new()
    {
        ["en"] = CultureInfo.GetCultureInfo("en-GB"),
        ["fr"] = CultureInfo.GetCultureInfo("fr-FR")
    };

    /// <summary>Languages offered in the interface.</summary>
    public static IReadOnlyList<string> SupportedLanguages { get; } = [.. Cultures.Keys];

    /// <summary>Current language code, "en" or "fr".</summary>
    public string Language { get; private set; } = DefaultLanguage;

    /// <summary>Culture used for dates and numbers.</summary>
    public CultureInfo Culture => Cultures[Language];

    /// <summary>Raised after a language change.</summary>
    public event Action? Changed;

    /// <summary>Changes the language. Unknown codes are ignored.</summary>
    public void SetLanguage(string? code)
    {
        var language = Normalize(code);

        if (language == Language)
        {
            return;
        }

        Language = language;
        Changed?.Invoke();
    }

    /// <summary>Translated text, formatted with the culture of the language.</summary>
    public string Get(string key, params object[] args)
    {
        var text = Lookup(key) ?? key;
        return args.Length == 0 ? text : string.Format(Culture, text, args);
    }

    /// <summary>
    /// Text chosen by quantity from the keys <c>key.one</c> and <c>key.other</c>.
    /// French treats 0 as singular, English does not.
    /// </summary>
    public string Plural(string key, int count, params object[] args)
    {
        var one = count == 1 || (Language == "fr" && count == 0);
        return Get($"{key}.{(one ? "one" : "other")}", [count, .. args]);
    }

    /// <summary>Translated text, or null when the key is unknown in both languages.</summary>
    public string? TryGet(string key, params object[] args)
    {
        var text = Lookup(key);
        return text is null ? null : args.Length == 0 ? text : string.Format(Culture, text, args);
    }

    /// <summary>Date and time in the culture of the current language.</summary>
    public string FormatDate(DateTime? value)
        => value is null ? string.Empty : value.Value.ToString("g", Culture);

    private string? Lookup(string key)
    {
        if (Texts(Language).TryGetValue(key, out var text))
        {
            return text;
        }

        return Texts(DefaultLanguage).TryGetValue(key, out text) ? text : null;
    }

    private static IReadOnlyDictionary<string, string> Texts(string language)
        => language == "fr" ? Strings.Fr : Strings.En;

    private static string Normalize(string? code)
    {
        var two = (code ?? string.Empty).Trim().ToLowerInvariant();
        two = two.Length > 2 ? two[..2] : two;
        return Cultures.ContainsKey(two) ? two : DefaultLanguage;
    }
}
