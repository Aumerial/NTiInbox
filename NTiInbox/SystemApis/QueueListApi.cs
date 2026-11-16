using Aumerial.Data.Nti;

namespace NTiInbox.SystemApis;

/// <summary>Lists the message queues of a library (QUSLOBJ).</summary>
public static class QueueListApi
{
    /// <summary>
    /// Lists IBM i objects through the QUSLOBJ API (OBJL0200 format).
    /// Same principle as QMHLSTM: the data goes through a user space in QTEMP.
    /// This API is not part of messaging, it only fills the sidebar with the
    /// accessible message queues.
    /// </summary>
    /// <param name="objectName">Object name searched, or *ALL.</param>
    /// <param name="objectLibrary">Library, or a special value such as *LIBL, *ALLUSR, *CURLIB.</param>
    /// <param name="objectType">Object type searched, for example *MSGQ.</param>
    /// <returns>The objects found.</returns>
    public static List<QueueListEntry> ListObjects(this NTiConnection conn,
        string objectName,
        string objectLibrary,
        string objectType)
    {
        conn.EnsureOpen();

        int initialSize = 65535;
        bool spaceAllocated = false;
        var entries = new List<QueueListEntry>();

        try
        {
            while (!spaceAllocated)
            {
                var initialParameters = new List<NTiProgramParameter>
                {
                    new NTiProgramParameter("NTILOBJLI", 10).Append("QTEMP", 10),
                    new NTiProgramParameter("QUSLOBJ", 10),
                    new NTiProgramParameter(initialSize),
                    new NTiProgramParameter(new byte[] { 0x00 }),
                    new NTiProgramParameter("*ALL", 10),
                    new NTiProgramParameter("List Objects userspace", 50)
                };

                conn.CallProgram("QSYS", "QUSCRTUS", initialParameters);

                var parameters = new List<NTiProgramParameter>
                {
                    new NTiProgramParameter("NTILOBJLI", 10).Append("QTEMP", 10),
                    new NTiProgramParameter("OBJL0200", 8),
                    new NTiProgramParameter(objectName, 10).Append(objectLibrary, 10),
                    new NTiProgramParameter(objectType, 10),
                    ApiErrorCode.Create()
                };

                conn.CallProgram("QSYS", "QUSLOBJ", parameters);

                ApiErrorCode.ThrowIfError(parameters[4], "QUSLOBJ");

                var finalParameters = new List<NTiProgramParameter>
                {
                    new NTiProgramParameter("NTILOBJLI", 10).Append("QTEMP", 10),
                    new NTiProgramParameter(1),
                    new NTiProgramParameter(initialSize),
                    new NTiProgramParameter("", initialSize).AsOutput()
                };

                conn.CallProgram("QSYS", "QUSRTVUS", finalParameters);

                var userSpace = finalParameters[3];

                var offsetToData = userSpace.GetInt(0x7C);
                var listSize = userSpace.GetInt(0x80);
                var numberOfEntries = userSpace.GetInt(0x84);
                var entrySize = userSpace.GetInt(0x88);

                int totalCalculatedSize = listSize + offsetToData;

                if (totalCalculatedSize > initialSize)
                {
                    conn.ExecuteClCommand("DLTUSRSPC QTEMP/NTILOBJLI");
                    initialSize = totalCalculatedSize;
                }
                else
                {
                    spaceAllocated = true;

                    for (int i = 0; i < numberOfEntries; i++)
                    {
                        int currentOffset = offsetToData + (i * entrySize);

                        entries.Add(new QueueListEntry
                        {
                            ObjectName = userSpace.GetString(currentOffset, 10).Trim(),
                            ObjectLibrary = userSpace.GetString(currentOffset + 10, 10).Trim(),
                            ObjectType = userSpace.GetString(currentOffset + 20, 10).Trim(),
                            InformationStatus = userSpace.GetString(currentOffset + 30, 1).Trim(),
                            ExtendedAttribute = userSpace.GetString(currentOffset + 31, 10).Trim(),
                            TextDescription = userSpace.GetString(currentOffset + 41, 50).Trim()
                        });
                    }
                }
            }
        }
        finally
        {
            try
            {
                conn.ExecuteClCommand("DLTUSRSPC QTEMP/NTILOBJLI");
            }
            catch
            {
                // The space does not exist, there is nothing to clean up.
            }
        }

        return entries;
    }
}
