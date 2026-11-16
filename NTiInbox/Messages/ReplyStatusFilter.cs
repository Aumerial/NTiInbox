namespace NTiInbox.Messages;

/// <summary>Filter on the reply status of a message.</summary>
public enum ReplyStatusFilter
{
    /// <summary>No filter.</summary>
    All,

    /// <summary>Only messages waiting for a reply.</summary>
    Waiting,

    /// <summary>Only messages already answered.</summary>
    Answered,

    /// <summary>Only messages that expect no reply.</summary>
    None
}
