namespace NTiInbox.SystemApis;

/// <summary>
/// Description of a predefined message, as returned by QMHRTVM (RTVM0300 format).
/// Holds the text, the help, the default reply and the substitution variable formats.
/// </summary>
public class MessageDescription
{
    /// <summary>Message severity, 0 to 99.</summary>
    public int MessageSeverity { get; set; }

    /// <summary>Resource index for the alert.</summary>
    public int AlertIndex { get; set; }

    /// <summary>Alert option: *IMMED, *DEFER, *NO...</summary>
    public string? AlertOption { get; set; }

    /// <summary>Message logging indicator: Y or N.</summary>
    public string? LogIndicator { get; set; }

    /// <summary>Message id, for example CPF9898.</summary>
    public string? MessageId { get; set; }

    /// <summary>Number of substitution variable formats.</summary>
    public int SubstitutionVariableFormatCount { get; set; }

    /// <summary>CCSID conversion status of the text.</summary>
    public int CCSIDConversionStatusText { get; set; }

    /// <summary>CCSID conversion status of the substitution data.</summary>
    public int CCSIDConversionStatusData { get; set; }

    /// <summary>CCSID of the returned text.</summary>
    public int CCSIDText { get; set; }

    /// <summary>Offset to the default reply.</summary>
    public int OffsetDefaultReply { get; set; }

    /// <summary>Returned length of the default reply.</summary>
    public int LengthDefaultReplyReturned { get; set; }

    /// <summary>Available length of the default reply.</summary>
    public int LengthDefaultReplyAvailable { get; set; }

    /// <summary>Offset to the message text.</summary>
    public int OffsetMessage { get; set; }

    /// <summary>Returned length of the message text.</summary>
    public int LengthMessageReturned { get; set; }

    /// <summary>Available length of the message text.</summary>
    public int LengthMessageAvailable { get; set; }

    /// <summary>Offset to the message help.</summary>
    public int OffsetMessageHelp { get; set; }

    /// <summary>Returned length of the message help.</summary>
    public int LengthMessageHelpReturned { get; set; }

    /// <summary>Available length of the message help.</summary>
    public int LengthMessageHelpAvailable { get; set; }

    /// <summary>Offset to the substitution formats.</summary>
    public int OffsetSubstitutionFormats { get; set; }

    /// <summary>Returned length of the substitution formats.</summary>
    public int LengthSubstitutionFormatsReturned { get; set; }

    /// <summary>Available length of the substitution formats.</summary>
    public int LengthSubstitutionFormatsAvailable { get; set; }

    /// <summary>Length of one substitution format element.</summary>
    public int LengthSubstitutionFormatElement { get; set; }

    /// <summary>Default reply text.</summary>
    public string? DefaultReply { get; set; }

    /// <summary>Message text.</summary>
    public string? Message { get; set; }

    /// <summary>Message help.</summary>
    public string? MessageHelp { get; set; }

    /// <summary>List of substitution variable definitions.</summary>
    public List<SubstitutionVariableFormat> SubstitutionFormats { get; set; } = new List<SubstitutionVariableFormat>();

    /// <summary>
    /// Format of a substitution variable.
    /// </summary>
    public class SubstitutionVariableFormat
    {
        /// <summary>Length of the substitution data for this variable.</summary>
        public int ReplacementDataLength { get; set; }

        /// <summary>Field size or number of decimal positions.</summary>
        public int FieldSizeOrDecimalPositions { get; set; }

        /// <summary>Substitution variable type: *CHAR, *DEC, *BIN...</summary>
        public string? SubstitutionVariableType { get; set; }
    }
}
