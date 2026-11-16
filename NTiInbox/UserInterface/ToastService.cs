namespace NTiInbox.UserInterface;

/// <summary>
/// Notifications of the current circuit. Errors and the results of changes
/// stay until dismissed, other confirmations disappear on their own.
/// </summary>
public sealed class ToastService(Localization.ErrorText errors)
{
    private readonly object _lock = new();
    private readonly List<Toast> _toasts = new();

    /// <summary>Notifications currently visible.</summary>
    public IReadOnlyList<Toast> Toasts
    {
        get
        {
            lock (_lock)
            {
                return _toasts.ToList();
            }
        }
    }

    /// <summary>Raised when the list changes.</summary>
    public event Action? Changed;

    /// <summary>
    /// Confirmation of an action. A change made on the system passes <paramref name="keep"/>
    /// so its result stays visible until the user closes it.
    /// </summary>
    public void Success(string title, string? detail = null, bool keep = false)
        => Push(new Toast { Kind = ToastKind.Success, Title = title, Detail = detail }, keep ? null : TimeSpan.FromSeconds(6));

    public void Info(string title, string? detail = null)
        => Push(new Toast { Kind = ToastKind.Info, Title = title, Detail = detail }, TimeSpan.FromSeconds(5));

    public void Error(string title, string? detail = null)
        => Push(new Toast { Kind = ToastKind.Error, Title = title, Detail = detail }, null);

    /// <summary>
    /// Reports an exception, translated when the IBM i message id is known and
    /// stripped of any credential.
    /// </summary>
    public void Error(string title, Exception exception)
        => Error(title, errors.Describe(exception));

    public void Dismiss(Guid id)
    {
        lock (_lock)
        {
            _toasts.RemoveAll(t => t.Id == id);
        }

        Changed?.Invoke();
    }

    private void Push(Toast toast, TimeSpan? autoDismiss)
    {
        lock (_lock)
        {
            _toasts.Add(toast);
        }

        Changed?.Invoke();

        if (autoDismiss == null)
        {
            return;
        }

        // Automatic dismissal runs on another thread, hence the lock.
        _ = Task.Delay(autoDismiss.Value).ContinueWith(_ => Dismiss(toast.Id));
    }
}
