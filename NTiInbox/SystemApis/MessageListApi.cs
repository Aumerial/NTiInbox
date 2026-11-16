using System.Globalization;
using Aumerial.Data.Nti;

namespace NTiInbox.SystemApis;

/// <summary>Lists the messages of a message queue (QMHLSTM).</summary>
public static class MessageListApi
{
    /// <summary>Initial size of the user space, in bytes.</summary>
    private const int ListSpaceInitialSize = 65536;

    /// <summary>Maximum size of a user space, in bytes.</summary>
    private const int ListSpaceMaximumSize = 16 * 1024 * 1024;

    /// <summary>Size of the generic header of a user space, in bytes.</summary>
    private const int GenericHeaderSize = 192;

    /// <summary>
    /// Lists the messages of one or two message queues through the QMHLSTM API (LSTM0100 format).
    /// The result goes through a temporary user space created in QTEMP.
    /// </summary>
    /// <param name="messageQueues">Message queues to read, two at most.</param>
    /// <param name="fieldIdentifiers">Variable fields to return. Every useful field by default.</param>
    /// <param name="listDirection">List direction: *NEXT by default, or *PRV.</param>
    /// <param name="maxMessages">Maximum number of messages. -1 returns all messages.</param>
    /// <param name="selectionCriteria">Selection filter: *ALL, *MNNR, *MNR, *PAR or *SCNR.</param>
    /// <param name="startingMessageKey">Starting point: *FIRST by default, or *LAST.</param>
    /// <param name="severityCriteria">
    /// Minimum severity of the messages kept, 0 to 99. 0 returns all messages.
    /// </param>
    /// <param name="maxMessageLength">Maximum length returned for fields 0301 and 0302.</param>
    /// <param name="maxMessageHelpLength">Maximum length returned for fields 0401 to 0404.</param>
    /// <returns>The messages found, in the order returned by the API.</returns>
    public static List<QueueMessage> ListNonProgramMessages(this NTiConnection conn,
        List<(string messageQueue, string library)> messageQueues,
        List<int>? fieldIdentifiers = null,
        string listDirection = "*NEXT",
        int maxMessages = -1,
        string selectionCriteria = "*ALL",
        string startingMessageKey = "*FIRST",
        int severityCriteria = 0,
        int maxMessageLength = 512,
        int maxMessageHelpLength = 512)
    {
        conn.EnsureOpen();

        int spaceSize = ListSpaceInitialSize;
        var listNonProgramMessage = new List<QueueMessage>();

        // Variable fields requested by default
        if (fieldIdentifiers == null || fieldIdentifiers.Count == 0)
        {
            fieldIdentifiers = new List<int> { 101, 201, 301, 302, 401, 402, 403, 404, 501, 601, 602, 603, 604, 605, 607, 702, 703, 704, 705, 801, 901, 1001, 1002, 1101, 1201, 1301, 1302, 1303, 1304 };
        }

        try
        {
            while (true)
            {
                UserSpaceApi.CreateListUserSpace(conn, spaceSize);
                CallListApi(conn, messageQueues, fieldIdentifiers, listDirection, maxMessages,
                    selectionCriteria, startingMessageKey, severityCriteria, maxMessageLength, maxMessageHelpLength);

                // The generic header is reread alone: it tells how much space is really used
                // and whether the whole list could be written.
                var header = UserSpaceApi.RetrieveUserSpace(conn, "NTILMSGLI", 1, GenericHeaderSize);

                var informationStatus = header.GetString(0x67, 1);
                var sizeUsed = header.GetInt(0x68);
                var offsetToData = header.GetInt(0x7C);
                var listSize = header.GetInt(0x80);
                var numberOfEntries = header.GetInt(0x84);

                int required = Math.Max(sizeUsed, offsetToData + listSize);
                bool complete = informationStatus == "C";

                if ((!complete || required > spaceSize) && spaceSize < ListSpaceMaximumSize)
                {
                    // Space too small: retry with the size actually needed.
                    conn.ExecuteClCommand("DLTUSRSPC QTEMP/NTILMSGLI");
                    spaceSize = Math.Min(ListSpaceMaximumSize, Math.Max(required + 4096, spaceSize * 2));
                    continue;
                }

                if (numberOfEntries <= 0)
                {
                    return listNonProgramMessage;
                }

                int readLength = Math.Min(spaceSize, Math.Max(required, GenericHeaderSize));
                var userSpace = UserSpaceApi.RetrieveUserSpace(conn, "NTILMSGLI", 1, readLength);

                ReadMessageEntries(userSpace, offsetToData, numberOfEntries, readLength, listNonProgramMessage);
                return listNonProgramMessage;
            }
        }
        finally
        {
            // The temporary space is always deleted, even when the API raised an error,
            // otherwise the creation would fail on the next call.
            try
            {
                conn.ExecuteClCommand("DLTUSRSPC QTEMP/NTILMSGLI");
            }
            catch
            {
                // The space does not exist, there is nothing to clean up.
            }
        }
    }

