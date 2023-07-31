using System;
using System.Collections.Generic;

namespace Bearded.TD.Game.Simulation.Events;

sealed class Accumulator<T>
{
    private readonly List<T> objects = new();

    public void Contribute(T obj)
    {
        objects.Add(obj);
    }

    public TOut Consume<TOut>(Func<IEnumerable<T>, TOut> consumer)
    {
        return consumer(objects);
    }
}
