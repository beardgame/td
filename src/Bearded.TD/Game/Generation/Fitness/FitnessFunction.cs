using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace Bearded.TD.Game.Generation.Fitness
{
    static class FitnessFunction
    {
        public static FitnessFunction<T> From<T>(params FitnessFunction<T>[] functions)
        {
            return new CompositeFitnessFunction<T>(functions);
        }

        private sealed class CompositeFitnessFunction<T> : FitnessFunction<T>
        {
            private readonly ImmutableArray<FitnessFunction<T>> functions;

            public CompositeFitnessFunction(FitnessFunction<T>[] functions)
            {
                this.functions = functions.ToImmutableArray();
            }

            public override string Name => "Composite Fitness";
            public override Fitness<T> FitnessOf(T instance)
            {
                var components = functions.Select(f => f.FitnessOf(instance)).ToImmutableArray();
                return new CompositeFitness<T>(this, components.Sum(c => c.Value), components);
            }
        }
        private sealed class CompositeFitness<T> : Fitness<T>
        {
            private readonly ImmutableArray<Fitness<T>> components;

            public CompositeFitness(FitnessFunction<T> function, double value, ImmutableArray<Fitness<T>> components)
                : base(function, value)
            {
                this.components = components;
            }

            private static readonly char[] newLines = {'\r', '\n'};

            public override string ToString()
            {
                var stringBuilder = new StringBuilder();
                stringBuilder.AppendLine($"{Function.Name}: {Value}");
                foreach (var component in components)
                {
                    var componentLines = component.ToString() ?? "";
                    foreach (var line in componentLines.Split(newLines, StringSplitOptions.RemoveEmptyEntries))
                    {
                        stringBuilder.Append("  ");
                        stringBuilder.AppendLine(line);
                    }
                }
                return stringBuilder.ToString();
            }
        }
    }

    abstract class FitnessFunction<T>
    {
        public abstract string Name { get; }

        public abstract Fitness<T> FitnessOf(T instance);

        public static FitnessFunction<T> operator *(FitnessFunction<T> function, double scalar)
            => new ScaledFitnessFunction(function, scalar);

        public static FitnessFunction<T> operator *(double scalar, FitnessFunction<T> function)
            => new ScaledFitnessFunction(function, scalar);

        private sealed class ScaledFitnessFunction : FitnessFunction<T>
        {
            private readonly FitnessFunction<T> function;
            private readonly double scalar;

            public ScaledFitnessFunction(FitnessFunction<T> function, double scalar)
            {
                this.function = function;
                this.scalar = scalar;
            }

            public override string Name => $"{function.Name} * {scalar}";

            public override Fitness<T> FitnessOf(T instance)
            {
                throw new NotImplementedException();
            }
        }
    }
}
