using System.Globalization;
using NTiInbox.Localization;
using NTiInbox.SystemApis;

namespace NTiInbox.Messages;

/// <summary>
/// Converts IBM i codes into labels and CSS classes for the interface. Nothing here
/// calls the system: these are pure conversions, translated through the localizer.
/// </summary>
public static class MessageFormatter
{
    /// <summary>Visual tier of a severity.</summary>
    public static SeverityLevel Level(int severity) => severity switch
    {
        < 10 => SeverityLevel.Low,
        < 30 => SeverityLevel.Info,
        < 40 => SeverityLevel.Warning,
        _ => SeverityLevel.Critical
    };

    /// <summary>CSS class of the severity badge.</summary>
    public static string SeverityCssClass(int severity) => Level(severity) switch
    {
        SeverityLevel.Low => "nti-sev-low",
        SeverityLevel.Info => "nti-sev-info",
        SeverityLevel.Warning => "nti-sev-warning",
        _ => "nti-sev-critical"
    };

    /// <summary>Severity shown on two digits.</summary>
    public static string SeverityText(int severity) => severity.ToString("00", CultureInfo.InvariantCulture);

    /// <summary>Label of the severity tier.</summary>
    public static string SeverityLabel(Localizer l, int severity) => l.Get(Level(severity) switch
    {
        SeverityLevel.Low => "severity.low",
        SeverityLevel.Info => "severity.info",
        SeverityLevel.Warning => "severity.warning",
        _ => "severity.critical"
    });

    /// <summary>Label of the message type, from the two-character code of the LSTM0100 format.</summary>
    public static string TypeLabel(Localizer l, string? messageType)
    {
        var code = (messageType ?? string.Empty).Trim();
        return code.Length == 0
            ? l.Get("msgtype.none")
            : l.TryGet($"msgtype.{code}") ?? l.Get("msgtype.other", code);
    }

    /// <summary>CSS class of the message type chip.</summary>
    public static string TypeCssClass(string? messageType) => (messageType ?? string.Empty).Trim() switch
    {
        "05" => "nti-chip-purple",
        "02" or "15" or "17" => "nti-chip-warning",
        "01" => "nti-chip-success",
        "21" or "22" or "23" or "24" or "25" or "26" => "nti-chip-info",
        _ => "nti-chip-neutral"
    };

    /// <summary>True when the message waits for a reply from the user.</summary>
    public static bool IsWaitingForReply(QueueMessage message)
        => string.Equals(message.ReplyStatus?.Trim(), "W", StringComparison.OrdinalIgnoreCase);

    /// <summary>Label of the reply status, or null when no reply is expected.</summary>
    public static string? ReplyStatusLabel(Localizer l, QueueMessage message)
        => (message.ReplyStatus ?? string.Empty).Trim().ToUpperInvariant() switch
        {
            "W" => l.Get("reply.waiting"),
            "A" => l.Get("reply.answered"),
            _ => null
        };

    /// <summary>CSS class of the reply status chip.</summary>
    public static string ReplyStatusCssClass(QueueMessage message)
        => IsWaitingForReply(message) ? "nti-chip-warning" : "nti-chip-success";

    /// <summary>
    /// Text to show for a message: the version with substitution when present, then the raw
    /// text, then the data of an immediate message. Empty when there is none, so the result
    /// can be used in a grouping key; the interface shows a placeholder instead.
    /// </summary>
    public static string DisplayText(QueueMessage message)
    {
        if (!string.IsNullOrWhiteSpace(message.MessageWithReplacementData))
        {
            return message.MessageWithReplacementData.Trim();
        }

        if (!string.IsNullOrWhiteSpace(message.MessageText))
        {
            return message.MessageText.Trim();
        }

        return string.IsNullOrWhiteSpace(message.ReplacementData) ? string.Empty : message.ReplacementData.Trim();
    }

    /// <summary>Text to show for a message, with a translated placeholder when it has none.</summary>
    public static string DisplayText(Localizer l, QueueMessage message)
    {
        var text = DisplayText(message);
        return text.Length == 0 ? l.Get("msg.noText") : text;
    }

    /// <summary>Message id to show, or a mention for an immediate message.</summary>
    public static string DisplayId(Localizer l, QueueMessage message)
        => string.IsNullOrWhiteSpace(message.MessageId) ? l.Get("msg.immediate") : message.MessageId.Trim();

    /// <summary>
    /// Substitution data made readable. The field mixes text with binary lengths and packed
    /// dates, so non-printable characters are replaced by a dot.
    /// </summary>
    public static string? DisplayReplacementData(QueueMessage message)
    {
        if (string.IsNullOrWhiteSpace(message.ReplacementData))
        {
            return null;
        }

        var cleaned = new string([.. message.ReplacementData.Select(c => char.IsControl(c) || c > '\u00FF' ? '.' : c)]).Trim();
        return cleaned.Length == 0 ? null : cleaned;
    }

    /// <summary>
    /// Rebuilt send date and time, or null when the fields cannot be used. Both are already
    /// normalised by the API port: yyyy-MM-dd and HH:mm:ss.
    /// </summary>
    public static DateTime? SentAt(QueueMessage message)
    {
        if (!DateTime.TryParse(message.DateSent, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
        {
            return null;
        }

        return TimeSpan.TryParse(message.TimeSent, CultureInfo.InvariantCulture, out var time) ? date.Add(time) : date;
    }

    /// <summary>Short stamp for the list: the time for today's messages, the day and month otherwise.</summary>
    public static string ShortStamp(QueueMessage message)
    {
        var today = DateTime.Today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

        if (message.DateSent == today && message.TimeSent is { Length: >= 5 })
        {
            return message.TimeSent[..5];
        }

        return DateTime.TryParse(message.DateSent, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed)
            ? parsed.ToString("dd/MM", CultureInfo.InvariantCulture)
            : message.TimeSent ?? string.Empty;
    }

    /// <summary>Full date and time shown in the reading pane, in the culture of the language.</summary>
    public static string FullStamp(Localizer l, QueueMessage message)
        => SentAt(message) is { } sentAt
            ? l.FormatDate(sentAt)
            : message.TimeSent ?? l.Get("msg.unknownDate");

    /// <summary>Label of a queue delivery method.</summary>
    public static string DeliveryLabel(Localizer l, string? deliveryMethod)
    {
        var method = (deliveryMethod ?? string.Empty).Trim().ToUpperInvariant();

        return method switch
        {
            "*BREAK" => l.Get("delivery.break"),
            "*HOLD" => l.Get("delivery.hold"),
            "*NOTIFY" => l.Get("delivery.notify"),
            "*DFT" => l.Get("delivery.dft"),
            "" => l.Get("common.unset"),
            _ => method
        };
    }

    /// <summary>Translates an IBM i 0 or 1 flag into yes or no.</summary>
    public static string YesNo(Localizer l, string? flag) => (flag ?? string.Empty).Trim().ToUpperInvariant() switch
    {
        "1" or "*YES" or "Y" => l.Get("common.yes"),
        "0" or "*NO" or "N" => l.Get("common.no"),
        "" => l.Get("common.unset"),
        _ => flag!.Trim()
    };
}
