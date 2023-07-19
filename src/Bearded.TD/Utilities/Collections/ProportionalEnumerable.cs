using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using Bearded.Utilities.Collections;

namespace Bearded.TD.Utilities.Collections;

sealed class ProportionalEnumerable<T> : IEnumerable<T>
{
    private readonly IReadOnlyDictionary<T, int> targetCounts;
    private readonly IReadOnlyDictionary<T, int> initialCounts;

    public ProportionalEnumerable(IReadOnlyDictionary<T, int> targetCounts)
        : this(targetCounts, ImmutableDictionary<T, int>.Empty) { }

    public ProportionalEnumerable(IReadOnlyDictionary<T, int> targetCounts, IReadOnlyDictionary<T, int> initialCounts)
    {
        this.targetCounts = targetCounts;
        this.initialCounts = initialCounts;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public IEnumerator<T> GetEnumerator() => new ProportionalEnumerator(targetCounts, initialCounts);

    private sealed class ProportionalEnumerator : IEnumerator<T>
    {
        private readonly IReadOnlyDictionary<T, int> targetCounts;
        private readonly IReadOnlyDictionary<T, int> initialCounts;
        private readonly Dictionary<T, int> actualCounts = new();
        private readonly StaticPriorityQueue<ProportionalEnumeratorKey, T> priorityQueue = new();

        public T Current { get; private set; }

        object IEnumerator.Current => Current;

        public ProportionalEnumerator(
            IReadOnlyDictionary<T, int> targetCounts, IReadOnlyDictionary<T, int> initialCounts)
        {
            this.targetCounts = targetCounts;
            this.initialCounts = initialCounts;
            Reset();
        }

        public void Reset()
        {
            actualCounts.Clear();
            priorityQueue.Clear();
            foreach (var (obj, targetCount) in targetCounts)
            {
                var initialCount = initialCounts.GetValueOrDefault(obj, 0);
                actualCounts[obj] = initialCount;
                if (initialCount >= targetCount)
                {
                    continue;
                }
                var priority = new ProportionalEnumeratorKey(targetCount, targetCount);
                priorityQueue.Enqueue(priority, obj);
            }

            Current = default;
        }

        public bool MoveNext()
        {
            if (priorityQueue.Count == 0)
            {
                return false;
            }

            var (priority, obj) = priorityQueue.Dequeue();
            Current = obj;
            var currentCount = actualCounts.GetValueOrDefault(obj, 0);
            var newCount = currentCount + 1;
            actualCounts[obj] = newCount;

            if (newCount >= priority.TotalRequested)
            {
                return true;
            }

            var newPriority = priority with { Ratio = priority.TotalRequested / (1.0 + newCount) };
            priorityQueue.Enqueue(newPriority, obj);

            return true;
        }

        public void Dispose() { }
    }

    private readonly record struct ProportionalEnumeratorKey(double Ratio, double TotalRequested)
        : IComparable<ProportionalEnumeratorKey>
    {
        public int CompareTo(ProportionalEnumeratorKey other)
        {
            var ratioComparison = Ratio.CompareTo(other.Ratio);
            if (ratioComparison != 0)
            {
                return -ratioComparison;
            }

            return TotalRequested.CompareTo(other.TotalRequested);
        }
    }
}
