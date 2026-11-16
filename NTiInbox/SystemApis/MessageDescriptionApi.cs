using Aumerial.Data.Nti;

namespace NTiInbox.SystemApis;

/// <summary>Retrieves the description of a message from a message file (QMHRTVM).</summary>
public static class MessageDescriptionApi
{
    /// <summary>
    /// Retrieves the description of a predefined message through the QMHRTVM API (RTVM0300 format).
    /// This format returns the text, the help, the default reply and the definition of the
    /// substitution variables.
    /// </summary>
    /// <param name="messageId">Message id on 7 characters, for example CPF2407.</param>
    /// <param name="messageFile">Message file name, for example QCPFMSG.</param>
    /// <param name="messageLibrary">Message file library, for example QSYS.</param>
    /// <param name="replacementData">
    /// Optional substitution values. Each string is padded to 32 bytes.
    /// </param>
    /// <param name="replaceSubstitution">Apply the values in the text: *YES by default, or *NO.</param>
    /// <param name="returnFormatControls">Return formatting characters: *NO by default, or *YES.</param>
    /// <param name="retrieveOption">Retrieve option: *MSGID by default, *FIRST or *NEXT.</param>
    /// <param name="ccsidToConvertTo">CCSID to convert the text to. 0 uses the job CCSID.</param>
    /// <param name="ccsidReplacementData">CCSID of the substitution data. 0 uses the job CCSID.</param>
    /// <returns>The full description of the message.</returns>
    /// <remarks>
    /// The available length fields and the offset to the default reply are not read here,
    /// as in the original port. The DefaultReply property therefore stays null.
    /// </remarks>
    public static MessageDescription RetrieveMessageDescription(this NTiConnection conn,
        string messageId,
        string messageFile,
        string messageLibrary,
        string[]? replacementData = null,
        string replaceSubstitution = "*YES",
        string returnFormatControls = "*NO",
        string retrieveOption = "*MSGID",
        int ccsidToConvertTo = 0,
        int ccsidReplacementData = 0)
    {
        conn.EnsureOpen();

        int bufferSize = 5000;
        var outputBuffer = new byte[bufferSize];

        var parameters = new List<NTiProgramParameter>
        {
            new NTiProgramParameter(outputBuffer).AsOutput(),
            new NTiProgramParameter(bufferSize),
            new NTiProgramParameter("RTVM0300", 8),
            new NTiProgramParameter(messageId, 7),
            new NTiProgramParameter(messageFile, 10).Append(messageLibrary, 10),
            new NTiProgramParameter(replacementData?.Select(d => d.PadRight(32)).ToArray() ?? Array.Empty<string>(), 32),
            new NTiProgramParameter(replacementData?.Length * 32 ?? 0),
            new NTiProgramParameter(replaceSubstitution, 10),
            new NTiProgramParameter(returnFormatControls, 10),
            ApiErrorCode.Create(),
            new NTiProgramParameter(retrieveOption, 10),
            new NTiProgramParameter(ccsidToConvertTo),
            new NTiProgramParameter(ccsidReplacementData)
        };

        conn.CallProgram("QSYS", "QMHRTVM", parameters);

        ApiErrorCode.ThrowIfError(parameters[9], "QMHRTVM");

        var buffer = parameters[0];

        var result = new MessageDescription
        {
            MessageSeverity = buffer.GetInt(8),
            AlertIndex = buffer.GetInt(12),
            AlertOption = buffer.GetString(16, 9).Trim(),
            LogIndicator = buffer.GetString(25, 1),
            MessageId = buffer.GetString(26, 7),
            SubstitutionVariableFormatCount = buffer.GetInt(36),
            CCSIDConversionStatusText = buffer.GetInt(40),
            CCSIDConversionStatusData = buffer.GetInt(44),
            CCSIDText = buffer.GetInt(48),
            OffsetMessage = buffer.GetInt(64),
            LengthMessageReturned = buffer.GetInt(68),
            OffsetMessageHelp = buffer.GetInt(76),
            LengthMessageHelpReturned = buffer.GetInt(80),
            OffsetSubstitutionFormats = buffer.GetInt(88),
            LengthSubstitutionFormatsReturned = buffer.GetInt(92),
            LengthSubstitutionFormatElement = buffer.GetInt(100)
        };

        result.DefaultReply = result.LengthDefaultReplyReturned > 0
            ? buffer.GetString(result.OffsetDefaultReply, result.LengthDefaultReplyReturned)
            : null;

        result.Message = result.LengthMessageReturned > 0
            ? buffer.GetString(result.OffsetMessage, result.LengthMessageReturned)
            : null;

        result.MessageHelp = result.LengthMessageHelpReturned > 0
            ? buffer.GetString(result.OffsetMessageHelp, result.LengthMessageHelpReturned)
            : null;

        for (int i = 0; i < result.SubstitutionVariableFormatCount; i++)
        {
            int entryOffset = result.OffsetSubstitutionFormats + i * result.LengthSubstitutionFormatElement;

            result.SubstitutionFormats.Add(new MessageDescription.SubstitutionVariableFormat
            {
                ReplacementDataLength = buffer.GetInt(entryOffset),
                FieldSizeOrDecimalPositions = buffer.GetInt(entryOffset + 4),
                SubstitutionVariableType = buffer.GetString(entryOffset + 8, 10).Trim()
            });
        }

        return result;
    }
}
