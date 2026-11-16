using Microsoft.JSInterop;

namespace NTiInbox.UserInterface;

/// <summary>
/// Display theme, kept by the browser. The initial value is applied by an inline script
/// in the page head to avoid a flash.
/// </summary>
public sealed class ThemeService(IJSRuntime js) : IAsyncDisposable
{
    private IJSObjectReference? _module;

    /// <summary>Current choice.</summary>
    public AppTheme Theme { get; private set; } = AppTheme.Dark;

    /// <summary>Raised after a theme change.</summary>
    public event Action? Changed;

    /// <summary>Reads the choice stored by the browser.</summary>
    public async Task InitializeAsync()
    {
        Theme = Parse(await InvokeAsync<string>("get"));
        Changed?.Invoke();
    }

    /// <summary>Switches between dark and light.</summary>
    public Task ToggleAsync() => SetAsync(Theme == AppTheme.Light ? AppTheme.Dark : AppTheme.Light);

    /// <summary>Applies and stores a choice.</summary>
    public async Task SetAsync(AppTheme theme)
    {
        Theme = theme;
        await InvokeAsync<string>("set", Name(theme));
        Changed?.Invoke();
    }

    private static string Name(AppTheme theme) => theme switch
    {
        AppTheme.Light => "light",
        AppTheme.System => "system",
        _ => "dark"
    };

    private static AppTheme Parse(string? value) => (value ?? string.Empty).ToLowerInvariant() switch
    {
        "light" => AppTheme.Light,
        "system" => AppTheme.System,
        _ => AppTheme.Dark
    };

    private async Task<T> InvokeAsync<T>(string identifier, params object?[] args)
    {
        try
        {
            _module ??= await js.InvokeAsync<IJSObjectReference>("import", "./js/theme.js");
            return await _module.InvokeAsync<T>(identifier, args);
        }
        catch (Exception ex) when (ex is JSDisconnectedException or TaskCanceledException)
        {
            return default!;
        }
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_module is null)
        {
            return;
        }

        try
        {
            await _module.DisposeAsync();
        }
        catch (JSDisconnectedException)
        {
            // Circuit already closed.
        }
    }
}
