using System.Globalization;
using NTiInbox.SystemApis;

namespace NTiInbox.Messages;

/// <summary>
/// Filter criteria of the inbox. Filtering runs on the messages already read,
/// without a new call to the system.
/// </summary>
public sealed class MessageFilter
{
    /// <summary>Free search on the id, the text and the sender.</summary>
    public string Search { get; set; } = string.Empty;

    /// <summary>Selected reply status.</summary>
    public ReplyStatusFilter ReplyStatus { get; set; } = ReplyStatusFilter.All;

    /// <summary>Selected minimum severity, 0 to 99.</summary>
    public int MinimumSeverity { get; set; }

    /// <summary>Selected message type code, empty for all types.</summary>
    public string MessageType { get; set; } = string.Empty;

    /// <summary>Selected send period.</summary>
    public PeriodFilter Period { get; set; } = PeriodFilter.All;

    /// <summary>
    /// Selected one-hour slot, chosen on the activity chart.
    /// Null when all hours are shown.
    /// </summary>
    public DateTime? HourBucket { get; set; }

    /// <summary>True when at least one criterion is active.</summary>
    public bool IsActive =>
        !string.IsNullOrWhiteSpace(Search)
        || ReplyStatus != ReplyStatusFilter.All
        || MinimumSeverity > 0
        || !string.IsNullOrWhiteSpace(MessageType)
        || Period != PeriodFilter.All
        || HourBucket is not null;

    /// <summary>Number of active criteria, shown beside the reset button.</summary>
    public int ActiveCount =>
        (string.IsNullOrWhiteSpace(Search) ? 0 : 1)
        + (ReplyStatus == ReplyStatusFilter.All ? 0 : 1)
        + (MinimumSeverity > 0 ? 1 : 0)
        + (string.IsNullOrWhiteSpace(MessageType) ? 0 : 1)
        + (Period == PeriodFilter.All ? 0 : 1)
        + (HourBucket is null ? 0 : 1);

    /// <summary>Resets every criterion to its default value.</summary>
    public void Reset()
    {
        Search = string.Empty;
        ReplyStatus = ReplyStatusFilter.All;
        MinimumSeverity = 0;
        MessageType = string.Empty;
        Period = PeriodFilter.All;
        HourBucket = null;
    }

    /// <summary>True when the message meets every active criterion.</summary>
    public bool Matches(QueueMessage message)
    {
        if (message.MessageSeverity < MinimumSeverity)
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(MessageType)
            && !string.Equals((message.MessageType ?? string.Empty).Trim(), MessageType, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (!MatchesReplyStatus(message))
        {
            return false;
        }

        if (!MatchesPeriod(message))
        {
            return false;
        }

        if (!MatchesHour(message))
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(Search))
        {
            return true;
        }

        var needle = Search.Trim();

        return (message.MessageId ?? string.Empty).Contains(needle, StringComparison.OrdinalIgnoreCase)
            || (message.MessageWithReplacementData ?? string.Empty).Contains(needle, StringComparison.OrdinalIgnoreCase)
            || (message.MessageText ?? string.Empty).Contains(needle, StringComparison.OrdinalIgnoreCase)
            || (message.ReplacementData ?? string.Empty).Contains(needle, StringComparison.OrdinalIgnoreCase)
            || (message.SenderJob ?? string.Empty).Contains(needle, StringComparison.OrdinalIgnoreCase)
            || (message.SenderUserProfile ?? string.Empty).Contains(needle, StringComparison.OrdinalIgnoreCase);
    }

    private bool MatchesReplyStatus(QueueMessage message)
    {
        if (ReplyStatus == ReplyStatusFilter.All)
        {
            return true;
        }

        var status = (message.ReplyStatus ?? string.Empty).Trim().ToUpperInvariant();

        return ReplyStatus switch
        {
            ReplyStatusFilter.Waiting => status == "W",
            ReplyStatusFilter.Answered => status == "A",
            ReplyStatusFilter.None => status != "W" && status != "A",
            _ => true
        };
    }

    /// <summary>
    /// Keeps the messages of a one-hour slot. The computation mirrors the activity
    /// chart, so clicking a bar shows exactly the messages it counts.
    /// </summary>
    private bool MatchesHour(QueueMessage message)
    {
        if (HourBucket is null)
        {
            return true;
        }

        if (string.IsNullOrWhiteSpace(message.DateSent)
            || !DateTime.TryParse(message.DateSent, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
        {
            return false;
        }

        var sent = string.IsNullOrWhiteSpace(message.TimeSent)
            || !TimeSpan.TryParse(message.TimeSent, CultureInfo.InvariantCulture, out var time)
                ? date
                : date.Add(time);

        var hour = new DateTime(sent.Year, sent.Month, sent.Day, sent.Hour, 0, 0);

        return hour == HourBucket.Value;
    }

    private bool MatchesPeriod(QueueMessage message)
    {
        if (Period == PeriodFilter.All)
        {
            return true;
        }

        if (string.IsNullOrWhiteSpace(message.DateSent)
            || !DateTime.TryParse(message.DateSent, CultureInfo.InvariantCulture, DateTimeStyles.None, out var sent))
        {
            return false;
        }

        var limit = Period switch
        {
            PeriodFilter.Today => DateTime.Today,
            PeriodFilter.Week => DateTime.Today.AddDays(-7),
            PeriodFilter.Month => DateTime.Today.AddDays(-30),
            _ => DateTime.MinValue
        };

        return sent.Date >= limit;
    }
}
