using NTiInbox.SystemApis;

namespace NTiInbox.UserInterface;

/// <summary>
/// Relay between the header, the queue list and the dialogs hosted by the layout, so
/// their buttons work from any page.
/// </summary>
public sealed class DialogService
{
    /// <summary>Request to open the compose dialog.</summary>
    public event Action? ComposeRequested;

    /// <summary>Request to open the break message dialog.</summary>
    public event Action? BreakRequested;

    /// <summary>Request to open the attributes of the current queue.</summary>
    public event Action? AttributesRequested;

    /// <summary>Request to open a job panel.</summary>
    public event Action<JobRef>? JobRequested;

    /// <summary>Request to reread the current queue.</summary>
    public event Action? RefreshRequested;

    public void OpenCompose() => ComposeRequested?.Invoke();

    public void OpenBreak() => BreakRequested?.Invoke();

    public void OpenAttributes() => AttributesRequested?.Invoke();

    public void OpenJob(JobRef job) => JobRequested?.Invoke(job);

    public void RequestRefresh() => RefreshRequested?.Invoke();
}
