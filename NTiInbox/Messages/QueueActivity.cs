using NTiInbox.SystemApis;

namespace NTiInbox.Messages;

/// <summary>
/// Hourly distribution of the messages of a queue, computed on the messages already
/// read without asking the system again.
/// </summary>
public static class QueueActivity
{
    /// <summary>Number of slots shown.</summary>
    public const int HourCount = 24;

    /// <summary>
    /// Builds the hourly slots. The window ends at the hour of the newest message and
    /// covers the 24 hours before it: on a quiet queue, a window anchored on the current
    /// hour would be empty.
    /// </summary>
    public static List<ActivityBucket> Build(IEnumerable<QueueMessage> messages)
    {
        var dated = messages
            .Select(m => (Message: m, SentAt: MessageFormatter.SentAt(m)))
            .Where(x => x.SentAt is not null)
            .Select(x => (x.Message, SentAt: x.SentAt!.Value))
            .ToList();

        if (dated.Count == 0)
        {
            return [];
        }

        var last = dated.Max(x => x.SentAt);
        var end = new DateTime(last.Year, last.Month, last.Day, last.Hour, 0, 0);
        var start = end.AddHours(-(HourCount - 1));

        var buckets = Enumerable.Range(0, HourCount)
            .Select(i => new ActivityBucket { Hour = start.AddHours(i) })
            .ToList();

        foreach (var (message, sentAt) in dated)
        {
            var index = (int)Math.Floor((sentAt - start).TotalHours);

            if (index < 0 || index >= HourCount)
            {
                continue;
            }

            var bucket = buckets[index];

            switch (MessageFormatter.Level(message.MessageSeverity))
            {
                case SeverityLevel.Low:
                    bucket.Low++;
                    break;
                case SeverityLevel.Info:
                    bucket.Info++;
                    break;
                case SeverityLevel.Warning:
                    bucket.Warning++;
                    break;
                default:
                    bucket.Critical++;
                    break;
            }
        }

        return buckets;
    }
}
