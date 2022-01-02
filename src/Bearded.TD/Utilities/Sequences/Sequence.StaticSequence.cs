using System.Collections.Immutable;

namespace Bearded.TD.Utilities.Sequences;

static partial class Sequence
{
    private sealed class StaticSequence<T> : ISequence<T>
    {
        private readonly ImmutableArray<T> sequence;

        public StaticSequence(ImmutableArray<T> sequence)
        {
            this.sequence = sequence;
        }

        public T GetElementAtPosition(int index) => sequence[index];
    }
}