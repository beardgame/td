namespace Bearded.TD.Game.Generation.Fitness
{
    abstract class FitnessComponent<T>
    {
        public double Value { get; }

        protected abstract string Name { get; }


        protected FitnessComponent(T instance)
        {
            Value = CalculateFitness(instance);
        }

        protected abstract double CalculateFitness(T instance);

        public override string ToString()
        {
            return $"- {Name}: {Value}";
        }
    }
}
