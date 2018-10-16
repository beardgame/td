namespace Bearded.TD.Shared.TechEffects
{
    struct Modification
    {
        public enum ModificationType : byte
        {
            Additive,
            Multiplicative
        }

        public ModificationType Type { get; }
        public double Value { get; }

        public Modification(ModificationType type, double value)
        {
            Type = type;
            Value = value;
        }
    }
}
