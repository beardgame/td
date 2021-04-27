using System;

namespace Bearded.TD.Game.Generation.Semantic.Fitness
{
    static partial class FitnessFunction
    {
        public static FitnessFunction<T> From<T>(Func<T, double> function, string name)
        {
            return new LambdaFitnessFunction<T>(function, name);
        }

        private sealed class LambdaFitnessFunction<T> : SimpleFitnessFunction<T>
        {
            private readonly Func<T, double> function;
            public override string Name { get; }

            public LambdaFitnessFunction(Func<T, double> function, string name)
            {
                this.function = function;
                Name = name;
            }

            protected override double CalculateFitness(T instance)
            {
                return function(instance);
            }
        }
    }
}
