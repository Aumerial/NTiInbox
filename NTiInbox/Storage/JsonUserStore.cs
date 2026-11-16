using System.Text.Json;

namespace NTiInbox.Storage;

/// <summary>
/// JSON storage in <c>App_Data</c>, one file per user key. A corrupted or unreadable file is
/// treated as empty and a failing write is logged, neither ever interrupts a screen.
/// The caller picks the file shape, and must never store a password in it.
/// </summary>
public sealed class JsonUserStore(IWebHostEnvironment environment, ILogger<JsonUserStore> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    private readonly Lock _lock = new();
    private readonly string _folder = Path.Combine(environment.ContentRootPath, "App_Data");

    /// <summary>Reads the data of a user, or an empty object when missing or unreadable.</summary>
    public T Load<T>(string userKey) where T : class, new()
    {
        lock (_lock)
        {
            return Read<T>(PathFor(userKey));
        }
    }

    /// <summary>Applies a change under the lock and writes the file back.</summary>
    public void Update<T>(string userKey, Action<T> change) where T : class, new()
    {
        lock (_lock)
        {
            var path = PathFor(userKey);
            var data = Read<T>(path);
            change(data);

            try
            {
                Directory.CreateDirectory(_folder);
                File.WriteAllText(path, JsonSerializer.Serialize(data, JsonOptions));
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                // A read-only data folder only costs the saved data.
                logger.LogWarning("Cannot write {File}: {Message}", Path.GetFileName(path), ex.Message);
            }
        }
    }

    private T Read<T>(string path) where T : class, new()
    {
        try
        {
            return File.Exists(path)
                ? JsonSerializer.Deserialize<T>(File.ReadAllText(path)) ?? new()
                : new();
        }
        catch (Exception ex) when (ex is JsonException or IOException or UnauthorizedAccessException)
        {
            // Start again from an empty file rather than blocking the sign-on.
            logger.LogWarning("Cannot read {File}: {Message}", Path.GetFileName(path), ex.Message);
            return new();
        }
    }

    // The key becomes a file name: keep letters, digits, dash and underscore only.
    private string PathFor(string userKey)
    {
        var safe = new string([.. userKey.Where(c => char.IsAsciiLetterOrDigit(c) || c is '-' or '_')]);
        return Path.Combine(_folder, $"{(safe.Length == 0 ? "user" : safe)}.json");
    }
}
