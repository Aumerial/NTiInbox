using System.Data;
using Aumerial.Data.Nti;

namespace NTiInbox.SystemApis;

/// <summary>Helpers shared by every system call.</summary>
public static class NTiConnectionExtensions
{
    /// <summary>
    /// Opens the connection when it is not open yet. NTi 5 throws if <c>Open()</c> is
    /// called on an open connection, so the state is tested first.
    /// </summary>
    public static NTiConnection EnsureOpen(this NTiConnection conn)
    {
        if (conn.State != ConnectionState.Open)
        {
            conn.Open();
        }

        return conn;
    }
}
