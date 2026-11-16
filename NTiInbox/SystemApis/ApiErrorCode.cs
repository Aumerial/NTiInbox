using Aumerial.Data.Nti;

namespace NTiInbox.SystemApis;

/// <summary>
/// Error code parameter shared by the message and user APIs (ERRC0100 format).
/// </summary>
internal static class ApiErrorCode
{
    private const int StructureLength = 16;

    /// <summary>
    /// Builds the error code parameter. The first 4 bytes ("bytes provided") are set
    /// to 16, so the API returns the message id in the structure instead of escalating
    /// an exception to the job. Everything else stays at binary zero.
    /// </summary>
    public static NTiProgramParameter Create()
    {
        var buffer = new byte[StructureLength];
        buffer[3] = StructureLength;
        return new NTiProgramParameter(buffer).AsInputOutput();
    }

    /// <summary>
    /// Reads the message id from the structure. Returns an empty string when the call succeeded.
    /// </summary>
    public static string Read(NTiProgramParameter parameter)
        => parameter.GetInt(4) > 0 ? parameter.GetString(8, 7).Trim() : string.Empty;

    /// <summary>
    /// Throws an <see cref="IbmiApiException"/> when the API filled the error structure.
    /// </summary>
    public static void ThrowIfError(NTiProgramParameter parameter, string apiName)
    {
        var errorCode = Read(parameter);

        if (errorCode.Length > 0)
        {
            throw new IbmiApiException(apiName, errorCode);
        }
    }
}
