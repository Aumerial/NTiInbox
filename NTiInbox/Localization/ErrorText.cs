using NTiInbox.Connection;
using NTiInbox.SystemApis;

namespace NTiInbox.Localization;

/// <summary>
/// Turns an exception into text for the user: translated when it is a known case,
/// and always stripped of credentials. IBM i texts stay in the job language.
/// </summary>
public sealed class ErrorText(Localizer localizer, IbmiSession connection)
{
    /// <summary>Text to display for an exception.</summary>
    public string Describe(Exception exception) => exception switch
    {
        UserMessageException user => localizer.Get(user.Key, user.Args),
        ConnectionException { LicenseMissing: true } => localizer.Get("session.licenseMissing"),
        ConnectionException failure => failure.Message,
        IbmiApiException api => localizer.TryGet($"cpf.{api.ErrorCode}") is { } known
            ? $"{api.ErrorCode}: {known}"
            : $"{api.ErrorCode} ({api.ApiName})",
        _ => connection.Sanitize(exception.Message)
    };
}
