using NTiInbox.SystemApis;

namespace NTiInbox.Messages;

/// <summary>
/// Navigation state of the current circuit: queues listed in the sidebar,
/// selected queue and message counters.
/// </summary>
public sealed class InboxState
{
    private readonly List<MessageQueueRef> _queues = [];
    private readonly Dictionary<string, int> _pendingCounts = [];
    private readonly Dictionary<string, int> _totalCounts = [];

    /// <summary>Queues shown in the sidebar.</summary>
    public IReadOnlyList<MessageQueueRef> Queues => _queues;

    /// <summary>Queue currently open.</summary>
    public MessageQueueRef? CurrentQueue { get; private set; }

    /// <summary>Library used by the last queue search.</summary>
    public string DiscoveryLibrary { get; set; } = "*LIBL";

    /// <summary>Raised when the queue list, the selection or a counter changes.</summary>
    public event Action? Changed;

    /// <summary>Replaces the queue list and keeps the selection if it still exists.</summary>
    public void SetQueues(IEnumerable<MessageQueueRef> queues)
    {
        _queues.Clear();
        _queues.AddRange(queues.OrderBy(q => q.Name, StringComparer.OrdinalIgnoreCase));

        if (CurrentQueue is not null && !_queues.Contains(CurrentQueue))
        {
            CurrentQueue = null;
        }

        CurrentQueue ??= _queues.FirstOrDefault();
        Changed?.Invoke();
    }

    /// <summary>Adds a queue typed by hand, without duplicate, and opens it.</summary>
    public void AddQueue(MessageQueueRef queue)
    {
        if (!_queues.Contains(queue))
        {
            _queues.Add(queue);
            _queues.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase));
        }

        CurrentQueue = _queues.First(q => q.Equals(queue));
        Changed?.Invoke();
    }

    /// <summary>Opens a queue.</summary>
    public void SelectQueue(MessageQueueRef? queue)
    {
        CurrentQueue = queue;
        Changed?.Invoke();
    }

    /// <summary>Stores the counters shown beside the queue name.</summary>
    public void SetCounts(MessageQueueRef queue, int total, int pending)
    {
        _totalCounts[queue.Key] = total;
        _pendingCounts[queue.Key] = pending;
        Changed?.Invoke();
    }

    /// <summary>Known message count of a queue, or null if it was never read.</summary>
    public int? TotalCount(MessageQueueRef queue)
        => _totalCounts.TryGetValue(queue.Key, out var value) ? value : null;

    /// <summary>Messages waiting for a reply, or null if the queue was never read.</summary>
    public int? PendingCount(MessageQueueRef queue)
        => _pendingCounts.TryGetValue(queue.Key, out var value) ? value : null;

    /// <summary>Clears the state, on disconnection.</summary>
    public void Reset()
    {
        _queues.Clear();
        _totalCounts.Clear();
        _pendingCounts.Clear();
        CurrentQueue = null;
        Changed?.Invoke();
    }
}
