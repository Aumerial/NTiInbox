namespace NTiInbox.UserInterface;

/// <summary>State of the browser notification permission.</summary>
public enum NotificationPermission
{
    /// <summary>The browser does not support notifications.</summary>
    Unsupported,

    /// <summary>The user has not answered yet.</summary>
    Default,

    /// <summary>Notifications are allowed.</summary>
    Granted,

    /// <summary>Notifications are denied.</summary>
    Denied
}
