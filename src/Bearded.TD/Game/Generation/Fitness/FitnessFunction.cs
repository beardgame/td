using System;

namespace Bearded.TD.Game.Generation.Fitness
{
    abstract class FitnessFunction<T>
    {
        public abstract string Name { get; }

        public abstract Fitness<T> FitnessOf(T instance);

        public static FitnessFunction<T> operator *(FitnessFunction<T> function, double scalar) => scalar * function;
        public static FitnessFunction<T> operator *( double scalar, FitnessFunction<T> function)
        {
            return new ScaledFitnessFunction(function, scalar);
        }

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
