using NTiInbox.SystemApis;

namespace NTiInbox.Messages;

/// <summary>
/// Group of identical messages, or a lone message when there is a single occurrence.
/// A system queue often repeats the same message every hour: grouping them turns a
/// hundred-line list into a few dozen useful lines.
/// </summary>
public sealed class MessageGroup
{
    public MessageGroup(string key, List<QueueMessage> messages, int threshold)
    {
        Key = key;
        Messages = messages;
        Threshold = threshold;
    }

    /// <summary>Grouping key: message id and displayed text.</summary>
    public string Key { get; }

    /// <summary>Occurrences, newest first.</summary>
    public List<QueueMessage> Messages { get; }

    /// <summary>Occurrence count from which the row is collapsed.</summary>
    public int Threshold { get; }

    /// <summary>Occurrence count.</summary>
    public int Count => Messages.Count;

    /// <summary>Newest occurrence, the one that carries the row.</summary>
    public QueueMessage Newest => Messages[0];

    /// <summary>Oldest occurrence of the group.</summary>
    public QueueMessage Oldest => Messages[^1];

    /// <summary>True when the group deserves to be collapsed into one row.</summary>
    public bool IsCollapsible => Count >= Threshold;

    /// <summary>Highest severity of the group.</summary>
    public int MaxSeverity => Messages.Max(m => m.MessageSeverity);

    /// <summary>Stamp of the oldest occurrence.</summary>
    public string From => MessageFormatter.ShortStamp(Oldest);

    /// <summary>Stamp of the newest occurrence.</summary>
    public string To => MessageFormatter.ShortStamp(Newest);
}
