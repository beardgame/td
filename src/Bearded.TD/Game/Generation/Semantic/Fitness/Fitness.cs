using System;

namespace Bearded.TD.Game.Generation.Semantic.Fitness;

record Fitness<T>(FitnessFunction<T> Function, double Value) : IComparable<Fitness<T>>
{
    public override string ToString() => $"- {Function.Name}: {Value}";

    public int CompareTo(Fitness<T>? other) => ReferenceEquals(null, other) ? 1 : Value.CompareTo(other.Value);
}