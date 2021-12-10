using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Bearded.TD.Utilities.Sequences;

static partial class Sequence
{
    public static ISequence<T> Constant<T>(T constant) => Infinite(_ => constant);

    public static ISequence<T> Finite<T>(IEnumerable<T> enumerable) =>
        new StaticSequence<T>(enumerable.ToImmutableArray());

    public static ISequence<T> Infinite<T>(Func<int, T> func) => new StreamSequence<T>(func);
}