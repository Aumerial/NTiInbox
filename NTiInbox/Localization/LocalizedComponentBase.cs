using Microsoft.AspNetCore.Components;

namespace NTiInbox.Localization;

/// <summary>
/// Base of every component with visible text: gives access to translations and
/// re-renders when the language changes.
/// </summary>
public abstract class LocalizedComponentBase : ComponentBase, IDisposable
{
    private Localizer _localizer = default!;

    /// <summary>Translator of the circuit. Subscribes on injection to follow language changes.</summary>
    [Inject]
    protected Localizer Localizer
    {
        get => _localizer;
        set
        {
            _localizer = value;
            _localizer.Changed += OnLanguageChanged;
        }
    }

    /// <summary>Translated text for a key.</summary>
    protected string L(string key, params object[] args) => _localizer.Get(key, args);

    /// <summary>Translated text chosen by quantity.</summary>
    protected string Plural(string key, int count, params object[] args) => _localizer.Plural(key, count, args);

    /// <summary>Called after a language change. Components that cache display text rebuild it here.</summary>
    protected virtual void OnLanguageChanged() => _ = InvokeAsync(StateHasChanged);

    /// <inheritdoc />
    public virtual void Dispose() => _localizer.Changed -= OnLanguageChanged;
}
