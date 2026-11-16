using Aumerial.Data.Nti;

namespace NTiInbox.SystemApis;

/// <summary>Replies to an inquiry message (QMHSNDRM).</summary>
public static class ReplyMessageApi
{
    /// <summary>
    /// Replies to an inquiry message (*INQ) in a queue through the QMHSNDRM API.
    /// </summary>
    /// <param name="messageKey">
    /// Key of the message to reply to, obtained when the queue was listed.
    /// </param>
    /// <param name="messageQueueName">Message queue name, on 10 characters.</param>
    /// <param name="libraryName">Queue library, on 10 characters.</param>
    /// <param name="reply">Text of the reply sent to the sender.</param>
    /// <param name="removeOption">Remove the message after the reply: *NO or *YES.</param>
    /// <remarks>
    /// The error code area is 16 bytes, not 8: the error message id starts at byte 8 of the
    /// standard structure and would be unreadable on 8 bytes.
    /// </remarks>
    public static void SendReplyMessage(this NTiConnection conn,
        byte[] messageKey,
        string messageQueueName,
        string libraryName,
        string reply,
        string removeOption = "*NO")
    {
        conn.EnsureOpen();

        var parameters = new List<NTiProgramParameter>
        {
            new NTiProgramParameter(messageKey),
            new NTiProgramParameter(messageQueueName, 10).Append(libraryName, 10),
            new NTiProgramParameter(reply, reply.Length),
            new NTiProgramParameter(reply.Length),
            new NTiProgramParameter(removeOption, 10), // *NO or *YES
            ApiErrorCode.Create()
        };

        conn.CallProgram("QSYS", "QMHSNDRM", parameters);

        ApiErrorCode.ThrowIfError(parameters[5], "QMHSNDRM");
    }
}
