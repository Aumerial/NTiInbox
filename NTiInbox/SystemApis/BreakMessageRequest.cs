namespace NTiInbox.SystemApis;

/// <summary>
/// User input for sending a break message through QMHSNDBM.
/// This kind of message really interrupts the recipient's session.
/// </summary>
public class BreakMessageRequest
{
    /// <summary>Text shown on the target workstation.</summary>
    public string MessageText { get; set; } = string.Empty;

    /// <summary>Message queue of the target workstation, usually the device name.</summary>
    public string TargetQueue { get; set; } = string.Empty;

    /// <summary>Library of the target queue.</summary>
    public string TargetLibrary { get; set; } = "*LIBL";

    /// <summary>Message type: *INFO or *INQ.</summary>
    public string MessageType { get; set; } = "*INFO";

    /// <summary>Reply queue, required for an *INQ message.</summary>
    public string ReplyQueue { get; set; } = string.Empty;

    /// <summary>Library of the reply queue.</summary>
    public string ReplyLibrary { get; set; } = "*LIBL";

    /// <summary>True when the message waits for a reply.</summary>
    public bool IsInquiry => string.Equals(MessageType, "*INQ", StringComparison.OrdinalIgnoreCase);
}
