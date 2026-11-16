namespace NTiInbox.SystemApis;

/// <summary>
/// User input for sending a message through QMHSNDM.
/// </summary>
public class SendMessageRequest
{
    /// <summary>Maximum number of target queues accepted by the API.</summary>
    public const int MaximumTargets = 50;

    /// <summary>Message text, or substitution data when an id is given.</summary>
    public string MessageText { get; set; } = string.Empty;

    /// <summary>Message type: *INFO, *COMP, *DIAG or *INQ.</summary>
    public string MessageType { get; set; } = "*INFO";

    /// <summary>
    /// Target message queues. The API accepts up to fifty,
    /// which lets a whole team be warned in one action.
    /// </summary>
    public List<MessageQueueRef> Targets { get; set; } = new();

    /// <summary>Reply queue, required for an *INQ message.</summary>
    public string ReplyQueue { get; set; } = string.Empty;

    /// <summary>Library of the reply queue.</summary>
    public string ReplyLibrary { get; set; } = "*LIBL";

    /// <summary>Id of a predefined message, left empty for an immediate message.</summary>
    public string MessageId { get; set; } = string.Empty;

    /// <summary>Message file, used only with an id.</summary>
    public string MessageFile { get; set; } = "QCPFMSG";

    /// <summary>Library of the message file.</summary>
    public string MessageFileLibrary { get; set; } = "QSYS";

    /// <summary>True when the message waits for a reply.</summary>
    public bool IsInquiry => string.Equals(MessageType, "*INQ", StringComparison.OrdinalIgnoreCase);

    /// <summary>True when a predefined message id was entered.</summary>
    public bool UsesPredefinedMessage => !string.IsNullOrWhiteSpace(MessageId);

    /// <summary>
    /// Adds a target queue when it is not there yet and the API limit is not reached.
    /// </summary>
    public bool AddTarget(MessageQueueRef queue)
    {
        if (string.IsNullOrWhiteSpace(queue.Name) || Targets.Count >= MaximumTargets || Targets.Contains(queue))
        {
            return false;
        }

        Targets.Add(queue);
        return true;
    }
}
