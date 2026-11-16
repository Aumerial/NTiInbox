namespace NTiInbox.Messages;

/// <summary>Filter on the send date.</summary>
public enum PeriodFilter
{
    /// <summary>No filter.</summary>
    All,

    /// <summary>Today's messages.</summary>
    Today,

    /// <summary>Messages of the last seven days.</summary>
    Week,

    /// <summary>Messages of the last thirty days.</summary>
    Month
}
