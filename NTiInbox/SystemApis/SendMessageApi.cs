using Aumerial.Data.Nti;

namespace NTiInbox.SystemApis;

/// <summary>Sends an informational or inquiry message to a queue (QMHSNDM).</summary>
public static class SendMessageApi
{
    /// <summary>
    /// Sends a message to one or more IBM i message queues through the QMHSNDM API.
    /// Accepted types: *INFO, *COMP, *DIAG and *INQ. An *INQ message requires a reply queue.
    /// </summary>
    /// <param name="messageText">
    /// Message text. Used as the full message when no id is given, otherwise it holds
    /// the substitution data.
    /// </param>
    /// <param name="messageType">Message type: *INFO, *COMP, *DIAG or *INQ.</param>
    /// <param name="qualifiedMessageQueues">
    /// Target message queues, as name on 10 characters followed by library on 10
    /// characters. The special values *SYSOPR, *HSTLOG, *CURLIB and *LIBL are accepted.
    /// Up to 50 queues, or 2 with *HSTLOG.
    /// </param>
    /// <param name="replyMessageQueue">Reply queue, used only for an *INQ message.</param>
    /// <param name="replyLibraryQueue">Library of the reply queue, only for an *INQ.</param>
    /// <param name="messageId">Id of a predefined message. Optional.</param>
    /// <param name="fileName">Message file, ignored without an id. Optional.</param>
    /// <param name="libraryName">Message file library, ignored without an id. Optional.</param>
    public static void SendNonProgramMessage(this NTiConnection conn,
        string messageText,
        string messageType,
        List<(string messageQueue, string library)> qualifiedMessageQueues,
        string? replyMessageQueue = null,
        string? replyLibraryQueue = null,
        string messageId = "",
        string fileName = "",
        string libraryName = "")
    {
        conn.EnsureOpen();

        // Number of message queues
        int numberOfMessageQueues = qualifiedMessageQueues.Count;

        // The parameter expects an ARRAY(*) of CHAR(20), that is a continuous zone of
        // twenty bytes per queue. The zone is therefore assembled here rather than passed
        // as an array: beyond the first queue, the array did not reach the API complete
        // and only one of the queues received the message.
        var formattedMessageQueues = string.Concat(qualifiedMessageQueues
            .Select(q => q.messageQueue.PadRight(10) + q.library.PadRight(10)));

        var parameters = new List<NTiProgramParameter>()
        {
            new NTiProgramParameter(messageId, 7),
        };

        // Immediate message: empty id, so file and library are ignored
        if (!string.IsNullOrWhiteSpace(messageId))
        {
            parameters.Add(new NTiProgramParameter(fileName, 10).Append(libraryName, 10));
        }
        else
        {
            parameters.Add(new NTiProgramParameter("", 10).Append("", 10));
        }

        // Remaining parameters
        parameters.Add(new NTiProgramParameter(messageText, messageText.Length));
        parameters.Add(new NTiProgramParameter(messageText.Length));
        parameters.Add(new NTiProgramParameter(messageType, 10));
        parameters.Add(new NTiProgramParameter(formattedMessageQueues, formattedMessageQueues.Length));
        parameters.Add(new NTiProgramParameter(numberOfMessageQueues));

        // An *INQ message needs the reply queue and its library
        if (messageType == "*INQ" && replyMessageQueue != null && replyLibraryQueue != null)
        {
            parameters.Add(new NTiProgramParameter(replyMessageQueue, 10).Append(replyLibraryQueue, 10));
        }
        else
        {
            parameters.Add(new NTiProgramParameter("", 10).Append("", 10));
        }

        parameters.Add(new NTiProgramParameter("", 4).AsOutput());
        parameters.Add(ApiErrorCode.Create());

        conn.CallProgram("QSYS", "QMHSNDM", parameters);

        ApiErrorCode.ThrowIfError(parameters[9], "QMHSNDM");
    }
}
