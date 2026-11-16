namespace NTiInbox.SystemApis;

/// <summary>
/// Message queue attributes, as returned by QMHRMQAT (RMQA0100 format).
/// </summary>
public class QueueAttributes
{
    /// <summary>Name of the message queue used.</summary>
    public string? MessageQueueName { get; set; }

    /// <summary>Library holding the message queue.</summary>
    public string? MessageQueueLibrary { get; set; }

    /// <summary>Number of messages currently in the queue.</summary>
    public int NumberOfMessagesOnQueue { get; set; }

    /// <summary>Current storage size of the queue, in bytes.</summary>
    public int CurrentStorageSize { get; set; }

    /// <summary>Storage increment size, in bytes.</summary>
    public int IncrementStorageSize { get; set; }

    /// <summary>Number of increments already used.</summary>
    public int NumberOfIncrements { get; set; }

    /// <summary>Maximum number of increments allowed.</summary>
    public int MaximumIncrements { get; set; }

    /// <summary>Severity filter applied to message delivery.</summary>
    public int SeverityCodeFilter { get; set; }

    /// <summary>Delivery method: *HOLD, *BREAK, *NOTIFY or *DFT.</summary>
    public string? DeliveryMethod { get; set; }

    /// <summary>Break handling program, when delivery is *BREAK.</summary>
    public string? BreakHandlingProgramName { get; set; }

    /// <summary>Library of the break handling program.</summary>
    public string? BreakHandlingProgramLibrary { get; set; }

    /// <summary>Force to auxiliary storage: *YES or *NO.</summary>
    public string? ForceToAuxiliaryStorage { get; set; }

    /// <summary>Descriptive text of the queue.</summary>
    public string? TextDescription { get; set; }

    /// <summary>Alerts allowed: 1 for yes, 0 for no.</summary>
    public string? AllowAlerts { get; set; }

    /// <summary>CCSID of the message queue.</summary>
    public int CCSID { get; set; }

    /// <summary>Action when the queue is full: *SNDMSG or *WRAP.</summary>
    public string? MessageQueueFullAction { get; set; }

    /// <summary>Whether other jobs may reply in *BREAK mode.</summary>
    public string? AllowOtherJobsToReply { get; set; }
}
