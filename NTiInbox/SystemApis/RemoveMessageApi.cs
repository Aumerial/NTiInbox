using Aumerial.Data.Nti;

namespace NTiInbox.SystemApis;

/// <summary>Removes messages from a message queue (QMHRMVM).</summary>
public static class RemoveMessageApi
{
    /// <summary>
    /// Removes one or more messages from a message queue through the QMHRMVM API.
    /// </summary>
    /// <param name="messageQueueName">Message queue name, on 10 characters.</param>
    /// <param name="libraryName">Queue library, on 10 characters.</param>
    /// <param name="messageKey">
    /// Key of the message to remove, required when <paramref name="removeOption"/> is *BYKEY.
    /// </param>
    /// <param name="removeOption">
    /// Messages to remove: *BYKEY, *ALL, *KEEPUNANS, *NEW or *OLD. *BYKEY by default.
    /// </param>
    public static void RemoveNonProgramMessage(this NTiConnection conn,
        string messageQueueName,
        string libraryName,
        byte[]? messageKey = null,
        string removeOption = "*BYKEY")
    {
        conn.EnsureOpen();

        var parameters = new List<NTiProgramParameter>
        {
            new NTiProgramParameter(messageQueueName, 10).Append(libraryName, 10)
        };

        if (removeOption == "*BYKEY")
        {
            var key = messageKey ?? new byte[4]; // No key: empty 4-byte array
            parameters.Add(new NTiProgramParameter(key));
        }
        else
        {
            parameters.Add(new NTiProgramParameter("", 4));
        }

        parameters.Add(new NTiProgramParameter(removeOption, 10));
        parameters.Add(ApiErrorCode.Create());

        conn.CallProgram("QSYS", "QMHRMVM", parameters);

        ApiErrorCode.ThrowIfError(parameters[3], "QMHRMVM");
    }
}
