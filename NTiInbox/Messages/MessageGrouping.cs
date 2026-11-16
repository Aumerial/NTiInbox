using NTiInbox.SystemApis;

namespace NTiInbox.Messages;

/// <summary>
/// Builds the groups of repeated messages.
/// </summary>
public static class MessageGrouping
{
    /// <summary>Occurrence count from which a group is collapsed.</summary>
    public const int DefaultThreshold = 3;

    /// <summary>
    /// Groups messages with the same id and text. The group takes the place of its
    /// newest occurrence, so the chronological order of the list stays readable.
    /// </summary>
    /// <param name="messages">Messages to group, newest first.</param>
    /// <param name="threshold">Occurrences needed to collapse a group.</param>
    public static List<MessageGroup> Build(IEnumerable<QueueMessage> messages, int threshold = DefaultThreshold)
    {
        var order = new List<string>();
        var buckets = new Dictionary<string, List<QueueMessage>>(StringComparer.Ordinal);

        foreach (var message in messages)
        {
            var key = KeyOf(message);

            if (!buckets.TryGetValue(key, out var bucket))
            {
                bucket = new List<QueueMessage>();
                buckets[key] = bucket;
                order.Add(key);
            }

            bucket.Add(message);
        }

        return order.Select(key => new MessageGroup(key, buckets[key], threshold)).ToList();
    }

    /// <summary>
    /// Grouping key. The text is part of the key: two messages with the same id but
    /// about different objects stay apart.
    /// </summary>
    private static string KeyOf(QueueMessage message)
    {
        var id = (message.MessageId ?? string.Empty).Trim();
        var text = MessageFormatter.DisplayText(message);

        return $"{id}{text}";
    }
}