    /// <summary>Builds the selection structure, then calls QMHLSTM.</summary>
    private static void CallListApi(NTiConnection conn,
        List<(string messageQueue, string library)> messageQueues,
        List<int> fieldIdentifiers,
        string listDirection,
        int maxMessages,
        string selectionCriteria,
        string startingMessageKey,
        int severityCriteria,
        int maxMessageLength,
        int maxMessageHelpLength)
    {
        int nbQueues = messageQueues.Count;

        // The fixed part of the MSLT0100 format takes 56 bytes: the three arrays
        // therefore start at 56, and the offsets announced to the API must match the
        // content built exactly, otherwise the API rejects the structure size with
        // CPF247D.
        int offsetMsgQueues = 56;
        int offsetMsgKeys = offsetMsgQueues + (nbQueues * 20);
        int offsetFields = offsetMsgKeys + (nbQueues * 4);

        var messageSelection = new NTiProgramParameter()
            .Append(maxMessages)
            .Append(listDirection, 10)
            .Append(selectionCriteria, 10)
            .Append(severityCriteria)
            .Append(maxMessageLength)
            .Append(maxMessageHelpLength)
            .Append(offsetMsgQueues)
            .Append(offsetMsgKeys)
            .Append(nbQueues)
            .Append(offsetFields)
            .Append(fieldIdentifiers.Count);

        // ARRAY(*) of CHAR(20): message queue names
        foreach (var mq in messageQueues)
        {
            messageSelection.Append(mq.messageQueue, 10).Append(mq.library, 10);
        }

        // ARRAY(*) of CHAR(4): starting key for each queue
        foreach (var _ in messageQueues)
        {
            byte[] startKey;

            if (startingMessageKey == "*LAST")
            {
                // 'FFFFFFFF'X: the first message returned is the newest of the queue
                startKey = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF };
            }
            else
            {
                // '00000000'X: the first message returned is the oldest of the queue
                startKey = new byte[] { 0x00, 0x00, 0x00, 0x00 };
            }

            messageSelection.Append(startKey);
        }

        // ARRAY(*) of BINARY(4): ids of the fields to return
        foreach (int field in fieldIdentifiers)
        {
            messageSelection.Append(field);
        }

        var parameters = new List<NTiProgramParameter>
        {
            new NTiProgramParameter("NTILMSGLI", 10).Append("QTEMP", 10),
            new NTiProgramParameter("LSTM0100", 8),
            messageSelection,
            new NTiProgramParameter(messageSelection.InputData.Length),
            new NTiProgramParameter("MSLT0100", 8),
            ApiErrorCode.Create()
        };

        conn.CallProgram("QSYS", "QMHLSTM", parameters);

