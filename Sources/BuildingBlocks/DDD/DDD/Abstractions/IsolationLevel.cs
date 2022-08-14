namespace Pulsar.BuildingBlocks.DDD.Abstractions;

public enum IsolationLevel
{
    /// <summary>
    /// can read uncommitted data (i.e. can be rolled back)
    /// </summary>
    Uncommitted = 0,
    /// <summary>
    /// can read uncommitted data and (possibly) stale data
    /// </summary>
    UncommittedStale,
    /// <summary>
    /// can read uncommitted data and (probably) stale data but offers the lowest latency
    /// </summary>
    UncommittedNearest,
    /// <summary>
    /// only read majority committed data
    /// </summary>
    Committed,
    /// <summary>
    /// only read majority committed data and (possibly) stale data
    /// </summary>
    CommittedStale,
    /// <summary>
    /// only read majority committed data and (probably) stale data but offers the lowest latency
    /// </summary>
    CommittedNearest,
    /// <summary>
    /// data is read as if being read from memory (in real time)
    /// </summary>
    Linearizable,
    /// <summary>
    /// offers snapshot isolation
    /// </summary>
    Snapshot
}
