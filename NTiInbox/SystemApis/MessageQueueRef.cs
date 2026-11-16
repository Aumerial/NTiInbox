namespace NTiInbox.SystemApis;

/// <summary>
/// Reference to a message queue: its name and library.
/// Used as a key in the sidebar and as a parameter of every API.
/// </summary>
public sealed class MessageQueueRef : IEquatable<MessageQueueRef>
{
    public MessageQueueRef(string name, string library, string? description = null)
    {
        Name = (name ?? string.Empty).Trim().ToUpperInvariant();
        Library = (library ?? string.Empty).Trim().ToUpperInvariant();
        Description = description?.Trim();
    }

    /// <summary>Queue name, for example QSYSOPR.</summary>
    public string Name { get; }

    /// <summary>Queue library, for example QSYS.</summary>
    public string Library { get; }

    /// <summary>Descriptive text of the object, when known.</summary>
    public string? Description { get; set; }

    /// <summary>Stable key used in the interface.</summary>
    public string Key => $"{Library}/{Name}";

    public override string ToString() => Key;

    public bool Equals(MessageQueueRef? other)
        => other is not null && other.Name == Name && other.Library == Library;

    public override bool Equals(object? obj) => Equals(obj as MessageQueueRef);

    public override int GetHashCode() => HashCode.Combine(Name, Library);
}
