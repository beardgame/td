using System.Collections.Immutable;

namespace Bearded.TD.Game.Generation.Semantic.Fitness;

static partial class FitnessFunction
{
    public static FitnessFunction<T> Scaled<T>(FitnessFunction<T> function, double scalar) =>
        new ScaledFitnessFunction<T>(function, scalar);

    private sealed class ScaledFitnessFunction<T> : FitnessFunction<T>
    {
        private readonly FitnessFunction<T> function;
        private readonly double scalar;

        public override string Name => $"{function.Name} * {scalar}";

        public ScaledFitnessFunction(FitnessFunction<T> function, double scalar)
        {
            this.function = function;
            this.scalar = scalar;
        }

        public override Fitness<T> FitnessOf(T instance)
        {
            var unscaledFitness = function.FitnessOf(instance);
            return new CompositeFitness<T>(
                this, unscaledFitness.Value * scalar, ImmutableArray.Create(unscaledFitness));
        }
    }
}