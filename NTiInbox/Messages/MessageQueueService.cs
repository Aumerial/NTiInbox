using NTiInbox.Connection;
using NTiInbox.Localization;
using NTiInbox.SystemApis;

namespace NTiInbox.Messages;

/// <summary>
/// Single entry point of the business logic: message queues, messages, catalog, jobs and
/// the connected profile. It knows neither Blazor nor the connection details.
/// </summary>
public sealed class MessageQueueService(IbmiSession connection)
{
    /// <summary>
    /// Messages read per call, newest first. A system queue can hold thousands of messages
    /// and reading them all would only saturate the user space.
    /// </summary>
    public const int PageSize = 300;

    // QMHLSTM refuses some system queues (QHST first) instead of returning an empty list.
    private const string UnsupportedQueueCode = "CPF2433";

    // Job-not-found codes. An ended job is the normal case for most senders.
    private static readonly string[] EndedJobCodes = ["CPF3C53", "CPF3C55", "CPF3C52"];

    // Maximum length of the help text returned by the listing, in bytes.
    private const int HelpMaxLength = 4000;

    // Variable fields requested from QMHLSTM. Only the help with its substitution values
    // applied (0402) is read: the raw help (0401) would show the placeholders (&1, &2...)
    // of the message file, and QMHRTVM cannot apply values the listing returns as text.
    private static readonly List<int> ListFields =
    [
        201,  // Substitution data or immediate text
        301,  // Message text
        302,  // Text with substitution
        402,  // Help with substitution
        501,  // Default reply
        601,  // Sender job
        607,  // Sender profile
        801,  // Message file library actually used
        1001, // Reply status
        1002  // Alert option
    ];

    /// <summary>Message files offered in the catalog lookup (name, library).</summary>
    public static IReadOnlyList<(string File, string Library)> CommonMessageFiles { get; } =
    [
        ("QCPFMSG", "QSYS"),
        ("QCPFMSG2", "QSYS"),
        ("QRPGMSG", "QSYS"),
        ("QSQLMSG", "QSYS"),
        ("QCEEMSG", "QSYS"),
        ("QTCPMSGF", "QSYS")
    ];

    // ---- Message queues -------------------------------------------------

    /// <summary>
    /// Lists the *MSGQ objects of a library or special value. *LIBL is the fastest,
    /// *ALLUSR sweeps the whole system.
    /// </summary>
    public async Task<List<MessageQueueRef>> DiscoverQueuesAsync(string library = "*LIBL")
    {
        var searchLibrary = string.IsNullOrWhiteSpace(library) ? "*LIBL" : library.Trim().ToUpperInvariant();

        var objects = await connection.RunAsync(conn => conn.ListObjects("*ALL", searchLibrary, "*MSGQ"));

        return [.. objects
            .Where(o => !string.IsNullOrWhiteSpace(o.ObjectName))
            .Select(o => new MessageQueueRef(o.ObjectName, o.ObjectLibrary, o.TextDescription))
            .DistinctBy(q => q.Key)
            .OrderBy(q => q.Name, StringComparer.OrdinalIgnoreCase)];
    }

    /// <summary>Reads the attributes of a message queue.</summary>
    public Task<QueueAttributes> GetAttributesAsync(MessageQueueRef queue)
        => connection.RunAsync(conn => conn.RetrieveMessageQueueAttributes(queue.Name, queue.Library));

    // ---- Messages -------------------------------------------------------

    /// <summary>Reads a queue, newest message first, like an inbox.</summary>
    /// <returns>
    /// The messages, or null when the queue is a system queue the API refuses to read.
    /// That is not a failure, the caller shows it as such.
    /// </returns>
    public Task<List<QueueMessage>?> ListAsync(MessageQueueRef queue, int maxMessages = PageSize)
    {
        List<(string messageQueue, string library)> queues = [(queue.Name, queue.Library)];

        return connection.RunAsync<List<QueueMessage>?>(conn =>
        {
            try
            {
                return conn.ListNonProgramMessages(
                    queues,
                    fieldIdentifiers: [.. ListFields],
                    listDirection: "*PRV",
                    maxMessages: maxMessages,
                    startingMessageKey: "*LAST",
                    severityCriteria: 0,
                    maxMessageLength: 512,
                    maxMessageHelpLength: HelpMaxLength);
            }
            catch (IbmiApiException ex) when (ex.ErrorCode == UnsupportedQueueCode)
            {
                // Caught inside the delegate, otherwise the debugger would break on
                // every click on QHST although the case is expected.
                return null;
            }
        });
    }

