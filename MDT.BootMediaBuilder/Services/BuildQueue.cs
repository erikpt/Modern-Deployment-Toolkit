using MDT.Core.Models;
using System.Collections.Concurrent;

namespace MDT.BootMediaBuilder.Services;

/// <summary>
/// Thread-safe queue for managing concurrent boot media builds
/// </summary>
public class BuildQueue
{
    private readonly ConcurrentDictionary<string, BootMediaBuild> _builds = new();
    private readonly SemaphoreSlim _semaphore;
    private readonly int _maxConcurrentBuilds;

    public BuildQueue(int maxConcurrentBuilds)
    {
        _maxConcurrentBuilds = maxConcurrentBuilds;
        _semaphore = new SemaphoreSlim(maxConcurrentBuilds, maxConcurrentBuilds);
    }

    /// <summary>
    /// Add a build to the queue
    /// </summary>
    public void Enqueue(BootMediaBuild build)
    {
        _builds.TryAdd(build.Id, build);
    }

    /// <summary>
    /// Get a build by ID
    /// </summary>
    public BootMediaBuild? GetBuild(string buildId)
    {
        _builds.TryGetValue(buildId, out var build);
        return build;
    }

    /// <summary>
    /// Get all builds
    /// </summary>
    public List<BootMediaBuild> GetAllBuilds()
    {
        return _builds.Values.ToList();
    }

    /// <summary>
    /// Remove a build from the queue
    /// </summary>
    public bool Remove(string buildId)
    {
        return _builds.TryRemove(buildId, out _);
    }

    /// <summary>
    /// Wait for an available build slot
    /// </summary>
    public async Task<bool> WaitForSlotAsync(CancellationToken cancellationToken = default)
    {
        return await _semaphore.WaitAsync(TimeSpan.FromSeconds(1), cancellationToken);
    }

    /// <summary>
    /// Release a build slot
    /// </summary>
    public void ReleaseSlot()
    {
        _semaphore.Release();
    }

    /// <summary>
    /// Get number of currently running builds
    /// </summary>
    public int RunningBuildsCount => _maxConcurrentBuilds - _semaphore.CurrentCount;
}
