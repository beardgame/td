using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace Bearded.TD.Game.Generation.Fitness
{
    sealed class Fitness<T>
    {
        private readonly ImmutableArray<FitnessComponent<T>> components;
        public double Value { get; }

        private Fitness(IEnumerable<FitnessComponent<T>> components)
        {
            this.components = components.ToImmutableArray();
            Value = this.components.Sum(c => c.Value);
        }

        public static Fitness<T> From(params FitnessComponent<T>[] components)
        {
            return new(components);
        }

        private static readonly char[] newLines = {'\r', '\n'};

        public override string ToString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"Fitness: {Value}");
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