    /// <summary>
    /// Returns the help text of a predefined message, with its substitution values applied.
    /// An immediate message has none, in which case the result is null.
    /// </summary>
    public async Task<string?> GetMessageHelpAsync(QueueMessage message)
    {
        if (string.IsNullOrWhiteSpace(message.MessageId) || string.IsNullOrWhiteSpace(message.MessageFileName))
        {
            return null;
        }

        // The listing already holds the help as the system formats it for this message.
        if (!string.IsNullOrWhiteSpace(message.MessageHelpWithReplacementData))
        {
            return message.MessageHelpWithReplacementData;
        }

        var library = !string.IsNullOrWhiteSpace(message.MessageFileLibraryUsed)
            ? message.MessageFileLibraryUsed!
            : string.IsNullOrWhiteSpace(message.MessageFileLibrarySpecified) ? "QSYS" : message.MessageFileLibrarySpecified!;

        var description = await connection.RunAsync(conn => conn.RetrieveMessageDescription(
            message.MessageId!.Trim(),
            message.MessageFileName!.Trim(),
            library.Trim(),
            replaceSubstitution: "*NO"));

        return description.MessageHelp?.Trim();
    }

    /// <summary>Replies to an inquiry message. The key comes from the queue listing.</summary>
    public Task ReplyAsync(MessageQueueRef queue, byte[] messageKey, string reply, bool removeAfterReply = false)
    {
        if (messageKey is not { Length: > 0 })
        {
            throw new UserMessageException("error.noMessageKey");
        }

        var removeOption = removeAfterReply ? "*YES" : "*NO";
        return connection.RunAsync(conn => conn.SendReplyMessage(messageKey, queue.Name, queue.Library, reply, removeOption));
    }

    /// <summary>Sends a message to one or more queues, immediate or predefined.</summary>
    public Task SendAsync(SendMessageRequest request)
    {
        if (request.Targets.Count == 0)
        {
            throw new UserMessageException("error.noTarget");
        }

        if (request.Targets.Count > SendMessageRequest.MaximumTargets)
        {
            throw new UserMessageException("error.tooManyTargets", SendMessageRequest.MaximumTargets);
        }

        if (!request.UsesPredefinedMessage && string.IsNullOrWhiteSpace(request.MessageText))
        {
            throw new UserMessageException("error.noText");
        }

        if (request.IsInquiry && string.IsNullOrWhiteSpace(request.ReplyQueue))
        {
            throw new UserMessageException("error.inquiryNeedsReplyQueue");
        }

        // An inquiry sent to several queues would create as many unrelated messages
        // to answer, so the application refuses it.
        if (request.IsInquiry && request.Targets.Count > 1)
        {
            throw new UserMessageException("error.inquirySingleTarget");
        }

        var targets = request.Targets.Select(t => (messageQueue: t.Name, library: t.Library)).ToList();

        // QMHSNDM refuses an empty text: a predefined message without substitution data
        // is sent with a single space.
        var text = string.IsNullOrEmpty(request.MessageText) ? " " : request.MessageText;

        var replyQueue = request.IsInquiry ? request.ReplyQueue.Trim().ToUpperInvariant() : null;
        var replyLibrary = request.IsInquiry ? request.ReplyLibrary.Trim().ToUpperInvariant() : null;

        var predefined = request.UsesPredefinedMessage;
        var messageId = predefined ? request.MessageId.Trim().ToUpperInvariant() : string.Empty;
        var messageFile = predefined ? request.MessageFile.Trim().ToUpperInvariant() : string.Empty;
        var messageFileLibrary = predefined ? request.MessageFileLibrary.Trim().ToUpperInvariant() : string.Empty;

        return connection.RunAsync(conn => conn.SendNonProgramMessage(
            text, request.MessageType, targets, replyQueue, replyLibrary, messageId, messageFile, messageFileLibrary));
    }

