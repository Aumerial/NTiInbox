namespace NTiInbox.SystemApis;

/// <summary>
/// IBM i object returned by QUSLOBJ (OBJL0200 format).
/// Used here to discover the message queues visible from the current job.
/// </summary>
public class QueueListEntry
{
    /// <summary>Object name.</summary>
    public string ObjectName { get; set; } = string.Empty;

    /// <summary>Library holding the object.</summary>
    public string ObjectLibrary { get; set; } = string.Empty;

    /// <summary>Object type, for example *MSGQ.</summary>
    public string ObjectType { get; set; } = string.Empty;

    /// <summary>
    /// Information status. Empty when everything is readable, A when authority is
    /// missing, L when the object is locked, D when it is damaged.
    /// </summary>
    public string InformationStatus { get; set; } = string.Empty;

    /// <summary>Extended attribute of the object.</summary>
    public string ExtendedAttribute { get; set; } = string.Empty;

    /// <summary>Descriptive text of the object.</summary>
    public string TextDescription { get; set; } = string.Empty;
}
