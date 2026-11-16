using Aumerial.Data.Nti;

namespace NTiInbox.SystemApis;

/// <summary>Creation and reading of the temporary user spaces that carry list results.</summary>
internal static class UserSpaceApi
{
    /// <summary>Creates the temporary user space that receives the list.</summary>
    internal static void CreateListUserSpace(NTiConnection conn, int size)
    {
        var parameters = new List<NTiProgramParameter>
        {
            new NTiProgramParameter("NTILMSGLI", 10).Append("QTEMP", 10),
            new NTiProgramParameter("QMHLSTM", 10),
            new NTiProgramParameter(size),
            new NTiProgramParameter(new byte[] { 0x00 }),
            new NTiProgramParameter("*ALL", 10),
            new NTiProgramParameter("List Message Queue userspace", 50)
        };

        conn.CallProgram("QSYS", "QUSCRTUS", parameters);
    }

    /// <summary>Rereads a portion of the user space.</summary>
    internal static NTiProgramParameter RetrieveUserSpace(NTiConnection conn, string spaceName, int startingPosition, int length)
    {
        var parameters = new List<NTiProgramParameter>
        {
            new NTiProgramParameter(spaceName, 10).Append("QTEMP", 10),
            new NTiProgramParameter(startingPosition),
            new NTiProgramParameter(length),
            new NTiProgramParameter("", length).AsOutput()
        };

        conn.CallProgram("QSYS", "QUSRTVUS", parameters);

        return parameters[3];
    }
}
