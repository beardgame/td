namespace Bearded.TD.Shared.TechEffects
{
    public struct Modification
    {
        public static Modification AddConstant(double constant)
            => new Modification(ModificationType.Additive, constant);
        public static Modification AddFractionOfBase(double fraction)
            => new Modification(ModificationType.Multiplicative, fraction);

        public enum ModificationType : byte
        {
            Additive,
            Multiplicative
        }

        public ModificationType Type { get; }
        public double Value { get; }

        private Modification(ModificationType type, double value)
        {
            Type = type;
            Value = value;
        }
    }
}
