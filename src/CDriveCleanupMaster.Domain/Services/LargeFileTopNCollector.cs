using CDriveCleanupMaster.Domain.Models;

namespace CDriveCleanupMaster.Domain.Services;

/// <summary>
/// 线程安全的大文件 Top-N 收集器，扫描过程中仅保留最大的 N 个文件。
/// </summary>
public sealed class LargeFileTopNCollector
{
    private readonly int _capacity;
    private readonly object _sync = new();
    private readonly PriorityQueue<LargeFileItem, long> _heap = new();
    private long _totalMatched;

    public LargeFileTopNCollector(int capacity)
    {
        _capacity = Math.Max(1, capacity);
    }

    public long TotalMatched => Interlocked.Read(ref _totalMatched);

    public int Count
    {
        get
        {
            lock (_sync)
            {
                return _heap.Count;
            }
        }
    }

    public void TryAdd(LargeFileItem item)
    {
        Interlocked.Increment(ref _totalMatched);

        lock (_sync)
        {
            if (_heap.Count < _capacity)
            {
                _heap.Enqueue(item, item.Bytes);
                return;
            }

            _heap.TryPeek(out _, out long smallestBytes);
            if (item.Bytes <= smallestBytes)
            {
                return;
            }

            _heap.Dequeue();
            _heap.Enqueue(item, item.Bytes);
        }
    }

    public LargeFileItem[] ToOrderedArray()
    {
        lock (_sync)
        {
            return _heap.UnorderedItems
                .Select(entry => entry.Element)
                .OrderByDescending(item => item.Bytes)
                .ToArray();
        }
    }
}
