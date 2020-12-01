using System;

namespace Bearded.TD.Utilities.Sequences
{
    static partial class Sequence
    {
        private sealed class StreamSequence<T> : ISequence<T>
        {
            private readonly Func<int, T> func;

            public StreamSequence(Func<int,T> func)
            {
                this.func = func;
            }

            public T GetElementAtPosition(int index) => func(index);
        }
    }
}
