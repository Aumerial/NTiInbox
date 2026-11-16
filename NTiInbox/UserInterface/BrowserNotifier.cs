using Microsoft.JSInterop;

namespace NTiInbox.UserInterface;

/// <summary>
/// Browser notifications, used only for messages waiting for a reply and only
/// while the tab is in the background.
/// </summary>
public sealed class BrowserNotifier(IJSRuntime js) : IAsyncDisposable
{
    private IJSObjectReference? _module;

    /// <summary>Last known permission.</summary>
    public NotificationPermission Permission { get; private set; } = NotificationPermission.Default;

    /// <summary>Reads the permission without asking anything to the user.</summary>
    public async Task<NotificationPermission> ReadPermissionAsync()
    {
        Permission = Parse(await InvokeAsync<string>("permission"));
        return Permission;
    }

    /// <summary>
    /// Asks for the permission. Browsers require a user gesture, so this must start from a click.
    /// </summary>
    public async Task<NotificationPermission> RequestPermissionAsync()
    {
        Permission = Parse(await InvokeAsync<string>("requestPermission"));
        return Permission;
    }

    /// <summary>
    /// Shows a notification. It is ignored when the tab is visible or the permission is missing.
    /// </summary>
    public async Task NotifyAsync(string title, string body, string tag)
        => await InvokeAsync<bool>("notify", title, body, tag);

    private async Task<T> InvokeAsync<T>(string identifier, params object?[] args)
    {
        try
        {
            _module ??= await js.InvokeAsync<IJSObjectReference>("import", "./js/live.js");
            return await _module.InvokeAsync<T>(identifier, args);
        }
        catch (Exception ex) when (ex is JSDisconnectedException or TaskCanceledException)
        {
            // The circuit is closed, there is no browser left to notify.
            return default!;
        }
    }

    private static NotificationPermission Parse(string? value) => (value ?? string.Empty).ToLowerInvariant() switch
    {
        "granted" => NotificationPermission.Granted,
        "denied" => NotificationPermission.Denied,
        "unsupported" => NotificationPermission.Unsupported,
        _ => NotificationPermission.Default
    };

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
