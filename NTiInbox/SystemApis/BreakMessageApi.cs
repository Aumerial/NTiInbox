using Aumerial.Data.Nti;

namespace NTiInbox.SystemApis;

/// <summary>Sends a break message to a user or workstation (QMHSNDBM).</summary>
public static class BreakMessageApi
{
    /// <summary>
    /// Sends an *INFO or *INQ break message to a message queue through the QMHSNDBM API.
    /// An *INFO message can target several queues, or *ALLWS by default.
    /// An *INQ message targets a single queue and requires a reply queue.
    /// </summary>
    /// <param name="messageText">Text of the message to send.</param>
    /// <param name="messageQueues">
    /// Target message queues. When the list is null and the type is *INFO,
    /// the message goes to *ALLWS.
    /// </param>
    /// <param name="replyMessageQueue">Reply queue for an *INQ message, ignored for an *INFO.</param>
    /// <param name="replyLibraryQueue">Library of the reply queue for an *INQ message.</param>
    /// <param name="messageType">Message type: *INFO by default, or *INQ.</param>
    /// <param name="ccsid">CCSID of the text. 0 uses the job CCSID.</param>
    /// <exception cref="ArgumentException">
    /// Raised when *INQ is used without a valid reply queue, or with several target queues.
    /// </exception>
    public static void SendBreakMessage(this NTiConnection conn,
        string messageText,
        List<(string messageQueue, string library)>? messageQueues = null,
        string? replyMessageQueue = null,
        string? replyLibraryQueue = null,
        string messageType = "*INFO",
        int ccsid = 0)
    {
        conn.EnsureOpen();

        // Preconditions for the call to work
        if (messageType == "*INFO")
        {
            // No queue given: *ALLWS by default
            messageQueues ??= new List<(string messageQueue, string library)> { ("*ALLWS", "") };
        }
        else if (messageType == "*INQ")
        {
            // An *INQ targets a single queue, never *ALLWS
            if (messageQueues == null || messageQueues.Count != 1)
            {
                throw new ArgumentException("For *INQ messages, you must specify one message queue.");
            }

            if (messageQueues[0].messageQueue == "*ALLWS")
            {
                throw new ArgumentException("*ALLWS cannot be used for *INQ messages. Specify a single message queue.");
            }

            if (replyMessageQueue == null || replyLibraryQueue == null)
            {
                throw new ArgumentException("For *INQ messages, you must specify a reply message queue and library.");
            }
        }

        int nbMessageQueues = messageQueues!.Count;

        // Same continuous 20-byte zone per queue as in QMHSNDM: an array does not
        // reach the API complete as soon as it has more than one element.
        var formattedMessageQueues = string.Concat(messageQueues
            .Select(q => q.messageQueue.PadRight(10) + q.library.PadRight(10)));

        var parameters = new List<NTiProgramParameter>
        {
            new NTiProgramParameter(messageText, messageText.Length),
            new NTiProgramParameter(messageText.Length),
            new NTiProgramParameter(messageType, 10),
            new NTiProgramParameter(formattedMessageQueues, formattedMessageQueues.Length),
            new NTiProgramParameter(nbMessageQueues)
        };

        // Reply queue for an *INQ, empty zone for an *INFO
        if (messageType == "*INQ" && replyMessageQueue != null && replyLibraryQueue != null)
        {
            parameters.Add(new NTiProgramParameter(replyMessageQueue, 10).Append(replyLibraryQueue, 10));
        }
        else
        {
            parameters.Add(new NTiProgramParameter("", 10).Append("", 10));
        }

        parameters.Add(ApiErrorCode.Create());
        parameters.Add(new NTiProgramParameter(ccsid));

        conn.CallProgram("QSYS", "QMHSNDBM", parameters);

        ApiErrorCode.ThrowIfError(parameters[6], "QMHSNDBM");
    }
}
