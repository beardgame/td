namespace Bearded.TD.Game.Generation.Fitness
{
    sealed class FitnessComponent<T>
    {
        public double Value { get; }

        public FitnessFunction<T> Function { get; }

        public override string ToString()
        {
            return $"- {Function.Name}: {Value}";
        }
    }
}