    /// <summary>
    /// Sends a break message. The caller must have obtained an explicit confirmation:
    /// the message really interrupts the recipient's session.
    /// </summary>
    public Task SendBreakAsync(BreakMessageRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.MessageText))
        {
            throw new UserMessageException("error.noText");
        }

        if (string.IsNullOrWhiteSpace(request.TargetQueue))
        {
            throw new UserMessageException("error.noBreakTarget");
        }

        List<(string messageQueue, string library)> targets =
            [(request.TargetQueue.Trim().ToUpperInvariant(), request.TargetLibrary.Trim().ToUpperInvariant())];

        var replyQueue = request.IsInquiry ? request.ReplyQueue.Trim().ToUpperInvariant() : null;
        var replyLibrary = request.IsInquiry ? request.ReplyLibrary.Trim().ToUpperInvariant() : null;

        return connection.RunAsync(conn => conn.SendBreakMessage(
            request.MessageText, targets, replyQueue, replyLibrary, request.MessageType));
    }

    /// <summary>Removes one message from a queue.</summary>
    public Task RemoveAsync(MessageQueueRef queue, byte[] messageKey)
    {
        if (messageKey is not { Length: > 0 })
        {
            throw new UserMessageException("error.noMessageKey");
        }

        return connection.RunAsync(conn => conn.RemoveNonProgramMessage(queue.Name, queue.Library, messageKey));
    }

    /// <summary>
    /// Removes messages designated by their keys. The API handles one key at a time, so
    /// the calls are chained in a single round trip to the system.
    /// </summary>
    /// <returns>Removed count, failed count and the first error, so a fully failed batch says why.</returns>
    public Task<(int Removed, int Failed, Exception? FirstError)> RemoveKeysAsync(
        MessageQueueRef queue,
        IReadOnlyList<byte[]> messageKeys)
    {
        if (messageKeys.Count == 0)
        {
            return Task.FromResult<(int, int, Exception?)>((0, 0, null));
        }

        return connection.RunAsync(conn =>
        {
            var removed = 0;
            var failed = 0;
            Exception? firstError = null;

            foreach (var key in messageKeys)
            {
                try
                {
                    conn.RemoveNonProgramMessage(queue.Name, queue.Library, key);
                    removed++;
                }
                catch (Exception ex)
                {
                    // A message already removed meanwhile must not stop the batch,
                    // but the first cause is kept for display.
                    failed++;
                    firstError ??= ex;
                }
            }

            return (removed, failed, firstError);
        });
    }

    // ---- Catalog --------------------------------------------------------

    /// <summary>
    /// Looks up a predefined message. Substitution values are shown as they are,
    /// without replacement.
    /// </summary>
    public Task<MessageDescription> LookupMessageAsync(string messageId, string messageFile, string messageLibrary)
    {
        if (string.IsNullOrWhiteSpace(messageId))
        {
            throw new UserMessageException("error.noMessageId");
        }

        var id = messageId.Trim().ToUpperInvariant();
        var file = string.IsNullOrWhiteSpace(messageFile) ? "QCPFMSG" : messageFile.Trim().ToUpperInvariant();
        var library = string.IsNullOrWhiteSpace(messageLibrary) ? "QSYS" : messageLibrary.Trim().ToUpperInvariant();

        return connection.RunAsync(conn => conn.RetrieveMessageDescription(id, file, library, replaceSubstitution: "*NO"));
    }

    // ---- Jobs and profile -----------------------------------------------

    /// <summary>Reads a job by qualified name, or null when it has left the system.</summary>
    public Task<JobInformation?> GetJobAsync(JobRef job)
        => connection.RunAsync<JobInformation?>(conn =>
        {
            // Caught inside the delegate so the debugger does not break on every ended job.
            try
            {
                return conn.RetrieveJobInformation(job.Name, job.User, job.Number);
            }
            catch (IbmiApiException ex) when (EndedJobCodes.Contains(ex.ErrorCode))
            {
                return null;
            }
        });

    /// <summary>
    /// Parses the sender job field of a message (field 0601): three fixed-length zones,
    /// name on 10, user on 10 and number on 6.
    /// </summary>
    public static JobRef? ParseSenderJob(string? senderJob)
    {
        if (string.IsNullOrWhiteSpace(senderJob))
        {
            return null;
        }

        // Only trailing blanks were trimmed, so the field is padded back to 26.
        var field = senderJob.PadRight(26);
        var job = new JobRef(field[..10], field[10..20], field[20..26]);

        return job.IsComplete ? job : null;
    }

    /// <summary>
    /// Reads the connected profile. *CURRENT designates the job's profile, which avoids
    /// depending on case or on a group profile.
    /// </summary>
    public Task<UserInformation> GetCurrentUserAsync()
        => connection.RunAsync(conn => conn.RetrieveUserInformation("*CURRENT"));

    /// <summary>
    /// Reads the connected profile once after sign-on and records its elevated authorities,
    /// which feed the warning banner of destructive actions.
    /// </summary>
    public async Task LoadSessionAsync()
    {
        var user = await GetCurrentUserAsync();
        connection.SetElevatedAuthorities(user.SpecialAuthorities
            .Select(a => a.Trim())
            .Where(a => a is "*ALLOBJ" or "*SECADM"));
    }
}
