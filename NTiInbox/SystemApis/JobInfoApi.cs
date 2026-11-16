using Aumerial.Data.Nti;

namespace NTiInbox.SystemApis;

/// <summary>Retrieves the information of a job (QUSRJOBI).</summary>
public static class JobInfoApi
{
    /// <summary>
    /// Retrieves job information through the QUSRJOBI API (JOBI0200 format).
    /// Answers the question a message raises: who sent it, and what was this job
    /// doing at that moment.
    /// </summary>
    /// <param name="jobName">Job name, 10 characters at most.</param>
    /// <param name="userName">Job user, 10 characters at most.</param>
    /// <param name="jobNumber">Job number, 6 characters.</param>
    /// <param name="internalJobIdentifier">
    /// Internal job identifier, in hexadecimal. When given, it takes precedence over the
    /// qualified name and the call uses the special value *INT.
    /// </param>
    /// <returns>The job information.</returns>
    public static JobInformation RetrieveJobInformation(this NTiConnection conn,
        string jobName,
        string userName,
        string jobNumber,
        string internalJobIdentifier = "")
    {
        conn.EnsureOpen();

        var parameters = new List<NTiProgramParameter>
        {
            new NTiProgramParameter(new byte[236]),
            new NTiProgramParameter(236),
            new NTiProgramParameter("JOBI0200", 8),
        };

        if (!string.IsNullOrEmpty(internalJobIdentifier))
        {
            parameters.Add(new NTiProgramParameter("*INT", 10).Append("", 10).Append("", 10));
            parameters.Add(new NTiProgramParameter(HexToBytes(internalJobIdentifier)));
        }
        else
        {
            parameters.Add(new NTiProgramParameter(jobName, 10).Append(userName, 10).Append(jobNumber, 10));
            parameters.Add(new NTiProgramParameter("", 16));
        }

        // Error in ERRC0100 format
        var errorBuffer = new byte[256];
        errorBuffer[0] = 0x00;
        errorBuffer[1] = 0x00;
        errorBuffer[2] = 0x01;
        errorBuffer[3] = 0x00;

        var errorCode = new NTiProgramParameter(errorBuffer).AsInputOutput();
        parameters.Add(errorCode);

        conn.CallProgram("QSYS", "QUSRJOBI", parameters);

        int errorBytesAvailable = errorCode.GetInt(4);

        if (errorBytesAvailable > 0)
        {
            throw new IbmiApiException("QUSRJOBI", errorCode.GetString(8, 7).Trim());
        }

        var listOfParameters = parameters[0];
        var bytesReturned = listOfParameters.GetInt(0);
        var bytesAvailable = listOfParameters.GetInt(4);

        var jobInfos = new JobInformation
        {
            JobName = listOfParameters.GetString(8, 10),
            UserName = listOfParameters.GetString(18, 10),
            JobNumber = listOfParameters.GetString(28, 6),
            JobStatus = listOfParameters.GetString(50, 10),
            JobType = listOfParameters.GetString(60, 1),
            JobSubtype = listOfParameters.GetString(61, 1),
            SubsystemDescriptionName = listOfParameters.GetString(62, 10),
            RunPriority = listOfParameters.GetInt(72),
            SystemPoolIdentifier = listOfParameters.GetInt(76),
            ProcessingUnitTimeUsed = listOfParameters.GetInt(80),
            NumberOfAuxiliaryIORequests = listOfParameters.GetInt(84),
            NumberOfInteractiveTransactions = listOfParameters.GetInt(88),
            ResponseTimeTotal = listOfParameters.GetInt(92),
            FunctionType = listOfParameters.GetString(96, 1),
            FunctionName = listOfParameters.GetString(97, 10),
            ActiveJobStatus = listOfParameters.GetString(107, 4),
            NumberOfDatabaseLockWaits = listOfParameters.GetInt(111),
            NumberOfInternalMachineLockWaits = listOfParameters.GetInt(115),
            NumberOfNondatabaseLockWaits = listOfParameters.GetInt(119),
            TimeSpentOnDatabaseLockWaits = listOfParameters.GetInt(123),
            TimeSpentOnInternalMachineLockWaits = listOfParameters.GetInt(127),
            TimeSpentOnNondatabaseLockWaits = listOfParameters.GetInt(131),
            CurrentSystemPoolIdentifier = listOfParameters.GetInt(136),
            ThreadCount = listOfParameters.GetInt(140),
            ProcessingUnitTimeUsedTotal = listOfParameters.GetInt(144),
            NumberOfAuxiliaryIORequestsTotal = listOfParameters.GetInt(152),
            ProcessingUnitTimeUsedForDatabaseTotal = listOfParameters.GetInt(160),
            PageFaults = listOfParameters.GetInt(168),
            ActiveJobStatusForJobsEnding = listOfParameters.GetString(176, 4),
            MemoryPoolName = listOfParameters.GetString(180, 10),
            MessageReply = listOfParameters.GetString(190, 1),
            MessageKey = listOfParameters.GetString(191, 4),
            MessageQueueName = listOfParameters.GetString(195, 10),
            MessageQueueLibraryName = listOfParameters.GetString(205, 10),
            MessageQueueLibraryASPDeviceName = listOfParameters.GetString(215, 10),
            PrestartJobReuseCount = listOfParameters.GetInt(228),
            PrestartJobMaximumNumberOfUses = listOfParameters.GetInt(232),
        };

        return jobInfos;
    }

    /// <summary>
    /// Converts a hexadecimal string into a byte array. The internal job identifier is
    /// produced in this form by job listings.
    /// </summary>
    private static byte[] HexToBytes(string hex)
    {
        var value = (hex ?? string.Empty).Replace("-", string.Empty).Trim();
        var bytes = new byte[value.Length / 2];

        for (int i = 0; i < bytes.Length; i++)
        {
            bytes[i] = Convert.ToByte(value.Substring(i * 2, 2), 16);
        }

        return bytes;
    }
}
