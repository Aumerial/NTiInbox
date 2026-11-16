namespace NTiInbox.SystemApis;

/// <summary>
/// Error raised by an IBM i API. The code is the message id returned in the
/// error structure, for example CPF2403.
/// </summary>
public class IbmiApiException : InvalidOperationException
{
    public IbmiApiException(string apiName, string errorCode)
        : base($"Error {errorCode}")
    {
        ApiName = apiName;
        ErrorCode = errorCode;
    }

    /// <summary>Name of the system program that was called, for example QMHSNDM.</summary>
    public string ApiName { get; }

    /// <summary>Message id returned by the API.</summary>
    public string ErrorCode { get; }
}
