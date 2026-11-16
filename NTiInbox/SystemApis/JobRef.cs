namespace NTiInbox.SystemApis;

/// <summary>
/// Identity of a job: name, user and number, as extracted from the sender job
/// field of a message.
/// </summary>
public sealed class JobRef
{
    public JobRef(string name, string user, string number)
    {
        Name = (name ?? string.Empty).Trim().ToUpperInvariant();
        User = (user ?? string.Empty).Trim().ToUpperInvariant();
        Number = (number ?? string.Empty).Trim();
    }

    /// <summary>Job name.</summary>
    public string Name { get; }

    /// <summary>User of the job.</summary>
    public string User { get; }

    /// <summary>Job number.</summary>
    public string Number { get; }

    /// <summary>Full qualified form, as on a 5250 screen.</summary>
    public string Qualified => $"{Number}/{User}/{Name}";

    /// <summary>True when all three parts are filled in.</summary>
    public bool IsComplete => Name.Length > 0 && User.Length > 0 && Number.Length > 0;

    public override string ToString() => Qualified;
}
