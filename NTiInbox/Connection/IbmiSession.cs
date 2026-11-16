using System.Data.Common;
using System.Runtime.ExceptionServices;
using System.Text.RegularExpressions;
using Aumerial.Data.Nti;
using Microsoft.Extensions.Options;
using NTiInbox.Localization;

namespace NTiInbox.Connection;

/// <summary>
/// IBM i session of the current Blazor circuit. Every system call goes through
/// <see cref="RunAsync{T}"/>, which runs it off the render thread and serialises it,
/// because an NTi connection does not support two simultaneous calls.
/// </summary>
public sealed partial class IbmiSession(
    IOptions<IbmiOptions> options,
    ILogger<IbmiSession> logger) : IDisposable
{
    private readonly SemaphoreSlim _gate = new(1, 1);
    private NTiConnection? _connection;

    // Kept only to scrub error messages: a driver may quote the connection string.
    private string _password = string.Empty;

    /// <summary>Host name or address of the connected system.</summary>
    public string Host { get; private set; } = string.Empty;

    /// <summary>Connected user profile.</summary>
    public string User { get; private set; } = string.Empty;

    /// <summary>Whether the connection uses TLS.</summary>
    public bool UseSsl { get; private set; }

    /// <summary>Whether the environment is marked as production.</summary>
    public bool IsProduction { get; private set; }

    /// <summary>Elevated special authorities of the profile (*ALLOBJ, *SECADM).</summary>
    public IReadOnlyList<string> ElevatedAuthorities { get; private set; } = [];

    /// <summary>Whether a connection is open.</summary>
    public bool IsConnected => _connection is not null;

    /// <summary>Last sign-on failure, kept for the sign-on screen. Cleared by the next attempt.</summary>
    public ConnectionException? LastFailure { get; private set; }

    /// <summary>Defaults from configuration, used to pre-fill the sign-on form.</summary>
    public IbmiOptions Defaults => options.Value;

    /// <summary>Two first letters of the profile, for the avatar.</summary>
    public string Initials => User.Length == 0 ? "?" : User[..Math.Min(2, User.Length)].ToUpperInvariant();

    /// <summary>Raised when the connection state or the session flags change.</summary>
    public event Action? Changed;

    /// <summary>Opens the connection. A previous connection is closed first.</summary>
    /// <exception cref="ConnectionException">The sign-on failed; the message holds no credential.</exception>
    public async Task ConnectAsync(ConnectionRequest request)
    {
        Disconnect();
        LastFailure = null;
        _password = request.Password;

        // The failure crosses Task.Run as a value: an exception left to escape the task
        // would stop the debugger even though it is handled after the await.
        var (opened, error) = await Task.Run(() =>
        {
            NTiConnection? attempt = null;

            try
            {
                attempt = new NTiConnection(BuildConnectionString(request));
                attempt.Open();
                return ((NTiConnection?)attempt, (Exception?)null);
            }
            catch (Exception ex)
            {
                attempt?.Dispose();
                return ((NTiConnection?)null, (Exception?)ex);
            }
        });

        if (opened is null)
        {
            var message = Sanitize(error?.Message);
            _password = string.Empty;

            // Only the cleaned message is logged: the raw exception may quote the connection string.
            logger.LogWarning("Sign-on failed: {Message}", message);

            var failure = new ConnectionException(message, message.Contains("licen", StringComparison.OrdinalIgnoreCase));
            LastFailure = failure;
            throw failure;
        }

        _connection = opened;
        Host = request.Host.Trim();
        User = request.User.Trim().ToUpperInvariant();
        UseSsl = request.UseSsl;
        IsProduction = request.IsProduction;
        Changed?.Invoke();
    }

    /// <summary>Runs a call that returns a value on the current connection.</summary>
    public async Task<T> RunAsync<T>(Func<NTiConnection, T> action)
    {
        var connection = _connection ?? throw new UserMessageException("error.notConnected");

        await _gate.WaitAsync();

        try
        {
            // Same reason as for the sign-on: the failure is returned as a value, then rethrown.
            var (result, error) = await Task.Run(() =>
            {
                try
                {
                    return (action(connection), (Exception?)null);
                }
                catch (Exception ex)
                {
                    return (default(T), ex);
                }
            });

            if (error is not null)
            {
                ExceptionDispatchInfo.Capture(error).Throw();
            }

            return result!;
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <summary>Runs a call without a return value on the current connection.</summary>
    public Task RunAsync(Action<NTiConnection> action)
        => RunAsync(connection =>
        {
            action(connection);
            return true;
        });

    /// <summary>Records the special authorities read after sign-on.</summary>
    public void SetElevatedAuthorities(IEnumerable<string> authorities)
    {
        ElevatedAuthorities = [.. authorities];
        Changed?.Invoke();
    }

    /// <summary>
    /// Removes the current password and any <c>password=</c> or <c>pwd=</c> pair from a text
    /// before it is shown or logged.
    /// </summary>
    public string Sanitize(string? text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return string.Empty;
        }

        var clean = CredentialPair().Replace(text, "$1=***");
        return _password.Length > 0 ? clean.Replace(_password, "***", StringComparison.Ordinal) : clean;
    }

    /// <summary>Closes the connection, if any.</summary>
    public void Disconnect()
    {
        var connection = _connection;
        var wasConnected = connection is not null;

        _connection = null;
        _password = string.Empty;
        Host = User = string.Empty;
        UseSsl = IsProduction = false;
        ElevatedAuthorities = [];

        if (connection is not null)
        {
            try
            {
                connection.Close();
            }
            catch (Exception ex)
            {
                // A failing close must never break the interface.
                logger.LogDebug("Closing the connection failed: {Message}", Sanitize(ex.Message));
            }
            finally
            {
                connection.Dispose();
            }
        }

        if (wasConnected)
        {
            Changed?.Invoke();
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Disconnect();
        _gate.Dispose();
    }

    // The builder quotes values that contain separators, so a password with ';' is safe.
    private string BuildConnectionString(ConnectionRequest request)
    {
        var builder = new DbConnectionStringBuilder
        {
            ["server"] = request.Host.Trim(),
            ["user"] = request.User.Trim(),
            ["password"] = request.Password,
            ["compress"] = "false"
        };

        if (request.UseSsl)
        {
            builder["use ssl"] = "true";
        }

        var extra = options.Value.ConnectionOptions.Trim();
        return extra.Length == 0 ? builder.ConnectionString : $"{builder.ConnectionString};{extra}";
    }

    [GeneratedRegex(@"\b(password|pwd)\s*=\s*(""[^""]*""|'[^']*'|[^;\s]*)", RegexOptions.IgnoreCase)]
    private static partial Regex CredentialPair();
}