        ApiErrorCode.ThrowIfError(parameters[5], "QMHLSTM");
    }

    /// <summary>
    /// Walks through the LSTM0100 entries present in the user space.
    /// Every access is bounded: an entry that would overrun the reread area ends the walk
    /// instead of raising a buffer overrun exception.
    /// </summary>
    private static void ReadMessageEntries(NTiProgramParameter userSpace,
        int offsetToData,
        int numberOfEntries,
        int readLength,
        List<QueueMessage> target)
    {
        int currentOffset = offsetToData;

        for (int i = 0; i < numberOfEntries; i++)
        {
            // An entry header takes 98 bytes up to and including the time zone.
            if (currentOffset < 0 || currentOffset + 98 > readLength)
            {
                return;
            }

            var message = new QueueMessage
            {
                MessageSeverity = userSpace.GetInt(currentOffset + 12),
                MessageId = userSpace.GetString(currentOffset + 16, 7).Trim(),
                MessageType = userSpace.GetString(currentOffset + 23, 2).Trim(),
                MessageKey = userSpace.GetBytes(currentOffset + 25, 4),
                MessageFileName = userSpace.GetString(currentOffset + 29, 10).Trim(),
                MessageFileLibrarySpecified = userSpace.GetString(currentOffset + 39, 10).Trim(),
                MessageQueue = userSpace.GetString(currentOffset + 49, 10).Trim(),
                MessageLibrary = userSpace.GetString(currentOffset + 59, 10).Trim(),
                DateSent = string.IsNullOrWhiteSpace(userSpace.GetString(currentOffset + 69, 7).Trim())
                    ? null
                    : DateTime.ParseExact(userSpace.GetString(currentOffset + 69, 7).Trim(), "1yyMMdd", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd"),
                TimeSent = string.IsNullOrWhiteSpace(userSpace.GetString(currentOffset + 76, 6).Trim())
                    ? null
                    : DateTime.ParseExact(userSpace.GetString(currentOffset + 76, 6).Trim(), "HHmmss", CultureInfo.InvariantCulture).ToString("HH:mm:ss"),
                Microseconds = userSpace.GetString(currentOffset + 82, 6).Trim(),
                TimeZoneAbbreviation = userSpace.GetString(currentOffset + 88, 10).Trim()
            };

            int offsetToFields = userSpace.GetInt(currentOffset + 4);
            int numberOfFields = userSpace.GetInt(currentOffset + 8);
            int fieldOffset = offsetToFields;

            for (int f = 0; f < numberOfFields; f++)
            {
                if (fieldOffset < 0 || fieldOffset + 32 > readLength)
                {
                    break;
                }

                int fieldLength = userSpace.GetInt(fieldOffset + 4);
                int identifier = userSpace.GetInt(fieldOffset + 8);
                int dataLength = userSpace.GetInt(fieldOffset + 28);

                if (dataLength < 0 || fieldOffset + 32 + dataLength > readLength)
                {
                    break;
                }

                string data = userSpace.GetString(fieldOffset + 32, dataLength).Trim();

                switch (identifier)
                {
                    case 101:
                        message.AlertOption = data;
                        break;
                    case 201:
                        message.ReplacementData = data;
                        break;
                    case 301:
                        message.MessageText = data;
                        break;
                    case 302:
                        message.MessageWithReplacementData = data;
                        break;
                    case 401:
                        message.MessageHelp = data;
                        break;
                    case 402:
                        message.MessageHelpWithReplacementData = data;
                        break;
                    case 403:
                        message.MessageHelpWithFormatting = data;
                        break;
                    case 501:
                        message.DefaultReply = data;
                        break;
                    case 601:
                        message.SenderJob = data;
                        break;
                    case 602:
                        message.SenderType = data;
                        break;
                    case 603:
                        message.SendingProgramName = data;
                        break;
                    case 604:
                        message.SendingModuleName = data;
                        break;
                    case 605:
                        message.SendingProcedureName = data;
                        break;
                    case 607:
                        message.SenderUserProfile = data;
                        break;
                    case 702:
                        message.ReceivingType = data;
                        break;
                    case 703:
                        message.ReceivingProgramName = data;
                        break;
                    case 704:
                        message.ReceivingModuleName = data;
                        break;
                    case 705:
                        message.ReceivingProcedureName = data;
                        break;
                    case 801:
                        message.MessageFileLibraryUsed = data;
                        break;
                    case 901:
                        message.ProblemId = data;
                        break;
                    case 1001:
                        message.ReplyStatus = data;
                        break;
                    case 1002:
                        message.CriticalBreakStatus = data;
                        break;
                    case 1101:
                        message.RequestStatus = data;
                        break;
                    case 1201:
                        message.RequestLevel = int.TryParse(data, out int requestLevel) ? requestLevel : 0;
                        break;
                    case 1301:
                        message.CCSID = int.TryParse(data, out int ccsid) ? ccsid : 0;
                        break;
                    case 1302:
                        message.CCSIDConversionStatusText = int.TryParse(data, out int ccsidStatusText) ? ccsidStatusText : 0;
                        break;
                    case 1303:
                        message.CCSIDData = int.TryParse(data, out int ccsidData) ? ccsidData : 0;
                        break;
                    case 1304:
                        message.CCSIDConversionStatusData = int.TryParse(data, out int ccsidStatusData) ? ccsidStatusData : 0;
                        break;
                    default:
                        break;
                }

                if (fieldLength <= 0)
                {
                    break;
                }

                fieldOffset += fieldLength;
            }

            target.Add(message);

            int nextEntryOffset = userSpace.GetInt(currentOffset + 0);

            if (nextEntryOffset <= currentOffset)
            {
                return; // No more message to read
            }

            currentOffset = nextEntryOffset;
        }
    }
}
