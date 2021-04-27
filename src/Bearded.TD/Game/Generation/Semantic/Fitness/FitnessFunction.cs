namespace Bearded.TD.Game.Generation.Semantic.Fitness
{
    abstract class FitnessFunction<T>
    {
        public abstract string Name { get; }
        public abstract Fitness<T> FitnessOf(T instance);

        public static FitnessFunction<T> operator *(FitnessFunction<T> function, double scalar) =>
            FitnessFunction.Scaled(function, scalar);

        public static FitnessFunction<T> operator *(double scalar, FitnessFunction<T> function) =>
            FitnessFunction.Scaled(function, scalar);
    }

    abstract class SimpleFitnessFunction<T> : FitnessFunction<T>
    {
        public sealed override Fitness<T> FitnessOf(T instance) => new(this, CalculateFitness(instance));
        protected abstract double CalculateFitness(T instance);
    }
}
