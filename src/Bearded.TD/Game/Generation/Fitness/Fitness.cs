using System;

namespace Bearded.TD.Game.Generation.Fitness
{
    class Fitness<T> : IComparable<Fitness<T>>
    {
        public double Value { get; }

        public FitnessFunction<T> Function { get; }

        public Fitness(FitnessFunction<T> function, double value)
        {
            Function = function;
            Value = value;
        }

        public override string ToString()
        {
            return $"- {Function.Name}: {Value}";
        }

        public int CompareTo(Fitness<T>? other)
        {
            return other == null ? 1 : Value.CompareTo(other.Value);
        }
    }
}
