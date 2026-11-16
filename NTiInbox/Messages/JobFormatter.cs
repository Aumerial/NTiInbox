using NTiInbox.Localization;
using NTiInbox.SystemApis;

namespace NTiInbox.Messages;

/// <summary>
/// Converts IBM i job codes into readable, translated labels.
/// </summary>
public static class JobFormatter
{
    /// <summary>Label of the job type.</summary>
    public static string TypeLabel(Localizer l, string? jobType)
    {
        var code = (jobType ?? string.Empty).Trim().ToUpperInvariant();
        return code.Length == 0 ? l.Get("common.unset") : l.TryGet($"jobtype.{code}") ?? l.Get("jobtype.other", code);
    }

    /// <summary>Label of the job subtype, or null when there is none.</summary>
    public static string? SubtypeLabel(Localizer l, string? jobSubtype)
        => l.TryGet($"jobsubtype.{(jobSubtype ?? string.Empty).Trim().ToUpperInvariant()}");

    /// <summary>Label of the job status.</summary>
    public static string StatusLabel(Localizer l, string? jobStatus)
    {
        var status = (jobStatus ?? string.Empty).Trim().ToUpperInvariant();
        return status.Length == 0 ? l.Get("common.unset") : l.TryGet($"jobstatus.{status.TrimStart('*')}") ?? status;
    }

    /// <summary>CSS class of the status chip.</summary>
    public static string StatusCssClass(string? jobStatus) => (jobStatus ?? string.Empty).Trim().ToUpperInvariant() switch
    {
        "*ACTIVE" => "nti-chip-success",
        "*JOBQ" => "nti-chip-info",
        _ => "nti-chip-neutral"
    };

    /// <summary>
    /// Label of the detailed status of an active job. MSGW matters most: it flags a job
    /// stuck waiting for the reply to a message.
    /// </summary>
    public static string ActiveStatusLabel(Localizer l, string? activeStatus)
    {
        var status = (activeStatus ?? string.Empty).Trim().ToUpperInvariant();
        return status.Length == 0 ? l.Get("common.unset") : l.TryGet($"jobactive.{status}") ?? status;
    }

    /// <summary>True when the job is stuck waiting for the reply to a message.</summary>
    public static bool IsWaitingForMessage(JobInformation job)
        => string.Equals(job.ActiveJobStatus?.Trim(), "MSGW", StringComparison.OrdinalIgnoreCase)
        || string.Equals(job.MessageReply?.Trim(), "Y", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Cleans a character field. Some fields are not filled in and hold raw bytes,
    /// which would otherwise render as squares.
    /// </summary>
    public static string CleanText(string? value)
        => string.IsNullOrEmpty(value)
            ? string.Empty
            : new string([.. value.Where(c => !char.IsControl(c) && c <= '\u00FF')]).Trim();

    /// <summary>Cleaned value, or a dash when the field is empty.</summary>
    public static string OrDash(string? value)
    {
        var cleaned = CleanText(value);
        return cleaned.Length == 0 ? "-" : cleaned;
    }

    /// <summary>
    /// What the job is doing, from the function type and name: the answer to
    /// "what was this job doing".
    /// </summary>
    public static string FunctionLabel(Localizer l, JobInformation job)
    {
        var name = CleanText(job.FunctionName);

        if (name.Length == 0)
        {
            return l.Get("jobfn.none");
        }

        var type = CleanText(job.FunctionType).ToUpperInvariant();
        return type is "*" ? l.Get("jobfn.star", name) : l.TryGet($"jobfn.{type}", name) ?? name;
    }

    /// <summary>CPU time in seconds, from milliseconds, in the culture of the language.</summary>
    public static string CpuTime(Localizer l, long milliseconds)
    {
        var seconds = Math.Max(milliseconds, 0) / 1000d;

        return seconds < 60
            ? $"{seconds.ToString("0.00", l.Culture)} s"
            : $"{TimeSpan.FromMilliseconds(milliseconds):hh\\:mm\\:ss}";
    }
}
