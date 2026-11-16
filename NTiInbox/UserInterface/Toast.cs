namespace NTiInbox.UserInterface;

/// <summary>Notification shown to the user.</summary>
public sealed class Toast
{
    public Guid Id { get; } = Guid.NewGuid();

    public ToastKind Kind { get; init; } = ToastKind.Info;

    public string Title { get; init; } = string.Empty;

    public string? Detail { get; init; }

    /// <summary>CSS class matching the kind of notification.</summary>
    public string CssClass => Kind switch
    {
        ToastKind.Success => "nti-success",
        ToastKind.Error => "nti-error",
        _ => "nti-info"
    };
}
