namespace NTiInbox.SystemApis;

/// <summary>
/// Job information, as returned by QUSRJOBI (JOBI0200 format).
/// </summary>
public class JobInformation
{
    /// <summary>Job name.</summary>
    public string? JobName { get; set; }

    /// <summary>User tied to the job.</summary>
    public string? UserName { get; set; }

    /// <summary>Job number.</summary>
    public string? JobNumber { get; set; }

    /// <summary>Internal job identifier.</summary>
    public string? InternalJobIdentifier { get; set; }

    /// <summary>Job status: *ACTIVE, *JOBQ or *OUTQ.</summary>
    public string? JobStatus { get; set; }

    /// <summary>One-character job type: B for batch, I for interactive...</summary>
    public string? JobType { get; set; }

    /// <summary>One-character job subtype.</summary>
    public string? JobSubtype { get; set; }

    /// <summary>Name of the subsystem description the job runs in.</summary>
    public string? SubsystemDescriptionName { get; set; }

    /// <summary>Run priority of the job.</summary>
    public int RunPriority { get; set; }

    /// <summary>Id of the system pool.</summary>
    public int SystemPoolIdentifier { get; set; }

    /// <summary>CPU time used, in milliseconds.</summary>
    public int ProcessingUnitTimeUsed { get; set; }

    /// <summary>Number of auxiliary I/O operations.</summary>
    public int NumberOfAuxiliaryIORequests { get; set; }

    /// <summary>Number of interactive transactions.</summary>
    public int NumberOfInteractiveTransactions { get; set; }

    /// <summary>Cumulative response time.</summary>
    public int ResponseTimeTotal { get; set; }

    /// <summary>Type of the current function: C for CL command, P for program...</summary>
    public string? FunctionType { get; set; }

    /// <summary>Name of the function being run.</summary>
    public string? FunctionName { get; set; }

    /// <summary>Detailed status of an active job: RUN, MSGW, DEQW, LCKW...</summary>
    public string? ActiveJobStatus { get; set; }

    /// <summary>Number of database lock waits.</summary>
    public int NumberOfDatabaseLockWaits { get; set; }

    /// <summary>Number of internal machine lock waits.</summary>
    public int NumberOfInternalMachineLockWaits { get; set; }

    /// <summary>Number of non-database lock waits.</summary>
    public int NumberOfNondatabaseLockWaits { get; set; }

    /// <summary>Time spent waiting for database locks, in milliseconds.</summary>
    public int TimeSpentOnDatabaseLockWaits { get; set; }

    /// <summary>Time spent waiting for internal machine locks, in milliseconds.</summary>
    public int TimeSpentOnInternalMachineLockWaits { get; set; }

    /// <summary>Time spent waiting for non-database locks, in milliseconds.</summary>
    public int TimeSpentOnNondatabaseLockWaits { get; set; }

    /// <summary>Id of the current system pool.</summary>
    public int CurrentSystemPoolIdentifier { get; set; }

    /// <summary>Number of threads of the job.</summary>
    public int ThreadCount { get; set; }

    /// <summary>Total CPU time used.</summary>
    public long ProcessingUnitTimeUsedTotal { get; set; }

    /// <summary>Total number of auxiliary I/O operations.</summary>
    public long NumberOfAuxiliaryIORequestsTotal { get; set; }

    /// <summary>Total CPU time used by the database.</summary>
    public long ProcessingUnitTimeUsedForDatabaseTotal { get; set; }

    /// <summary>Number of page faults.</summary>
    public long PageFaults { get; set; }

    /// <summary>Status of a job that is ending.</summary>
    public string? ActiveJobStatusForJobsEnding { get; set; }

    /// <summary>Name of the memory pool of the job.</summary>
    public string? MemoryPoolName { get; set; }

    /// <summary>Whether the job waits for the reply to a message.</summary>
    public string? MessageReply { get; set; }

    /// <summary>Key of the message waiting for a reply.</summary>
    public string? MessageKey { get; set; }

    /// <summary>Message queue of the job.</summary>
    public string? MessageQueueName { get; set; }

    /// <summary>Library of the job message queue.</summary>
    public string? MessageQueueLibraryName { get; set; }

    /// <summary>ASP unit of the message queue library.</summary>
    public string? MessageQueueLibraryASPDeviceName { get; set; }

    /// <summary>Number of reuses of a prestart job.</summary>
    public int PrestartJobReuseCount { get; set; }

    /// <summary>Maximum number of uses of a prestart job.</summary>
    public int PrestartJobMaximumNumberOfUses { get; set; }
}
