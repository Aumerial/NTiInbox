using Aumerial.Data.Nti;

namespace NTiInbox.SystemApis;

/// <summary>Retrieves the attributes of a message queue (QMHRMQAT).</summary>
public static class QueueAttributesApi
{
    /// <summary>
    /// Retrieves the attributes of a message queue through the QMHRMQAT API (RMQA0100 format).
    /// </summary>
    /// <param name="messageQueueName">Message queue name, on 10 characters.</param>
    /// <param name="libraryName">Queue library, on 10 characters.</param>
    /// <returns>The attributes of the queue.</returns>
    public static QueueAttributes RetrieveMessageQueueAttributes(this NTiConnection conn,
        string messageQueueName,
        string libraryName)
    {
        conn.EnsureOpen();

        string formatName = "RMQA0100";

        var parameters = new List<NTiProgramParameter>
        {
            new NTiProgramParameter(new byte[160]).AsOutput(),
            new NTiProgramParameter(160),
            new NTiProgramParameter(formatName, 8),
            new NTiProgramParameter(messageQueueName, 10).Append(libraryName, 10),
            ApiErrorCode.Create()
        };

        conn.CallProgram("QSYS", "QMHRMQAT", parameters);

        ApiErrorCode.ThrowIfError(parameters[4], "QMHRMQAT");

        var listOfParameters = parameters[0];

        var messageQueueAttributes = new QueueAttributes
        {
            MessageQueueName = listOfParameters.GetString(8, 10),
            MessageQueueLibrary = listOfParameters.GetString(18, 10),
            NumberOfMessagesOnQueue = listOfParameters.GetInt(28),
            CurrentStorageSize = listOfParameters.GetInt(32),
            IncrementStorageSize = listOfParameters.GetInt(36),
            NumberOfIncrements = listOfParameters.GetInt(40),
            MaximumIncrements = listOfParameters.GetInt(44),
            SeverityCodeFilter = listOfParameters.GetInt(48),
            DeliveryMethod = listOfParameters.GetString(52, 7),
            BreakHandlingProgramName = listOfParameters.GetString(59, 10),
            BreakHandlingProgramLibrary = listOfParameters.GetString(69, 10),
            ForceToAuxiliaryStorage = listOfParameters.GetString(79, 4),
            TextDescription = listOfParameters.GetString(83, 50),
            AllowAlerts = listOfParameters.GetString(133, 1),
            CCSID = listOfParameters.GetInt(136),
            MessageQueueFullAction = listOfParameters.GetString(140, 10),
            AllowOtherJobsToReply = listOfParameters.GetString(150, 10),
        };

        return messageQueueAttributes;
    }
}
