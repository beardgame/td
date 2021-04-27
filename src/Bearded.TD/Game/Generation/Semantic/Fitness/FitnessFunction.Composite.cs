using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace Bearded.TD.Game.Generation.Semantic.Fitness
{
    static partial class FitnessFunction
    {
        public static FitnessFunction<T> From<T>(params FitnessFunction<T>[] functions)
        {
            return new CompositeFitnessFunction<T>(functions);
        }

        private sealed class CompositeFitnessFunction<T> : FitnessFunction<T>
        {
            private readonly ImmutableArray<FitnessFunction<T>> functions;

            public override string Name => "Composite Fitness";

            public CompositeFitnessFunction(IEnumerable<FitnessFunction<T>> functions)
            {
                this.functions = functions.ToImmutableArray();
            }

            public override Fitness<T> FitnessOf(T instance)
            {
                var components = functions.Select(f => f.FitnessOf(instance)).ToImmutableArray();
                return new CompositeFitness<T>(this, components.Sum(c => c.Value), components);
            }
        }

        private sealed record CompositeFitness<T>(
                FitnessFunction<T> Function, double Value, ImmutableArray<Fitness<T>> Components)
            : Fitness<T>(Function, Value)
        {
            // ReSharper disable once StaticMemberInGenericType
            private static readonly char[] newLines = {'\r', '\n'};

            public override string ToString()
            {
                var stringBuilder = new StringBuilder();
                stringBuilder.AppendLine($"{Function.Name}: {Value}");
                foreach (var component in Components)
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
}
